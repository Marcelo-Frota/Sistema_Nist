using System.Collections.Generic;

namespace NistCsfMaturitySystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalSubcategorias { get; set; }
        public int SubcategoriasAvaliadas { get; set; }
        public int PlanosAcaoPendentes { get; set; }
        public string NomeCenarioAtual { get; set; }

        // Gráfico Radar (Maturidade por Função)
        public List<string> LabelsFuncoes { get; set; } = new List<string>();
        public List<double> ValoresMaturidade { get; set; } = new List<double>();
        public List<string> CoresFuncoes { get; set; } = new List<string>();

        // Gráfico de Rosca (Status de Maturidade)
        public int QtdAtendido { get; set; }
        public int QtdRazoavel { get; set; }
        public int QtdParcial { get; set; }
        public int QtdDeficitario { get; set; }
        public int QtdNaoAtendido { get; set; }
    }
}
