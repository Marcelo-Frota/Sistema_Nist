using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Models;

namespace NistCsfMaturitySystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<NistFuncao> NistFuncoes { get; set; }
        public DbSet<NistCategoria> NistCategorias { get; set; }
        public DbSet<NistSubcategoria> NistSubcategorias { get; set; }
        public DbSet<NistExemploImplementacao> NistExemplosImplementacao { get; set; }
        public DbSet<Cenario> Cenarios { get; set; }
        public DbSet<AvaliacaoSubcategoria> AvaliacoesSubcategoria { get; set; }
        public DbSet<PlanoAcao> PlanosAcao { get; set; }
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Unique constraints
            modelBuilder.Entity<AvaliacaoSubcategoria>()
                .HasIndex(a => new { a.CenarioId, a.SubcategoriaId })
                .IsUnique();

            // Default values
            modelBuilder.Entity<Cenario>()
                .Property(c => c.DataCriacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            modelBuilder.Entity<AuditoriaLog>()
                .Property(a => a.DataHoraUTC)
                .HasDefaultValueSql("SYS_EXTRACT_UTC(SYSTIMESTAMP)");
        }
    }
}
