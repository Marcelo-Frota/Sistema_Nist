using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;
using NistCsfMaturitySystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NistCsfMaturitySystem.Middleware
{
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IServiceProvider _serviceProvider;

        public ClaimsTransformer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as WindowsIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return principal;
            }

            var userName = identity.Name;
            var email = "marcelo.frota@cptm.sp.gov.br"; // Mockado para achar no DB

            var clone = principal.Clone();
            
            // Criamos uma nova identidade específica para a nossa aplicação, 
            // dizendo explicitamente que o tipo de Role é ClaimTypes.Role
            var appIdentity = new ClaimsIdentity("NistAuth", ClaimTypes.Name, ClaimTypes.Role);

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await dbContext.Usuarios
                    .Include(u => u.Perfil)
                    .FirstOrDefaultAsync(u => u.Email == email && u.Status == "ATIVO");

                if (user != null)
                {
                    appIdentity.AddClaim(new Claim(ClaimTypes.Role, user.Perfil.Nome));
                    appIdentity.AddClaim(new Claim("UsuarioId", user.Id.ToString()));
                }
            }

            // --- MOCK TEMPORÁRIO PARA DESENVOLVIMENTO ---
            if (userName != null && userName.Contains("marcelofro", StringComparison.OrdinalIgnoreCase))
            {
                if (!appIdentity.HasClaim(c => c.Value == "Administrador"))
                    appIdentity.AddClaim(new Claim(ClaimTypes.Role, "Administrador"));
                
                if (!appIdentity.HasClaim(c => c.Value == "Editor"))
                    appIdentity.AddClaim(new Claim(ClaimTypes.Role, "Editor"));

                if (!appIdentity.HasClaim(c => c.Value == "Auditor"))
                    appIdentity.AddClaim(new Claim(ClaimTypes.Role, "Auditor"));
            }

            // Adiciona a nova identidade com as Roles ao principal clonado
            clone.AddIdentity(appIdentity);

            return clone;
        }
    }
}
