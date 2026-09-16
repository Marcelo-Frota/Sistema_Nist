using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Data;
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
                .FirstOrDefaultAsync(c => c.Tipo == "ALVO" && c.Status == "PENDENTE_APROVACAO");

            if (cenarioAlvo == null)
            {
                ViewBag.Mensagem = "Não há nenhum Cenário Alvo aguardando aprovação no momento.";
                return View("Vazio");
            }

            var totalSubcategorias = await _context.NistSubcategorias.CountAsync(s => s.Status == "ATIVO");
            
            ViewBag.Progresso = (cenarioAlvo.Avaliacoes.Count * 100) / (totalSubcategorias == 0 ? 1 : totalSubcategorias);
            
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
    }
}
