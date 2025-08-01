using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace KwendaMoney.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DepositosPendentesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public DepositosPendentesModel(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public List<SolicitacaoDeposito> Depositos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string BuscaNome { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DataInicial { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DataFinal { get; set; }

        [BindProperty]
        public List<int> Ids { get; set; }

        [BindProperty]
        public List<decimal> ValoresCorrigidos { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.SolicitacoesDeposito
                .Include(d => d.Usuario)
                .Include(d => d.ContaAdmin)
                .Where(d => d.Status == "Pendente")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(BuscaNome))
            {
                query = query.Where(d => d.Usuario.Nome.Contains(BuscaNome));
            }

            if (DataInicial.HasValue)
            {
                query = query.Where(d => d.DataSolicitacao >= DataInicial.Value);
            }

            if (DataFinal.HasValue)
            {
                query = query.Where(d => d.DataSolicitacao <= DataFinal.Value);
            }

            Depositos = await query.OrderByDescending(d => d.DataSolicitacao).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string acao)
        {
            if (string.IsNullOrEmpty(acao)) return RedirectToPage();

            var partes = acao.Split('_');
            var comando = partes[0];
            var id = int.Parse(partes[1]);

            var index = Ids.IndexOf(id);
            if (index == -1) return RedirectToPage();

            var deposito = await _context.SolicitacoesDeposito
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deposito == null) return RedirectToPage();

            if (comando == "aprovar")
            {
                var valorCorrigido = ValoresCorrigidos[index];

                deposito.Usuario.SaldoCarteiraGeral += valorCorrigido;
                deposito.ValorInformado = valorCorrigido;
                deposito.Status = "Aprovado";

                await _context.SaveChangesAsync();

                await _emailSender.SendEmailAsync(
                    deposito.Usuario.Email,
                    "Depósito Aprovado",
                    $@"<p>Olá {deposito.Usuario.Nome},</p>
                    <p>Seu depósito de <strong>AOA {valorCorrigido:N2}</strong> foi aprovado com sucesso!</p>
                    <p>O valor já está disponível na sua carteira.</p>
                    <p>Obrigado por usar o KwendaMoney!</p>");
            }
            else if (comando == "rejeitar")
            {
                deposito.Status = "Rejeitado";
                await _context.SaveChangesAsync();

                await _emailSender.SendEmailAsync(
                    deposito.Usuario.Email,
                    "Depósito Rejeitado",
                    $@"<p>Olá {deposito.Usuario.Nome},</p>
                    <p>Infelizmente sua solicitação de depósito foi rejeitada.</p>
                    <p>Verifique os dados enviados e tente novamente.</p>
                    <p>Atenciosamente,<br/>Equipe KwendaMoney</p>");
            }

            return RedirectToPage();
        }
    }
}
