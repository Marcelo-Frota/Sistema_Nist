using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NistCsfMaturitySystem.Models
{
    [Table("PERFIS")]
    public class Perfil
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("NOME")]
        [StringLength(50)]
        public string Nome { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }

    [Table("USUARIOS")]
    public class Usuario
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("EMAIL")]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [Column("NOME")]
        [StringLength(255)]
        public string Nome { get; set; }

        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; } = "ATIVO";

        [Column("PERFIL_ID")]
        public int PerfilId { get; set; }

        [ForeignKey("PerfilId")]
        public Perfil Perfil { get; set; }
    }

    [Table("NIST_FUNCOES")]
    public class NistFuncao
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("CODIGO")]
        [StringLength(10)]
        public string Codigo { get; set; }

        [Required]
        [Column("NOME")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Column("COR")]
        [StringLength(20)]
        public string Cor { get; set; }

        public ICollection<NistCategoria> Categorias { get; set; }
    }

    [Table("NIST_CATEGORIAS")]
    public class NistCategoria
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("FUNCAO_ID")]
        public int FuncaoId { get; set; }

        [Required]
        [Column("CODIGO")]
        [StringLength(20)]
        public string Codigo { get; set; }

        [Required]
        [Column("NOME")]
        [StringLength(255)]
        public string Nome { get; set; }

        [Column("DESCRICAO")]
        [StringLength(1000)]
        public string Descricao { get; set; }

        [ForeignKey("FuncaoId")]
        public NistFuncao Funcao { get; set; }

        public ICollection<NistSubcategoria> Subcategorias { get; set; }
    }

    [Table("NIST_SUBCATEGORIAS")]
    public class NistSubcategoria
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("CATEGORIA_ID")]
        public int CategoriaId { get; set; }

        [Required]
        [Column("CODIGO")]
        [StringLength(20)]
        public string Codigo { get; set; }

        [Required]
        [Column("DESCRICAO")]
        public string Descricao { get; set; }

        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; } = "ATIVO";

        [ForeignKey("CategoriaId")]
        public NistCategoria Categoria { get; set; }
        
        public ICollection<NistExemploImplementacao> Exemplos { get; set; }
    }

    [Table("NIST_EXEMPLOS_IMPLEMENTACAO")]
    public class NistExemploImplementacao
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("SUBCATEGORIA_ID")]
        public int SubcategoriaId { get; set; }

        [Required]
        [Column("EXEMPLO")]
        public string Exemplo { get; set; }

        [ForeignKey("SubcategoriaId")]
        public NistSubcategoria Subcategoria { get; set; }
    }

    [Table("CENARIOS")]
    public class Cenario
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("NOME")]
        [StringLength(255)]
        public string Nome { get; set; }

        [Required]
        [Column("TIPO")]
        [StringLength(10)]
        public string Tipo { get; set; } // ATUAL, ALVO

        [Required]
        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; } // EM_ELABORACAO, VIGENTE, etc.

        [Column("DATA_CRIACAO")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [Column("DATA_EFETIVACAO")]
        public DateTime? DataEfetivacao { get; set; }

        [Column("CRIADO_POR")]
        public int CriadoPorId { get; set; }

        [ForeignKey("CriadoPorId")]
        public Usuario CriadoPor { get; set; }
        
        public ICollection<AvaliacaoSubcategoria> Avaliacoes { get; set; }
    }

    [Table("AVALIACOES_SUBCATEGORIA")]
    public class AvaliacaoSubcategoria
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("CENARIO_ID")]
        public int CenarioId { get; set; }

        [Column("SUBCATEGORIA_ID")]
        public int SubcategoriaId { get; set; }

        [Column("STATUS_MATURIDADE")]
        [StringLength(50)]
        public string StatusMaturidade { get; set; }

        [Column("PRIORIDADE")]
        [StringLength(10)]
        public string Prioridade { get; set; }

        [Column("NIVEL_IMPLEMENTACAO")]
        public int? NivelImplementacao { get; set; }

        [Column("JUSTIFICATIVA")]
        public string Justificativa { get; set; }

        [Column("POLITICAS_ALVO")]
        public string PoliticasAlvo { get; set; }

        [Column("PRATICAS_ALVO")]
        public string PraticasAlvo { get; set; }

        [Column("RESPONSABILIDADES_ALVO")]
        public string ResponsabilidadesAlvo { get; set; }

        [ForeignKey("CenarioId")]
        public Cenario Cenario { get; set; }

        [ForeignKey("SubcategoriaId")]
        public NistSubcategoria Subcategoria { get; set; }
    }

    [Table("AUDITORIA_LOG")]
    public class AuditoriaLog
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("USUARIO_ID")]
        public int? UsuarioId { get; set; }

        [Column("DATA_HORA_UTC")]
        public DateTime DataHoraUTC { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("ACAO")]
        [StringLength(50)]
        public string Acao { get; set; }

        [Required]
        [Column("MODULO")]
        [StringLength(100)]
        public string Modulo { get; set; }

        [Column("ENTIDADE_AFETADA")]
        [StringLength(100)]
        public string EntidadeAfetada { get; set; }

        [Column("REGISTRO_ID")]
        public int? RegistroId { get; set; }

        [Column("VALORES_ANTERIORES")]
        public string ValoresAnteriores { get; set; }

        [Column("NOVOS_VALORES")]
        public string NovosValores { get; set; }

        [Column("IP_ORIGEM")]
        [StringLength(50)]
        public string IpOrigem { get; set; }
    }
}
