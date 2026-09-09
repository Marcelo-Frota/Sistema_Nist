using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NistCsfMaturitySystem.Models;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace NistCsfMaturitySystem.Data
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        // Em um cenário real, você injetaria IHttpContextAccessor para obter o UsuarioId e IP
        // Para simplificação, estamos deixando nulo ou fixo, mas a estrutura está preparada.
        
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            BeforeSaveChanges(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            BeforeSaveChanges(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void BeforeSaveChanges(DbContext context)
        {
            if (context == null) return;

            var auditEntries = new List<AuditoriaLog>();
            var entries = context.ChangeTracker.Entries().Where(e => e.Entity is not AuditoriaLog && e.State != EntityState.Unchanged && e.State != EntityState.Detached).ToList();

            foreach (var entry in entries)
            {
                var auditLog = new AuditoriaLog
                {
                    Modulo = entry.Entity.GetType().Name,
                    EntidadeAfetada = entry.Entity.GetType().Name,
                    Acao = entry.State.ToString(),
                    // UsuarioId = TODO: Obter do Contexto de Claims
                };

                var oldValues = new Dictionary<string, object>();
                var newValues = new Dictionary<string, object>();

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) continue; // Pular PKs geradas no banco

                    string propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                if (oldValues.Count > 0)
                    auditLog.ValoresAnteriores = JsonSerializer.Serialize(oldValues);
                if (newValues.Count > 0)
                    auditLog.NovosValores = JsonSerializer.Serialize(newValues);

                auditEntries.Add(auditLog);
            }

            if (auditEntries.Count > 0)
            {
                context.Set<AuditoriaLog>().AddRange(auditEntries);
            }
        }
    }
}
