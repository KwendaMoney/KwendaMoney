using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace KwendaMoney.Pages
{
    [Authorize]
    public class DepositoInvestimentoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public DepositoInvestimentoModel(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string MensagemErro { get; set; }

        [TempData]
        public string MensagemSucesso { get; set; }

        public class InputModel
        {
            [Required]
            [Range(1, 1000000, ErrorMessage = "Insira um valor válido.")]
            public decimal Valor { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                MensagemErro = "Preencha corretamente o valor.";
                return Page();
            }

            if (usuario.SaldoCarteiraGeral < Input.Valor)
            {
                MensagemErro = "Saldo insuficiente na carteira geral.";
                return Page();
            }

            var carteira = await _context.CarteirasInvestimento
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id);

            if (carteira == null)
            {
                carteira = new CarteiraInvestimento
                {
                    UsuarioId = usuario.Id,
                    ValorInvestido = Input.Valor,
                    LucroGerado = 0
                };

                _context.CarteirasInvestimento.Add(carteira);
            }
            else
            {
                carteira.ValorInvestido += Input.Valor;
                carteira.DataUltimoDeposito = DateTime.Now;
            }

            usuario.SaldoCarteiraGeral -= Input.Valor;

            await _context.SaveChangesAsync();

            MensagemSucesso = $"Depósito de AOA {Input.Valor.ToString("N2", new CultureInfo("pt-AO"))} realizado com sucesso.";

            return RedirectToPage("/Dashboard");
        }
    }
}
