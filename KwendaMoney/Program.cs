using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using KwendaMoney.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Envio de e-mail
builder.Services.AddTransient<IEmailSender, EmailSender>();

// ✅ Banco de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ✅ Identity com roles e confirmação
builder.Services.AddDefaultIdentity<Usuario>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // true se quiser confirmação por e-mail
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// ✅ Criar admin se necessário
async Task CriarPerfilAdminAsync()
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<Usuario>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string roleName = "Admin";

    // Cria o papel Admin se não existir
    if (!await roleManager.RoleExistsAsync(roleName))
        await roleManager.CreateAsync(new IdentityRole(roleName));

    // Verifica se já existe algum usuário com a role Admin
    var adminsExistem = (await userManager.GetUsersInRoleAsync(roleName)).Any();
    if (!adminsExistem)
    {
        // Se não existir admin, promove o primeiro usuário criado
        var primeiroUsuario = await userManager.Users.OrderBy(u => u.Id).FirstOrDefaultAsync();
        if (primeiroUsuario != null)
        {
            await userManager.AddToRoleAsync(primeiroUsuario, roleName);
        }
    }
}


await CriarPerfilAdminAsync();

// ✅ Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // IMPORTANTE: deve vir antes de Authorization
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // Apenas uma vez

app.Run();
