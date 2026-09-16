using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Data;
using NistCsfMaturitySystem.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace NistCsfMaturitySystem.Controllers
{
    [Authorize(Roles = "Administrador,Editor")]
    public class PlanoAcaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlanoAcaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retorna a lista de planos de ação (PartialView para abrir no Modal)
        [HttpGet]
        public async Task<IActionResult> ListarPorAvaliacao(int avaliacaoId)
        {
            var planos = await _context.PlanosAcao
                .Where(p => p.AvaliacaoId == avaliacaoId)
                .OrderBy(p => p.Prazo)
                .ToListAsync();

            ViewBag.AvaliacaoId = avaliacaoId;
            return PartialView("_ListaPlanos", planos);
        }

        [HttpPost]
        public async Task<IActionResult> Adicionar([FromBody] PlanoAcao dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Descricao))
                return BadRequest("A descrição do plano de ação é obrigatória.");

            var avaliacao = await _context.AvaliacoesSubcategoria
                .Include(a => a.Cenario)
                .FirstOrDefaultAsync(a => a.Id == dto.AvaliacaoId);

            if (avaliacao == null || avaliacao.Cenario.Tipo == "ATUAL" || avaliacao.Cenario.Status != "EM_ELABORACAO")
            {
                return BadRequest("Avaliação inválida ou cenário não editável.");
            }

            var novoPlano = new PlanoAcao
            {
                AvaliacaoId = dto.AvaliacaoId,
                Descricao = dto.Descricao,
                Prazo = dto.Prazo,
                Responsavel = dto.Responsavel,
                Status = "PENDENTE"
            };

            _context.PlanosAcao.Add(novoPlano);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Plano de Ação adicionado com sucesso!" });
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarStatus(int planoId, string novoStatus)
        {
            var plano = await _context.PlanosAcao.FindAsync(planoId);
            if (plano == null) return NotFound();

            var statusValidos = new[] { "PENDENTE", "EM_ANDAMENTO", "CONCLUIDO", "CANCELADO" };
            if (!statusValidos.Contains(novoStatus))
                return BadRequest("Status inválido.");

            plano.Status = novoStatus;
            _context.PlanosAcao.Update(plano);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> Excluir(int planoId)
        {
            var plano = await _context.PlanosAcao.FindAsync(planoId);
            if (plano == null) return NotFound();

            _context.PlanosAcao.Remove(plano);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
