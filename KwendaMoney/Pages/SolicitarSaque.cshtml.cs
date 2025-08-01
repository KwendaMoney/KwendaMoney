using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KwendaMoney.Pages.SaqueUsuario 
{
    [Authorize]
    public class SolicitarSaqueModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEmailSender _emailSender;

        public SolicitarSaqueModel(ApplicationDbContext context, UserManager<Usuario> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<ContaAdmin> ContasUsuario { get; set; } = new();

        [TempData]
        public string MensagemErro { get; set; }

        [TempData]
        public string MensagemSucesso { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Selecione uma conta")]
            public int ContaId { get; set; }

            [Required(ErrorMessage = "Informe um valor")]
            [Range(500, 500000, ErrorMessage = "O valor deve estar entre 500 e 500.000 AOA")]
            public decimal ValorSolicitado { get; set; }
        }

        public bool TemSaquePendente { get; set; } // adiciona isso na classe acima

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            TemSaquePendente = await _context.Saques
                .AnyAsync(s => s.UsuarioId == usuario.Id && s.Status == "Pendente");

            ContasUsuario = await _context.ContasAdmin
                .Where(c => c.UsuarioId == usuario.Id)
                .ToListAsync();

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            ContasUsuario = await _context.ContasAdmin
                .Where(c => c.UsuarioId == usuario.Id)
                .ToListAsync();

            /*
            var possuiSaquePendente = await _context.Saques
    .AnyAsync(s => s.UsuarioId == usuario.Id && s.Status == "Pendente");

            if (possuiSaquePendente)
            {
                MensagemErro = "Voc� j� possui uma solicita��o de saque pendente. Aguarde a an�lise antes de fazer uma nova.";
                return Page();
            }

            */


            if (!ModelState.IsValid)
            {
                MensagemErro = "Corrija os erros do formul�rio.";
                return Page();
            }

            // Verifica se o usu�rio tem saldo suficiente
            if (usuario.SaldoCarteiraGeral < Input.ValorSolicitado)
            {
                MensagemErro = "Saldo insuficiente na carteira geral.";
                return Page();
            }

            var taxa = Math.Round(Input.ValorSolicitado * 0.05m, 2);
            var valorLiquido = Input.ValorSolicitado - taxa;

            var novaSolicitacao = new Models.Saque // <- Usando nome completo para evitar conflito
            {
                UsuarioId = usuario.Id,
                ContaDestinoId = Input.ContaId,
                ValorSolicitado = Input.ValorSolicitado,
                ValorLiquido = valorLiquido,
                TaxaSaqueCarteiraGeral = taxa,
                Status = "Pendente",
                DataSolicitacao = DateTime.Now
            };

            _context.Saques.Add(novaSolicitacao);
            await _context.SaveChangesAsync();

            MensagemSucesso = $"Saque de AOA {valorLiquido.ToString("N2", new CultureInfo("pt-AO"))} solicitado com sucesso e est� em an�lise.";

            // Envia email para o usu�rio
            await _emailSender.SendEmailAsync(
                usuario.Email,
                "Solicita��o de Saque Recebida",
                $@"
                    <p>Ol� {usuario.Nome},</p>
                    <p>Recebemos sua solicita��o de saque no valor de <strong>AOA {Input.ValorSolicitado:N2}</strong>.</p>
                    <p>Uma taxa de 5% (AOA {taxa:N2}) ser� aplicada, e voc� receber� AOA {valorLiquido:N2}.</p>
                    <p>Seu saque est� em an�lise e ser� processado em breve.</p>
                    <p>Equipe KwendaMoney.</p>"
            );

            var admins = await _userManager.GetUsersInRoleAsync("Admin");

            foreach (var admin in admins)
            {
                await _emailSender.SendEmailAsync(
                    admin.Email,
                    "Nova Solicita��o de Saque",
                    $"<p>Ol� administrador,</p>" +
                    $"<p>O usu�rio <strong>{usuario.Nome}</strong> solicitou um saque no valor de <strong>AOA {Input.ValorSolicitado:N2}</strong>.</p>" +
                    $"<p>Taxa de 5%: AOA {taxa:N2} | Valor l�quido: AOA {valorLiquido:N2}</p>" +
                    $"<p>Por favor, acesse o painel de administra��o para revisar e aprovar ou recusar a solicita��o.</p>"
                );
            }


            return RedirectToPage("/Dashboard");
        }
    }
}
