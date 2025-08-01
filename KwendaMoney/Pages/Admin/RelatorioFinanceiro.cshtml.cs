using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KwendaMoney.Data;
using KwendaMoney.Models;
using System.Threading.Tasks;

namespace KwendaMoney.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class RelatorioFinanceiroModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RelatorioFinanceiroModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public RelatorioFinanceiroSistema Relatorio { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Relatorio = await _context.RelatorioFinanceiroSistema.FirstOrDefaultAsync();

            if (Relatorio == null)
            {
                Relatorio = new RelatorioFinanceiroSistema();
                _context.RelatorioFinanceiroSistema.Add(Relatorio);
                await _context.SaveChangesAsync();
            }

            var totalCarteiraGeral = await _context.Users
                .SumAsync(u => (decimal?)u.SaldoCarteiraGeral) ?? 0;

            var totalInvestido = await _context.CarteirasInvestimento
                .SumAsync(c => (decimal?)c.ValorInvestido) ?? 0;

            var totalLucroInvestimento = await _context.CarteirasInvestimento
                .SumAsync(c => (decimal?)c.LucroGerado) ?? 0;

            Relatorio.TotalCarteirasGeraisUsuarios = totalCarteiraGeral;
            Relatorio.TotalInvestidoCarteirasInvestimento = totalInvestido;
            Relatorio.TotalLucroCarteirasInvestimento = totalLucroInvestimento;
            Relatorio.TotalGeralCarteiraInvestimento = totalInvestido + totalLucroInvestimento;

            Relatorio.LucroGeral =
                Relatorio.LucroTaxaSaqueCarteiraGeral +
                Relatorio.LucroTaxaSaqueCarteiraInvestimento +
                Relatorio.LucroAviator;

            await _context.SaveChangesAsync();

            return Page();
        }
    }
}
