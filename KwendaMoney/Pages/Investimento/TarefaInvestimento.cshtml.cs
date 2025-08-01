using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace KwendaMoney.Pages.Investimento
{
    [Authorize]
    public class TarefaInvestimentoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public TarefaInvestimentoModel(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public CarteiraInvestimento Carteira { get; set; }

        [TempData]
        public string Mensagem { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            Carteira = await _context.CarteirasInvestimento
                .Include(c => c.Pacote)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id && !c.Encerrado);

            return Page();
        }

        public async Task<IActionResult> OnPostTarefaAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            var carteira = await _context.CarteirasInvestimento
                .Include(c => c.Pacote)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id && !c.Encerrado);

            if (carteira == null)
            {
                Mensagem = "Nenhum investimento ativo encontrado.";
                return RedirectToPage();
            }

            if (carteira.DataUltimaTarefa.Date == DateTime.Now.Date)
            {
                Mensagem = "A tarefa diária já foi realizada hoje.";
                return RedirectToPage();
            }

            if (carteira.DiasAcumulados >= carteira.Pacote.DiasMaximos)
            {
                Mensagem = "Você já atingiu o limite de dias deste investimento.";
                return RedirectToPage();
            }

            carteira.LucroGerado += carteira.Pacote.LucroDiario;
            carteira.DiasAcumulados++;
            carteira.DataUltimaTarefa = DateTime.Now;

            await _context.SaveChangesAsync();

            Mensagem = $"Tarefa realizada com sucesso! Lucro diário de AOA {carteira.Pacote.LucroDiario.ToString("N2", new CultureInfo("pt-AO"))} adicionado.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSacarLucroAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            var carteira = await _context.CarteirasInvestimento
                .Include(c => c.Pacote)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id && !c.Encerrado);

            if (carteira == null || carteira.LucroGerado < carteira.ValorInvestido * 0.5m)
            {
                Mensagem = "O saque só é permitido se o lucro for igual ou superior a 50% do valor investido.";
                return RedirectToPage();
            }

            var total = carteira.ValorInvestido + carteira.LucroGerado;
            var taxa = total * 0.1m;
            var valorLiquido = total - taxa;

            usuario.SaldoCarteiraGeral += valorLiquido;
            carteira.Encerrado = true;
            carteira.DataUltimoDeposito = DateTime.Now;

            // ✅ Cria ou atualiza o relatório
            var relatorio = await _context.RelatorioFinanceiroSistema.FirstOrDefaultAsync();
            if (relatorio == null)
            {
                relatorio = new RelatorioFinanceiroSistema();
                _context.RelatorioFinanceiroSistema.Add(relatorio);
            }

            relatorio.LucroTaxaSaqueCarteiraInvestimento += taxa;

            await _context.SaveChangesAsync();

            Mensagem = $"Saque realizado com sucesso! Valor líquido de AOA {valorLiquido:N2} transferido para sua carteira geral.";
            return RedirectToPage();
        }



        public async Task<IActionResult> OnPostCancelarAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            var carteira = await _context.CarteirasInvestimento
                .Include(c => c.Pacote)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id && !c.Encerrado);

            if (carteira == null)
            {
                Mensagem = "Nenhum investimento ativo encontrado.";
                return RedirectToPage();
            }

            var taxa = carteira.ValorInvestido * 0.1m;
            var valorComDesconto = carteira.ValorInvestido - taxa;

            usuario.SaldoCarteiraGeral += valorComDesconto;
            carteira.LucroGerado = 0;
            carteira.Encerrado = true;
            carteira.DataUltimoDeposito = DateTime.Now;

            // ✅ Cria ou atualiza o relatório
            var relatorio = await _context.RelatorioFinanceiroSistema.FirstOrDefaultAsync();
            if (relatorio == null)
            {
                relatorio = new RelatorioFinanceiroSistema();
                _context.RelatorioFinanceiroSistema.Add(relatorio);
            }

            relatorio.LucroTaxaSaqueCarteiraInvestimento += taxa;

            await _context.SaveChangesAsync();

            Mensagem = $"Investimento cancelado. Você recebeu AOA {valorComDesconto:N2} na sua carteira geral (10% de taxa aplicada).";
            return RedirectToPage();
        }


    }
}
