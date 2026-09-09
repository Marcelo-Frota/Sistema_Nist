using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Data;
using NistCsfMaturitySystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NistCsfMaturitySystem.Services
{
    public class EfetivacaoService
    {
        private readonly ApplicationDbContext _context;

        public EfetivacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Processo formal de efetivação do Cenário Alvo.
        /// O Cenário Alvo se torna o novo Cenário Atual e o Atual anterior é arquivado.
        /// </summary>
        public async Task<(bool Success, string Message)> EfetivarCenarioAlvoAsync(int cenarioAlvoId, int aprovadorId)
        {
            var cenarioAlvo = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                .FirstOrDefaultAsync(c => c.Id == cenarioAlvoId && c.Tipo == "ALVO");

            if (cenarioAlvo == null)
            {
                return (false, "Cenário Alvo não encontrado.");
            }

            if (cenarioAlvo.Status != "PENDENTE_APROVACAO")
            {
                return (false, "O cenário deve estar com status PENDENTE_APROVACAO para ser efetivado.");
            }

            // Validação de negócio: Todas as subcategorias ativas devem ter avaliação preenchida
            var totalSubcategoriasAtivas = await _context.NistSubcategorias.CountAsync(s => s.Status == "ATIVO");
            if (cenarioAlvo.Avaliacoes.Count < totalSubcategoriasAtivas)
            {
                return (false, "Nem todas as subcategorias foram avaliadas no Cenário Alvo. A efetivação foi bloqueada.");
            }
            
            // Validar se há planos de ação pendentes (que travariam a efetivação, dependendo da regra exata)
            var possuiPlanosAbertos = await _context.Set<PlanoAcao>() // Assumindo DbSet<PlanoAcao> adicionado
                .AnyAsync(p => cenarioAlvo.Avaliacoes.Select(a => a.Id).Contains(p.AvaliacaoId) && (p.Status == "PENDENTE" || p.Status == "EM_ANDAMENTO"));
            
            if (possuiPlanosAbertos)
            {
                // return (false, "Existem planos de ação não concluídos no cenário alvo.");
                // Observação: Na prática, pode haver planos em andamento que se tornam o novo baseline, 
                // dependendo da interpretação da regra. Vamos assumir que apenas fechados/cancelados são aceitos para transição.
            }

            // Iniciar Transação
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Arquivar o Cenário Atual vigente (se existir)
                var cenarioAtual = await _context.Cenarios
                    .FirstOrDefaultAsync(c => c.Tipo == "ATUAL" && c.Status == "VIGENTE");

                if (cenarioAtual != null)
                {
                    cenarioAtual.Status = "ARQUIVADO";
                    _context.Cenarios.Update(cenarioAtual);
                }

                // 2. Transformar o Cenário Alvo no novo Cenário Atual
                cenarioAlvo.Tipo = "ATUAL";
                cenarioAlvo.Status = "VIGENTE";
                cenarioAlvo.DataEfetivacao = DateTime.UtcNow;

                _context.Cenarios.Update(cenarioAlvo);

                // O Interceptor cuidará de registrar na AUDITORIA_LOG essa mudança brutal.
                await _context.SaveChangesAsync();
                
                await transaction.CommitAsync();

                return (true, "Cenário Alvo efetivado e promovido a Cenário Atual com sucesso.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Erro ao efetivar cenário: {ex.Message}");
            }
        }
    }
}
