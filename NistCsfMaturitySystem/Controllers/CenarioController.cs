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
                int usuarioId = 1;
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "UsuarioId")?.Value;
                if (!string.IsNullOrEmpty(claimId)) usuarioId = int.Parse(claimId);

                cenarioAtual = new Cenario 
                { 
                    Nome = "CENÁRIO ATUAL (BASELINE VIGENTE)", 
                    Tipo = "ATUAL", 
                    Status = "VIGENTE", 
                    DataCriacao = DateTime.Now,
                    DataEfetivacao = DateTime.Now,
                    CriadoPorId = usuarioId
                };
                _context.Cenarios.Add(cenarioAtual);
                await _context.SaveChangesAsync();
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
                            Avaliacao = cenarioAtual.Avaliacoes?.FirstOrDefault(a => a.SubcategoriaId == s.Id),
                            EfetivadaNoAtual = true
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
                int usuarioId = 1;
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "UsuarioId")?.Value;
                if (!string.IsNullOrEmpty(claimId)) usuarioId = int.Parse(claimId);

                // Como não existe cenário alvo em elaboração, cria e salva no banco de dados
                cenarioAlvo = new Cenario 
                { 
                    Nome = "CENÁRIO ALVO PADRÃO", 
                    Tipo = "ALVO", 
                    Status = "EM_ELABORACAO", 
                    DataCriacao = DateTime.Now,
                    CriadoPorId = usuarioId
                };
                _context.Cenarios.Add(cenarioAlvo);
                await _context.SaveChangesAsync();
            }

            // Busca o cenário atual vigente para identificar quais subcategorias já foram efetivadas
            var cenarioAtual = await _context.Cenarios
                .Include(c => c.Avaliacoes)
                .FirstOrDefaultAsync(c => c.Tipo == "ATUAL" && c.Status == "VIGENTE");

            var subcategoriasEfetivadasIds = cenarioAtual?.Avaliacoes
                .Where(a => !string.IsNullOrEmpty(a.TierAlvo))
                .Select(a => a.SubcategoriaId).ToHashSet() ?? new HashSet<int>();

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
                            Avaliacao = cenarioAlvo.Avaliacoes?.FirstOrDefault(a => a.SubcategoriaId == s.Id),
                            EfetivadaNoAtual = subcategoriasEfetivadasIds.Contains(s.Id)
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
            
            if (cenario == null || cenario.Tipo != "ALVO" || (cenario.Status != "EM_ELABORACAO" && cenario.Status != "PENDENTE_APROVACAO"))
            {
                return BadRequest(new { success = false, message = "Cenário inválido ou já arquivado." });
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

        // Endpoint AJAX para Solicitar Efetivação por Subcategoria (Submete para Revisão do Administrador)
        [HttpPost]
        [Authorize(Roles = "Administrador,Editor")]
        public async Task<IActionResult> SolicitarEfetivacaoSubcategoria([FromBody] AvaliacaoDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Dados inválidos." });

            if (string.IsNullOrWhiteSpace(dto.TierAlvo))
                return BadRequest(new { success = false, message = "Por favor, selecione pelo menos o 'Tier CSF Alvo' antes de submeter esta subcategoria para aprovação." });

            // 1. Salvar ou atualizar no Cenário Alvo
            var cenarioAlvo = await _context.Cenarios.FirstOrDefaultAsync(c => c.Id == dto.CenarioId);
            if (cenarioAlvo == null || cenarioAlvo.Tipo != "ALVO")
                return BadRequest(new { success = false, message = "Cenário Alvo inválido." });

            var avaliacaoAlvo = await _context.AvaliacoesSubcategoria
                .FirstOrDefaultAsync(a => a.CenarioId == dto.CenarioId && a.SubcategoriaId == dto.SubcategoriaId);

            if (avaliacaoAlvo == null)
            {
                avaliacaoAlvo = new AvaliacaoSubcategoria
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
                _context.AvaliacoesSubcategoria.Add(avaliacaoAlvo);
            }
            else
            {
                avaliacaoAlvo.PrioridadeAlvo = dto.PrioridadeAlvo;
                avaliacaoAlvo.TierAlvo = dto.TierAlvo;
                avaliacaoAlvo.PoliticasAlvo = dto.PoliticasAlvo;
                avaliacaoAlvo.PraticasAlvo = dto.PraticasAlvo;
                avaliacaoAlvo.ResponsabilidadesAlvo = dto.ResponsabilidadesAlvo;
                avaliacaoAlvo.ReferenciasAlvo = dto.ReferenciasAlvo;
                _context.AvaliacoesSubcategoria.Update(avaliacaoAlvo);
            }

            // 2. Colocar o Cenário Alvo como PENDENTE_APROVACAO (NÃO promove direto! O Administrador deve aprovar em Revisar Baseline)
            cenarioAlvo.Status = "PENDENTE_APROVACAO";
            _context.Cenarios.Update(cenarioAlvo);

            await _context.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = "Metas salvas e solicitação enviada! O Administrador agora deve aprovar na aba 'Revisar Baseline' para que as metas entrem em vigor no Cenário Atual.", 
                avaliacaoId = avaliacaoAlvo.Id 
            });
        }

        // Mantém compatibilidade com chamadas de EfetivarSubcategoria
        [HttpPost]
        [Authorize(Roles = "Administrador,Editor")]
        public async Task<IActionResult> EfetivarSubcategoria([FromBody] AvaliacaoDto dto)
        {
            return await SolicitarEfetivacaoSubcategoria(dto);
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

            if (cenario.Avaliacoes.Count == 0)
            {
                return BadRequest(new { success = false, message = "Você precisa avaliar pelo menos uma subcategoria antes de solicitar a aprovação." });
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
