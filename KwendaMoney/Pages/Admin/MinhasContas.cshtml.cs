using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KwendaMoney.Data;
using KwendaMoney.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace KwendaMoney.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class MinhasContasModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public MinhasContasModel(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<ContaAdmin> ContasExistentes { get; set; }

        [BindProperty]
        public ContaInputModel NovaConta { get; set; }

        public class ContaInputModel
        {
            public int Id { get; set; }

            [Required]
            public string NomeBancoOuOperadora { get; set; }

            [Required]
            public string NomeTitular { get; set; }

            [Required]
            public string NumeroOuIban { get; set; }

            [Required]
            public string Tipo { get; set; } // IBAN ou MulticaixaExpress
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            ContasExistentes = await _context.ContasAdmin
                .Where(c => c.UsuarioId == usuario.Id)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                ContasExistentes = await _context.ContasAdmin
                    .Where(c => c.UsuarioId == usuario.Id)
                    .ToListAsync();
                return Page();
            }

            var conta = new ContaAdmin
            {
                NomeBancoOuOperadora = NovaConta.NomeBancoOuOperadora,
                NomeTitular = NovaConta.NomeTitular,
                NumeroOuIban = NovaConta.NumeroOuIban,
                Tipo = NovaConta.Tipo,
                UsuarioId = usuario.Id
            };

            _context.ContasAdmin.Add(conta);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarContaAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            var conta = await _context.ContasAdmin
                .FirstOrDefaultAsync(c => c.Id == NovaConta.Id && c.UsuarioId == usuario.Id);

            if (conta == null) return RedirectToPage();

            conta.NomeBancoOuOperadora = NovaConta.NomeBancoOuOperadora;
            conta.NomeTitular = NovaConta.NomeTitular;
            conta.NumeroOuIban = NovaConta.NumeroOuIban;
            conta.Tipo = NovaConta.Tipo;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExcluirAsync(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            var conta = await _context.ContasAdmin
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuario.Id);

            if (conta != null)
            {
                _context.ContasAdmin.Remove(conta);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
