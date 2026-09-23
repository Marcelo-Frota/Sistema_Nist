using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using NistCsfMaturitySystem.Data;
using NistCsfMaturitySystem.Services;
using NistCsfMaturitySystem.Middleware;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurando Autenticação Windows (NTLM / Negotiate)
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // Por padrão, exige usuário autenticado e ativo no banco (que injeta a role)
    options.FallbackPolicy = options.DefaultPolicy;
});

// Adicionando a transformação de Claims para injetar o Perfil do Oracle
builder.Services.AddScoped<IClaimsTransformation, ClaimsTransformer>();

// Registro do DbContext Oracle e Audit Interceptor
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddScoped<EfetivacaoService>();

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
    var connectionString = builder.Configuration.GetConnectionString("OracleDbConnection");
    
    // Fallback in-memory para fim de desenvolvimento se a string estiver vazia
    if (string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("NistCsfDb");
    }
    else 
    {
        options.UseOracle(connectionString)
               .AddInterceptors(auditInterceptor);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Home/Error");
    // app.UseHsts();
}
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
