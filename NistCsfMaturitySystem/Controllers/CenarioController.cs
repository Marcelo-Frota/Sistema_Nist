using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Data;
using NistCsfMaturitySystem.Models;
using NistCsfMaturitySystem.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

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
                .FirstOrDefaultAsync(c => c.Tipo == "ATUAL" && c.Status == "VIGENTE");

            if (cenarioAtual == null)
            {
                ViewBag.Message = "Nenhum Cenário Atual vigente encontrado.";
                return View(new CenarioAlvoViewModel { Cenario = new Cenario { Nome = "Não há baseline vigente" } });
            }

            var funcoesDb = await _context.NistFuncoes
                .Include(f => f.Categorias)
                    .ThenInclude(c => c.Subcategorias.Where(s => s.Status == "ATIVO"))
                .ToListAsync();

            var viewModel = new CenarioAlvoViewModel
            {
                Cenario = cenarioAtual,
                Funcoes = funcoesDb.Select(f => new FuncaoViewModel
                {
                    Id = f.Id,
                    Codigo = f.Codigo,
                    Nome = f.Nome,
                    Cor = f.Cor,
                    Categorias = f.Categorias.Select(c => new CategoriaViewModel
                    {
                        Id = c.Id,
                        Codigo = c.Codigo,
                        Nome = c.Nome,
                        Descricao = c.Descricao,
                        Subcategorias = c.Subcategorias.Select(s => new SubcategoriaViewModel
                        {
                            Id = s.Id,
                            Codigo = s.Codigo,
                            Descricao = s.Descricao,
                            Avaliacao = cenarioAtual.Avaliacoes?.FirstOrDefault(a => a.SubcategoriaId == s.Id)
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        // Cenário Alvo (Em elaboração) - Edição para Editores/Admins
        [Authorize(Roles = "Administrador,Editor")]
        public async Task<IActionResult> Alvo()
        {
            // Busca o cenário alvo em elaboração
            var cenarioAlvo = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                .FirstOrDefaultAsync(c => c.Tipo == "ALVO" && (c.Status == "EM_ELABORACAO" || c.Status == "PENDENTE_APROVACAO"));

            if (cenarioAlvo == null)
            {
                // Em um sistema real, o Admin criaria um novo. Para teste, criamos um em memória se não existir
                cenarioAlvo = new Cenario { Nome = "Cenário Alvo Padrão", Tipo = "ALVO", Status = "EM_ELABORACAO" };
                // return View("NaoEncontrado"); 
            }

            // Montar a Árvore do NIST CSF com as avaliações
            var funcoesDb = await _context.NistFuncoes
                .Include(f => f.Categorias)
                    .ThenInclude(c => c.Subcategorias.Where(s => s.Status == "ATIVO"))
                .ToListAsync();

            var viewModel = new CenarioAlvoViewModel
            {
                Cenario = cenarioAlvo,
                Funcoes = funcoesDb.Select(f => new FuncaoViewModel
                {
                    Id = f.Id,
                    Codigo = f.Codigo,
                    Nome = f.Nome,
                    Cor = f.Cor,
                    Categorias = f.Categorias.Select(c => new CategoriaViewModel
                    {
                        Id = c.Id,
                        Codigo = c.Codigo,
                        Nome = c.Nome,
                        Descricao = c.Descricao,
                        Subcategorias = c.Subcategorias.Select(s => new SubcategoriaViewModel
                        {
                            Id = s.Id,
                            Codigo = s.Codigo,
                            Descricao = s.Descricao,
                            // Mapeia a avaliação existente, se houver
                            Avaliacao = cenarioAlvo.Avaliacoes?.FirstOrDefault(a => a.SubcategoriaId == s.Id)
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }
        
        // Endpoint AJAX para Atualização de Subcategoria no Cenário Alvo
        [HttpPost]
        [Authorize(Roles = "Administrador,Editor")]
        public async Task<IActionResult> SalvarAlvo([FromBody] AvaliacaoDto dto)
        {
            var cenario = await _context.Cenarios.FirstOrDefaultAsync(c => c.Id == dto.CenarioId);
            
            if (cenario == null || cenario.Tipo != "ALVO" || cenario.Status != "EM_ELABORACAO")
            {
                return BadRequest(new { success = false, message = "Cenário inválido ou não editável." });
            }

            var avaliacao = await _context.AvaliacoesSubcategoria
                .FirstOrDefaultAsync(a => a.CenarioId == dto.CenarioId && a.SubcategoriaId == dto.SubcategoriaId);

            if (avaliacao == null)
            {
                avaliacao = new AvaliacaoSubcategoria
                {
                    CenarioId = dto.CenarioId,
                    SubcategoriaId = dto.SubcategoriaId,
                    PrioridadeAlvo = dto.PrioridadeAlvo,
                    TierAlvo = dto.TierAlvo,
                    PoliticasAlvo = dto.PoliticasAlvo,
                    PraticasAlvo = dto.PraticasAlvo,
                    ResponsabilidadesAlvo = dto.ResponsabilidadesAlvo,
                    ReferenciasAlvo = dto.ReferenciasAlvo
                };
                _context.AvaliacoesSubcategoria.Add(avaliacao);
            }
            else
            {
                avaliacao.PrioridadeAlvo = dto.PrioridadeAlvo;
                avaliacao.TierAlvo = dto.TierAlvo;
                avaliacao.PoliticasAlvo = dto.PoliticasAlvo;
                avaliacao.PraticasAlvo = dto.PraticasAlvo;
                avaliacao.ResponsabilidadesAlvo = dto.ResponsabilidadesAlvo;
                avaliacao.ReferenciasAlvo = dto.ReferenciasAlvo;
                _context.AvaliacoesSubcategoria.Update(avaliacao);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Metas do Alvo salvas com sucesso!", avaliacaoId = avaliacao.Id });
        }

        // Endpoint AJAX para Atualização de Subcategoria no Cenário Atual
        [HttpPost]
        [Authorize(Roles = "Administrador,Editor,Auditor")]
        public async Task<IActionResult> SalvarAtual([FromBody] AvaliacaoDto dto)
        {
            var cenario = await _context.Cenarios.FirstOrDefaultAsync(c => c.Id == dto.CenarioId);
            
            if (cenario == null || cenario.Tipo != "ATUAL")
            {
                return BadRequest(new { success = false, message = "Cenário inválido para edição atual." });
            }

            var avaliacao = await _context.AvaliacoesSubcategoria
                .FirstOrDefaultAsync(a => a.CenarioId == dto.CenarioId && a.SubcategoriaId == dto.SubcategoriaId);

            if (avaliacao == null)
            {
                // Se não existir avaliação (não deveria acontecer se o Alvo gerou tudo, mas por segurança)
                avaliacao = new AvaliacaoSubcategoria
                {
                    CenarioId = dto.CenarioId,
                    SubcategoriaId = dto.SubcategoriaId,
                    PrioridadeAtual = dto.PrioridadeAtual,
                    StatusAtual = dto.StatusAtual,
                    PoliticasAtual = dto.PoliticasAtual,
                    PraticasAtual = dto.PraticasAtual,
                    ResponsabilidadesAtual = dto.ResponsabilidadesAtual,
                    ReferenciasAtual = dto.ReferenciasAtual,
                    EvidenciasAtual = dto.EvidenciasAtual
                };
                _context.AvaliacoesSubcategoria.Add(avaliacao);
            }
            else
            {
                avaliacao.PrioridadeAtual = dto.PrioridadeAtual;
                avaliacao.StatusAtual = dto.StatusAtual;
                avaliacao.PoliticasAtual = dto.PoliticasAtual;
                avaliacao.PraticasAtual = dto.PraticasAtual;
                avaliacao.ResponsabilidadesAtual = dto.ResponsabilidadesAtual;
                avaliacao.ReferenciasAtual = dto.ReferenciasAtual;
                avaliacao.EvidenciasAtual = dto.EvidenciasAtual;
                _context.AvaliacoesSubcategoria.Update(avaliacao);
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Status Atual salvo com sucesso!", avaliacaoId = avaliacao.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Editor")]
        public async Task<IActionResult> SubmeterAprovacao(int id)
        {
            var cenario = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cenario == null || cenario.Tipo != "ALVO" || cenario.Status != "EM_ELABORACAO")
                return BadRequest(new { success = false, message = "Cenário inválido para submissão." });

            var totalSubcategorias = await _context.NistSubcategorias.CountAsync(s => s.Status == "ATIVO");
            if (cenario.Avaliacoes.Count < totalSubcategorias)
            {
                return BadRequest(new { success = false, message = $"Ainda faltam itens a serem avaliados. Progresso: {cenario.Avaliacoes.Count}/{totalSubcategorias}" });
            }

            cenario.Status = "PENDENTE_APROVACAO";
            _context.Cenarios.Update(cenario);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cenário submetido para aprovação do Administrador." });
        }
    }

    public class AvaliacaoDto
    {
        public int CenarioId { get; set; }
        public int SubcategoriaId { get; set; }

        // Campos Alvo
        public string? PrioridadeAlvo { get; set; }
        public string? TierAlvo { get; set; }
        public string? PoliticasAlvo { get; set; }
        public string? PraticasAlvo { get; set; }
        public string? ResponsabilidadesAlvo { get; set; }
        public string? ReferenciasAlvo { get; set; }

        // Campos Atuais
        public string? PrioridadeAtual { get; set; }
        public string? StatusAtual { get; set; }
        public string? PoliticasAtual { get; set; }
        public string? PraticasAtual { get; set; }
        public string? ResponsabilidadesAtual { get; set; }
        public string? ReferenciasAtual { get; set; }
        public string? EvidenciasAtual { get; set; }
    }
}
