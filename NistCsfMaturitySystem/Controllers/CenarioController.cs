using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NistCsfMaturitySystem.Controllers
{
    [Authorize]
    public class CenarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CenarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Cenário Atual (Vigente) - Somente Leitura
        public async Task<IActionResult> Atual()
        {
            var cenarioAtual = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                    .ThenInclude(a => a.Subcategoria)
                .FirstOrDefaultAsync(c => c.Tipo == "ATUAL" && c.Status == "VIGENTE");

            if (cenarioAtual == null)
            {
                ViewBag.Message = "Nenhum Cenário Atual vigente encontrado.";
            }

            return View(cenarioAtual);
        }

        // Cenário Alvo (Em elaboração) - Edição para Editores/Admins
        [Authorize(Roles = "Administrador,Editor")]
        public async Task<IActionResult> Alvo()
        {
            var cenarioAlvo = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                    .ThenInclude(a => a.Subcategoria)
                .FirstOrDefaultAsync(c => c.Tipo == "ALVO" && (c.Status == "EM_ELABORACAO" || c.Status == "PENDENTE_APROVACAO"));

            return View(cenarioAlvo);
        }
        
        // Exemplo de Endpoint AJAX para Atualização de Subcategoria no Cenário Alvo
        [HttpPost]
        [Authorize(Roles = "Administrador,Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarMaturidade(int avaliacaoId, string novoStatus, string prioridade)
        {
            var avaliacao = await _context.AvaliacoesSubcategoria
                .Include(a => a.Cenario)
                .FirstOrDefaultAsync(a => a.Id == avaliacaoId);

            if (avaliacao == null || avaliacao.Cenario.Tipo == "ATUAL")
            {
                return BadRequest("Avaliação não encontrada ou pertence a um cenário imutável.");
            }

            if (avaliacao.Cenario.Status != "EM_ELABORACAO")
            {
                return BadRequest("O cenário alvo não está em elaboração.");
            }

            avaliacao.StatusMaturidade = novoStatus;
            avaliacao.Prioridade = prioridade;

            await _context.SaveChangesAsync(); // AuditInterceptor vai gravar a alteração no AUDITORIA_LOG automaticamente!

            return Ok(new { success = true, message = "Avaliação salva com sucesso!" });
        }
    }
}
