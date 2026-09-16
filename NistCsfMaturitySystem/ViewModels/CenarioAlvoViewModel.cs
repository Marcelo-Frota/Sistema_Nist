using System.Collections.Generic;
using NistCsfMaturitySystem.Models;

namespace NistCsfMaturitySystem.ViewModels
{
    public class CenarioAlvoViewModel
    {
        public Cenario Cenario { get; set; }
        public List<FuncaoViewModel> Funcoes { get; set; }
    }

    public class FuncaoViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Cor { get; set; }
        public List<CategoriaViewModel> Categorias { get; set; }
    }

    public class CategoriaViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public List<SubcategoriaViewModel> Subcategorias { get; set; }
    }

    public class SubcategoriaViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
        public AvaliacaoSubcategoria Avaliacao { get; set; } // Pode ser nulo se ainda não foi avaliado
    }
}
