using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;
using NistCsfMaturitySystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NistCsfMaturitySystem.Middleware
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IServiceProvider _serviceProvider;

        public ClaimsTransformer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // O IIS/Windows Authentication popula a identidade
            var identity = principal.Identity as WindowsIdentity; // ou apenas principal.Identity
            if (identity == null || !identity.IsAuthenticated)
            {
                return principal;
            }

            // Exemplo de Name: "DOMINIO\marcelofro" ou email (se configurado no AD)
            // Em ambiente corporativo real, mapearíamos isso para o email, ex: marcelofro@cptm.sp.gov.br
            var userName = identity.Name;
            var email = "marcelo.frota@cptm.sp.gov.br"; // Para fim de simulação da seed, forçaremos ou faremos um split

            // No mundo real: var email = ExtractEmailFromAD(userName);

            var clone = principal.Clone();
            var newIdentity = (ClaimsIdentity)clone.Identity;

            // Scoped resolution for DbContext
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await dbContext.Usuarios
                    .Include(u => u.Perfil)
                    .FirstOrDefaultAsync(u => u.Email == email && u.Status == "ATIVO");

                if (user != null)
                {
                    // Adicionando a Claim de Role (Perfil) para o RBAC funcionar no ASP.NET Core
                    newIdentity.AddClaim(new Claim(ClaimTypes.Role, user.Perfil.Nome));
                    newIdentity.AddClaim(new Claim("UsuarioId", user.Id.ToString()));
                }
            }

            // --- MOCK TEMPORÁRIO PARA DESENVOLVIMENTO ---
            // Como estamos rodando localmente (talvez sem o banco preenchido), 
            // vamos garantir que você (Marcelo) tenha acesso total, incluindo Auditor
            if (identity.Name != null && identity.Name.Contains("marcelofro", StringComparison.OrdinalIgnoreCase))
            {
                if (!newIdentity.HasClaim(c => c.Value == "Administrador"))
                    newIdentity.AddClaim(new Claim(ClaimTypes.Role, "Administrador"));
                
                if (!newIdentity.HasClaim(c => c.Value == "Editor"))
                    newIdentity.AddClaim(new Claim(ClaimTypes.Role, "Editor"));

                if (!newIdentity.HasClaim(c => c.Value == "Auditor"))
                    newIdentity.AddClaim(new Claim(ClaimTypes.Role, "Auditor"));
            }

            return clone;
        }
    }
}
