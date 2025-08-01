using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KwendaMoney.Models;
using System.Threading.Tasks;

namespace KwendaMoney.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<Usuario> _userManager;

        public DashboardModel(UserManager<Usuario> userManager)
        {
            _userManager = userManager;
        }

        public Usuario UsuarioAtual { get; set; }

        public async Task OnGetAsync()
        {
            UsuarioAtual = await _userManager.GetUserAsync(User);
        }
    }
}
