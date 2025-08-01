using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using KwendaMoney.Models;

namespace KwendaMoney.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<Usuario> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O campo E-mail � obrigat�rio.")]
            [EmailAddress(ErrorMessage = "O e-mail inserido n�o � v�lido.")]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // N�o revelar que o usu�rio n�o existe ou n�o confirmou e-mail
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));


            var callbackUrl = Url.Page(
    "/Account/ResetPassword",
    pageHandler: null,
    values: new { area = "Identity", code = encodedCode, email = user.Email },
    protocol: Request.Scheme);




            var corpoEmail = $@"
<p>Ol� {user.Nome},</p>
<p>Recebemos uma solicita��o para redefinir sua senha no <strong>KwendaMoney</strong>.</p>
<p>Clique no bot�o abaixo para escolher uma nova senha:</p>
<p><a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Redefinir senha</a></p>
<p>Se voc� n�o solicitou isso, ignore este e-mail.</p>";

            await _emailSender.SendEmailAsync(Input.Email, "Redefinir sua senha - KwendaMoney", corpoEmail);

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
