using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Data;
using NistCsfMaturitySystem.Models;
using NistCsfMaturitySystem.Services;
using System.Linq;
using System.Threading.Tasks;

namespace NistCsfMaturitySystem.Controllers
{
    [Authorize(Roles = "Administrador,Auditor")] // Administradores e Auditores podem aprovar
    public class AprovacaoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EfetivacaoService _efetivacaoService;

        public AprovacaoController(ApplicationDbContext context, EfetivacaoService efetivacaoService)
        {
            _context = context;
            _efetivacaoService = efetivacaoService;
        }

        // Tela de revisão para o Administrador
        public async Task<IActionResult> Revisar()
        {
            var cenarioAlvo = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                    .ThenInclude(a => a.Subcategoria)
                        .ThenInclude(s => s.Categoria)
                            .ThenInclude(cat => cat.Funcao)
                .FirstOrDefaultAsync(c => c.Tipo == "ALVO" && (c.Status == "PENDENTE_APROVACAO" || c.Status == "EM_ELABORACAO"));

            if (cenarioAlvo == null)
            {
                ViewBag.Mensagem = "Não há nenhum Cenário Alvo com metas para revisão no momento.";
                return View("Vazio");
            }

            var totalSubcategorias = await _context.NistSubcategorias.CountAsync(s => s.Status == "ATIVO");
            var itensComMeta = cenarioAlvo.Avaliacoes.Count(a => !string.IsNullOrEmpty(a.TierAlvo));
            
            ViewBag.Progresso = (itensComMeta * 100) / (totalSubcategorias == 0 ? 1 : totalSubcategorias);

            // Obter subcategorias que já foram aprovadas e estão ativas no Cenário Atual
            var cenarioAtual = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                .FirstOrDefaultAsync(c => c.Tipo == "ATUAL" && c.Status == "VIGENTE");

            var subcategoriasEfetivadasIds = cenarioAtual?.Avaliacoes
                .Where(a => !string.IsNullOrEmpty(a.TierAlvo))
                .Select(a => a.SubcategoriaId).ToHashSet() ?? new HashSet<int>();

            ViewBag.SubcategoriasEfetivadasIds = subcategoriasEfetivadasIds;
            ViewBag.CenarioAtual = cenarioAtual;
            
            return View(cenarioAlvo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Efetivar(int id)
        {
            // Na prática, buscaríamos o ID real do usuário autenticado no banco via Claims (User.Identity.Name)
            var adminUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nome == "Marcelo Frota"); 
            var adminId = adminUser?.Id ?? 1;

            var resultado = await _efetivacaoService.EfetivarCenarioAlvoAsync(id, adminId);

            if (resultado.Success)
            {
                TempData["Sucesso"] = resultado.Message;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Erro"] = resultado.Message;
                return RedirectToAction("Revisar");
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rejeitar(int id)
        {
            var cenarioAlvo = await _context.Cenarios.FindAsync(id);
            if (cenarioAlvo != null && cenarioAlvo.Status == "PENDENTE_APROVACAO")
            {
                // Devolve para os editores
                cenarioAlvo.Status = "EM_ELABORACAO";
                _context.Cenarios.Update(cenarioAlvo);
                await _context.SaveChangesAsync();
                
                TempData["Sucesso"] = "Cenário devolvido para elaboração.";
            }
            
            return RedirectToAction("Revisar");
        }

        // Endpoint AJAX para Aprovação e Efetivação Modular de Subcategoria pelo Administrador
        [HttpPost]
        public async Task<IActionResult> AprovarSubcategoria([FromBody] AprovarSubcategoriaDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Dados inválidos." });

            var adminUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Nome == "Marcelo Frota"); 
            var adminId = adminUser?.Id ?? 1;

            var avaliacaoAlvo = await _context.AvaliacoesSubcategoria
                .Include(a => a.Subcategoria)
                .FirstOrDefaultAsync(a => a.CenarioId == dto.CenarioId && a.SubcategoriaId == dto.SubcategoriaId);

            if (avaliacaoAlvo == null)
                return BadRequest(new { success = false, message = "Avaliação da subcategoria não encontrada no Cenário Alvo." });

            if (string.IsNullOrWhiteSpace(avaliacaoAlvo.TierAlvo))
                return BadRequest(new { success = false, message = "Esta subcategoria não possui metas definidas (Tier Alvo está vazio)." });

            // 1. Localizar ou criar o Cenário Atual Vigente
            var cenarioAtual = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                .FirstOrDefaultAsync(c => c.Tipo == "ATUAL" && c.Status == "VIGENTE");

            if (cenarioAtual == null)
            {
                cenarioAtual = new Cenario
                {
                    Nome = "CENÁRIO ATUAL (BASELINE VIGENTE)",
                    Tipo = "ATUAL",
                    Status = "VIGENTE",
                    DataCriacao = DateTime.Now,
                    DataEfetivacao = DateTime.Now,
                    CriadoPorId = adminId
                };
                _context.Cenarios.Add(cenarioAtual);
                await _context.SaveChangesAsync();
            }

            // 2. Atualizar ou criar a subcategoria no Cenário Atual com as metas aprovadas
            var avaliacaoAtual = await _context.AvaliacoesSubcategoria
                .FirstOrDefaultAsync(a => a.CenarioId == cenarioAtual.Id && a.SubcategoriaId == dto.SubcategoriaId);

            if (avaliacaoAtual == null)
            {
                avaliacaoAtual = new AvaliacaoSubcategoria
                {
                    CenarioId = cenarioAtual.Id,
                    SubcategoriaId = dto.SubcategoriaId,
                    PrioridadeAlvo = avaliacaoAlvo.PrioridadeAlvo,
                    TierAlvo = avaliacaoAlvo.TierAlvo,
                    PoliticasAlvo = avaliacaoAlvo.PoliticasAlvo,
                    PraticasAlvo = avaliacaoAlvo.PraticasAlvo,
                    ResponsabilidadesAlvo = avaliacaoAlvo.ResponsabilidadesAlvo,
                    ReferenciasAlvo = avaliacaoAlvo.ReferenciasAlvo
                };
                _context.AvaliacoesSubcategoria.Add(avaliacaoAtual);
            }
            else
            {
                avaliacaoAtual.PrioridadeAlvo = avaliacaoAlvo.PrioridadeAlvo;
                avaliacaoAtual.TierAlvo = avaliacaoAlvo.TierAlvo;
                avaliacaoAtual.PoliticasAlvo = avaliacaoAlvo.PoliticasAlvo;
                avaliacaoAtual.PraticasAlvo = avaliacaoAlvo.PraticasAlvo;
                avaliacaoAtual.ResponsabilidadesAlvo = avaliacaoAlvo.ResponsabilidadesAlvo;
                avaliacaoAtual.ReferenciasAlvo = avaliacaoAlvo.ReferenciasAlvo;
                _context.AvaliacoesSubcategoria.Update(avaliacaoAtual);
            }

            cenarioAtual.DataEfetivacao = DateTime.Now;
            _context.Cenarios.Update(cenarioAtual);

            await _context.SaveChangesAsync();

            var codigoSubcat = avaliacaoAlvo.Subcategoria?.Codigo ?? $"ID {dto.SubcategoriaId}";
            return Ok(new { 
                success = true, 
                message = $"Subcategoria {codigoSubcat} aprovada e efetivada com sucesso no Cenário Atual (Baseline Vigente)!" 
            });
        }
    }

    public class AprovarSubcategoriaDto
    {
        public int CenarioId { get; set; }
        public int SubcategoriaId { get; set; }
    }
}
