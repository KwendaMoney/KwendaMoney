// Pages/Admin/SaquesPendentes.cshtml.cs
using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KwendaMoney.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SaquesPendentesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public SaquesPendentesModel(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public List<Saque> Saques { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string BuscaNome { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DataInicial { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? DataFinal { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1;

        public int TotalPaginas { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var query = _context.Saques
                .Where(s => s.Status == "Pendente")
                .Include(s => s.Usuario)
                .Include(s => s.ContaDestino)
                .AsQueryable();

            if (!string.IsNullOrEmpty(BuscaNome))
            {
                query = query.Where(s => s.Usuario.Nome.Contains(BuscaNome));
            }

            if (DataInicial.HasValue)
            {
                query = query.Where(s => s.DataSolicitacao >= DataInicial.Value);
            }

            if (DataFinal.HasValue)
            {
                query = query.Where(s => s.DataSolicitacao <= DataFinal.Value);
            }

            int totalRegistros = await query.CountAsync();
            int tamanhoPagina = 10;
            TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanhoPagina);

            Saques = await query
                .OrderByDescending(s => s.DataSolicitacao)
                .Skip((Pagina - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string acao)
        {
            if (string.IsNullOrEmpty(acao)) return RedirectToPage();

            var partes = acao.Split('_');
            var comando = partes[0];
            var id = int.Parse(partes[1]);

            var saque = await _context.Saques
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (saque == null) return RedirectToPage();

            if (comando == "aprovar")
            {
                if (saque.Usuario.SaldoCarteiraGeral >= saque.ValorSolicitado)
                {
                    saque.Usuario.SaldoCarteiraGeral -= saque.ValorSolicitado;
                    saque.Status = "Aprovado";

                    var relatorio = await _context.RelatorioFinanceiroSistema.FirstOrDefaultAsync();
                    if (relatorio != null)
                        relatorio.LucroTaxaSaqueCarteiraGeral += saque.TaxaSaqueCarteiraGeral;

                    await _emailSender.SendEmailAsync(
                        saque.Usuario.Email,
                        "Saque Aprovado",
                        $"<p>Olá {saque.Usuario.Nome},</p><p>Seu saque de <strong>AOA {saque.ValorSolicitado:N2}</strong> foi aprovado. Você receberá AOA {saque.ValorLiquido:N2}.</p><p>Equipe KwendaMoney</p>");
                }
                else
                {
                    saque.Status = "Rejeitado";
                    await _emailSender.SendEmailAsync(
                        saque.Usuario.Email,
                        "Saque Rejeitado",
                        $"<p>Olá {saque.Usuario.Nome},</p><p>Seu saque de AOA {saque.ValorSolicitado:N2} foi rejeitado por saldo insuficiente.</p><p>Equipe KwendaMoney</p>");
                }
            }
            else if (comando == "rejeitar")
            {
                saque.Status = "Rejeitado";
                await _emailSender.SendEmailAsync(
                    saque.Usuario.Email,
                    "Saque Rejeitado",
                    $"<p>Olá {saque.Usuario.Nome},</p><p>Seu saque de AOA {saque.ValorSolicitado:N2} foi rejeitado após análise.</p><p>Equipe KwendaMoney</p>");
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
