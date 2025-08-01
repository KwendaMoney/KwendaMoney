using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KwendaMoney.Pages.Investimento
{
    [Authorize]
    public class ConfirmarInvestimentoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public ConfirmarInvestimentoModel(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public int PacoteId { get; set; }

        public PacoteInvestimento Pacote { get; set; }

        public decimal SaldoCarteiraGeral { get; set; }

        public async Task<IActionResult> OnGetAsync(int pacoteId)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToPage("/Account/Login");

            SaldoCarteiraGeral = usuario.SaldoCarteiraGeral;
            Pacote = await _context.PacotesInvestimento.FirstOrDefaultAsync(p => p.Id == pacoteId);
            if (Pacote == null) return RedirectToPage("/Investimento/PacotesInvestimento");

            PacoteId = pacoteId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToPage("/Account/Login");

            var pacote = await _context.PacotesInvestimento.FirstOrDefaultAsync(p => p.Id == PacoteId);
            if (pacote == null) return RedirectToPage("/Investimento/PacotesInvestimento");

            // Verifica se já possui um investimento ativo
            bool temInvestimento = await _context.CarteirasInvestimento
                .AnyAsync(c => c.UsuarioId == usuario.Id && !c.Encerrado);
            if (temInvestimento)
            {
                TempData["Mensagem"] = "Você já possui um investimento ativo.";
                return RedirectToPage("/Investimento/PacotesInvestimento");
            }

            // Verifica saldo
            if (usuario.SaldoCarteiraGeral < pacote.Valor)
            {
                TempData["Mensagem"] = "Saldo insuficiente para este investimento.";
                return RedirectToPage("/Investimento/PacotesInvestimento");
            }

            // Criar investimento
            var novaCarteira = new CarteiraInvestimento
            {
                UsuarioId = usuario.Id,
                PacoteInvestimentoId = pacote.Id,
                ValorInvestido = pacote.Valor,
                LucroGerado = 0,
                DiasAcumulados = 0,
                DataUltimaTarefa = DateTime.MinValue,
                DataUltimoDeposito = DateTime.Now,
                Encerrado = false
            };

            usuario.SaldoCarteiraGeral -= pacote.Valor;

            _context.CarteirasInvestimento.Add(novaCarteira);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Investimento iniciado com sucesso!";
            return RedirectToPage("/Investimento/TarefaInvestimento");
        }
    }
}
