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
    public class PacotesInvestimentoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public PacotesInvestimentoModel(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<PacoteInvestimento> Pacotes { get; set; } = new();
        public CarteiraInvestimento InvestimentoAtivo { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            // Lista todos os pacotes disponíveis
            Pacotes = await _context.PacotesInvestimento.ToListAsync();

            // Verifica se o usuário já tem um investimento ativo
            InvestimentoAtivo = await _context.CarteirasInvestimento
                .Include(c => c.Pacote)
                .FirstOrDefaultAsync(c => c.UsuarioId == usuario.Id && !c.Encerrado);

            return Page();
        }
    }
}
