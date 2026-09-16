using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NistCsfMaturitySystem.Models
{
    [Table("SGSI_PERFIL")]
    public class Perfil
    {
        [Key]
        [Column("ID_PERFIL")]
        public int Id { get; set; }

        [Required]
        [Column("TX_NOME")]
        [StringLength(50)]
        public string Nome { get; set; } = null!;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }

    [Table("SGSI_USUARIO")]
    public class Usuario
    {
        [Key]
        [Column("ID_USUARIO")]
        public int Id { get; set; }

        [Required]
        [Column("TX_EMAIL")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [Column("TX_NOME")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Column("TX_STATUS")]
        [StringLength(20)]
        public string Status { get; set; } = "ATIVO";

        [Column("ID_PERFIL")]
        public int PerfilId { get; set; }

        [ForeignKey("PerfilId")]
        public Perfil Perfil { get; set; } = null!;
    }

    [Table("SGSI_FUNCAO")]
    public class NistFuncao
    {
        [Key]
        [Column("ID_FUNCAO")]
        public int Id { get; set; }

        [Required]
        [Column("CD_FUNCAO")]
        [StringLength(10)]
        public string Codigo { get; set; } = null!;

        [Required]
        [Column("TX_NOME")]
        [StringLength(100)]
        public string Nome { get; set; } = null!;

        [Column("TX_COR")]
        [StringLength(20)]
        public string Cor { get; set; } = null!;

        public ICollection<NistCategoria> Categorias { get; set; } = new List<NistCategoria>();
    }

    [Table("SGSI_CATEGORIA")]
    public class NistCategoria
    {
        [Key]
        [Column("ID_CATEGORIA")]
        public int Id { get; set; }

        [Column("ID_FUNCAO")]
        public int FuncaoId { get; set; }

        [Required]
        [Column("CD_CATEGORIA")]
        [StringLength(20)]
        public string Codigo { get; set; } = null!;

        [Required]
        [Column("TX_NOME")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Column("TX_DESCRICAO")]
        [StringLength(1000)]
        public string Descricao { get; set; } = null!;

        [ForeignKey("FuncaoId")]
        public NistFuncao Funcao { get; set; } = null!;

        public ICollection<NistSubcategoria> Subcategorias { get; set; } = new List<NistSubcategoria>();
    }

    [Table("SGSI_SUBCATEGORIA")]
    public class NistSubcategoria
    {
        [Key]
        [Column("ID_SUBCATEGORIA")]
        public int Id { get; set; }

        [Column("ID_CATEGORIA")]
        public int CategoriaId { get; set; }

        [Required]
        [Column("CD_SUBCATEGORIA")]
        [StringLength(20)]
        public string Codigo { get; set; } = null!;

        [Required]
        [Column("TX_DESCRICAO")]
        public string Descricao { get; set; } = null!;

        [Column("TX_STATUS")]
        [StringLength(20)]
        public string Status { get; set; } = "ATIVO";

        [ForeignKey("CategoriaId")]
        public NistCategoria Categoria { get; set; } = null!;
        
        public ICollection<NistExemploImplementacao> Exemplos { get; set; } = new List<NistExemploImplementacao>();
    }

    [Table("SGSI_EXEMPLO_IMP")]
    public class NistExemploImplementacao
    {
        [Key]
        [Column("ID_EXEMPLO")]
        public int Id { get; set; }

        [Column("ID_SUBCATEGORIA")]
        public int SubcategoriaId { get; set; }

        [Required]
        [Column("TX_EXEMPLO")]
        public string Exemplo { get; set; } = null!;

        [ForeignKey("SubcategoriaId")]
        public NistSubcategoria Subcategoria { get; set; } = null!;
    }

    [Table("SGSI_CENARIO")]
    public class Cenario
    {
        [Key]
        [Column("ID_CENARIO")]
        public int Id { get; set; }

        [Required]
        [Column("TX_NOME")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Required]
        [Column("TX_TIPO")]
        [StringLength(10)]
        public string Tipo { get; set; } = null!; // ATUAL, ALVO

        [Required]
        [Column("TX_STATUS")]
        [StringLength(20)]
        public string Status { get; set; } = null!; // EM_ELABORACAO, VIGENTE, etc.

        [Column("DT_CRIACAO")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [Column("DT_EFETIVACAO")]
        public DateTime? DataEfetivacao { get; set; }

        [Column("ID_USUARIO_CRIADOR")]
        public int CriadoPorId { get; set; }

        [ForeignKey("CriadoPorId")]
        public Usuario CriadoPor { get; set; } = null!;
        
        public ICollection<AvaliacaoSubcategoria> Avaliacoes { get; set; } = new List<AvaliacaoSubcategoria>();
    }

    [Table("SGSI_AVALIA_SUBCAT")]
    public class AvaliacaoSubcategoria
    {
        [Key]
        [Column("ID_AVALIACAO")]
        public int Id { get; set; }

        [Column("ID_CENARIO")]
        public int CenarioId { get; set; }

        [Column("ID_SUBCATEGORIA")]
        public int SubcategoriaId { get; set; }

        [Column("TX_STATUS_MATURIDADE")]
        [StringLength(50)]
        public string? StatusMaturidade { get; set; }

        [Column("TX_PRIORIDADE")]
        [StringLength(10)]
        public string? Prioridade { get; set; }

        [Column("NR_NIVEL_IMPLEMENTACAO")]
        public int? NivelImplementacao { get; set; }

        [Column("TX_JUSTIFICATIVA")]
        public string? Justificativa { get; set; }

        [Column("TX_POLITICAS_ALVO")]
        public string? PoliticasAlvo { get; set; }

        [Column("TX_PRATICAS_ALVO")]
        public string? PraticasAlvo { get; set; }

        [Column("TX_RESPONSABILIDADES")]
        public string? ResponsabilidadesAlvo { get; set; }

        [ForeignKey("CenarioId")]
        public Cenario Cenario { get; set; } = null!;

        [ForeignKey("SubcategoriaId")]
        public NistSubcategoria Subcategoria { get; set; } = null!;
    }

    [Table("SGSI_PLANO_ACAO")]
    public class PlanoAcao
    {
        [Key]
        [Column("ID_PLANO")]
        public int Id { get; set; }

        [Column("ID_AVALIACAO")]
        public int AvaliacaoId { get; set; }

        [Column("TX_DESCRICAO")]
        public string Descricao { get; set; } = null!;

        [Column("DT_PRAZO")]
        public DateTime? Prazo { get; set; }

        [Column("TX_RESPONSAVEL")]
        public string? Responsavel { get; set; }

        [Column("TX_STATUS")]
        public string Status { get; set; } = "PENDENTE";
    }

    [Table("SGSI_AUDITORIA_LOG")]
    public class AuditoriaLog
    {
        [Key]
        [Column("ID_AUDITORIA")]
        public int Id { get; set; }

        [Column("ID_USUARIO")]
        public int? UsuarioId { get; set; }

        [Column("DT_HORA_UTC")]
        public DateTime DataHoraUTC { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("TX_ACAO")]
        [StringLength(50)]
        public string Acao { get; set; } = null!;

        [Required]
        [Column("TX_MODULO")]
        [StringLength(100)]
        public string Modulo { get; set; } = null!;

        [Column("TX_ENTIDADE_AFETADA")]
        [StringLength(100)]
        public string? EntidadeAfetada { get; set; }

        [Column("ID_REGISTRO")]
        public int? RegistroId { get; set; }

        [Column("TX_VALORES_ANTERIORES")]
        public string? ValoresAnteriores { get; set; }

        [Column("TX_NOVOS_VALORES")]
        public string? NovosValores { get; set; }

        [Column("TX_IP_ORIGEM")]
        [StringLength(50)]
        public string? IpOrigem { get; set; }
    }
}

