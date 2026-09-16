using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NistCsfMaturitySystem.Data;
using NistCsfMaturitySystem.Models;
using NistCsfMaturitySystem.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NistCsfMaturitySystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            // 1. Identificar o Cenário VIGENTE (Atual) ou o ALVO se não houver vigente
            var cenario = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                    .ThenInclude(a => a.Subcategoria)
                        .ThenInclude(s => s.Categoria)
                            .ThenInclude(cat => cat.Funcao)
                .Where(c => c.Status == "VIGENTE")
                .OrderByDescending(c => c.DataEfetivacao)
                .FirstOrDefaultAsync();

            if (cenario == null)
            {
                // Se não tem cenário vigente aprovado, tenta pegar o em elaboração para ter algo no painel
                cenario = await _context.Cenarios
                    .Include(c => c.Avaliacoes)
                        .ThenInclude(a => a.Subcategoria)
                            .ThenInclude(s => s.Categoria)
                                .ThenInclude(cat => cat.Funcao)
                    .FirstOrDefaultAsync(c => c.Status == "EM_ELABORACAO");
            }

            if (cenario != null)
            {
                viewModel.NomeCenarioAtual = cenario.Nome + (cenario.Status == "EM_ELABORACAO" ? " (Em Elaboração)" : "");
                viewModel.TotalSubcategorias = await _context.NistSubcategorias.CountAsync(s => s.Status == "ATIVO");
                viewModel.SubcategoriasAvaliadas = cenario.Avaliacoes.Count;

                // Status Counts
                viewModel.QtdAtendido = cenario.Avaliacoes.Count(a => a.StatusMaturidade == "ATENDIDO");
                viewModel.QtdRazoavel = cenario.Avaliacoes.Count(a => a.StatusMaturidade == "RAZOAVELMENTE_ATENDIDO");
                viewModel.QtdParcial = cenario.Avaliacoes.Count(a => a.StatusMaturidade == "PARCIALMENTE_ATENDIDO");
                viewModel.QtdDeficitario = cenario.Avaliacoes.Count(a => a.StatusMaturidade == "DEFICITARIAMENTE_ATENDIDO");
                viewModel.QtdNaoAtendido = cenario.Avaliacoes.Count(a => a.StatusMaturidade == "NAO_ATENDIDO");

                // Planos de Ação Pendentes (Vinculados a este cenário)
                var avaliacoesIds = cenario.Avaliacoes.Select(a => a.Id).ToList();
                viewModel.PlanosAcaoPendentes = await _context.PlanosAcao
                    .CountAsync(p => avaliacoesIds.Contains(p.AvaliacaoId) && p.Status == "PENDENTE");

                // Cálculo para Gráfico Radar (Agrupado por Função)
                // Vamos atribuir pesos: Atendido = 4, Razoavel = 3, Parcial = 2, Deficitario = 1, Nao Atendido = 0
                var funcoes = await _context.NistFuncoes.ToListAsync();
                foreach (var f in funcoes)
                {
                    viewModel.LabelsFuncoes.Add(f.Nome);
                    viewModel.CoresFuncoes.Add(f.Cor);

                    var avaliacoesDaFuncao = cenario.Avaliacoes
                        .Where(a => a.Subcategoria.Categoria.FuncaoId == f.Id)
                        .ToList();

                    if (avaliacoesDaFuncao.Any())
                    {
                        double scoreTotal = avaliacoesDaFuncao.Sum(a => a.StatusMaturidade switch
                        {
                            "ATENDIDO" => 4.0,
                            "RAZOAVELMENTE_ATENDIDO" => 3.0,
                            "PARCIALMENTE_ATENDIDO" => 2.0,
                            "DEFICITARIAMENTE_ATENDIDO" => 1.0,
                            _ => 0.0
                        });
                        
                        // Media (0 a 4)
                        double media = scoreTotal / avaliacoesDaFuncao.Count;
                        // Converte para percentual (0 a 100%)
                        viewModel.ValoresMaturidade.Add(Math.Round((media / 4.0) * 100, 1));
                    }
                    else
                    {
                        viewModel.ValoresMaturidade.Add(0);
                    }
                }
            }
            else
            {
                viewModel.NomeCenarioAtual = "Nenhum Cenário Encontrado";
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
