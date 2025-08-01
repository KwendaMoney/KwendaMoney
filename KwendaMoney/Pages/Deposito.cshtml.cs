using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KwendaMoney.Data;
using KwendaMoney.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;

namespace KwendaMoney.Pages
{
    [Authorize]
    public class DepositoModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender _emailSender;

        public DepositoModel(ApplicationDbContext context, UserManager<Usuario> userManager, IWebHostEnvironment environment, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _emailSender = emailSender;
        }

        public List<ContaAdmin> ContasDisponiveis { get; set; } = new();
        public bool TemSolicitacaoPendente { get; set; }

        [TempData]
        public string MensagemErro { get; set; }

        [TempData]
        public string MensagemSucesso { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int ContaDestinoId { get; set; }

            [Required]
            [Range(200.00, 100000.00, ErrorMessage = "O valor deve estar entre 200 AOA e 100.000 AOA.")]
            public decimal ValorInformado { get; set; }

            [Required]
            public string IdTransacao { get; set; }

            [Required]
            public IFormFile Comprovativo { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);

            TemSolicitacaoPendente = await _context.SolicitacoesDeposito
                .AnyAsync(d => d.UsuarioId == usuario.Id && d.Status == "Pendente");

            var adminIds = await _userManager.GetUsersInRoleAsync("Admin");
            var ids = adminIds.Select(a => a.Id).ToList();

            ContasDisponiveis = await _context.ContasAdmin
                .Where(c => ids.Contains(c.UsuarioId))
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int contaDestinoId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            // ⚠️ Verifica se já há um depósito pendente
            TemSolicitacaoPendente = await _context.SolicitacoesDeposito
                .AnyAsync(d => d.UsuarioId == usuario.Id && d.Status == "Pendente");

            if (TemSolicitacaoPendente)
            {
                MensagemErro = "Você já possui uma solicitação de depósito pendente.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                MensagemErro = "Preencha todos os campos corretamente.";
                return RedirectToPage();
            }
            // 🔐 Verificar tipo do comprovativo
            var extensao = Path.GetExtension(Input.Comprovativo.FileName).ToLowerInvariant();
            var extensoesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

            if (!extensoesPermitidas.Contains(extensao))
            {
                MensagemErro = "O arquivo deve ser uma imagem (JPEG, PNG) ou PDF.";
                return RedirectToPage();
            }



            var fileName = Guid.NewGuid() + Path.GetExtension(Input.Comprovativo.FileName);
            var relativePath = Path.Combine("comprovativos", fileName);
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

            using var stream = new FileStream(absolutePath, FileMode.Create);
            await Input.Comprovativo.CopyToAsync(stream);

            var solicitacao = new SolicitacaoDeposito
            {
                ContaDestinoId = contaDestinoId,
                ValorInformado = decimal.Round(Input.ValorInformado, 2),
                IdTransacao = Input.IdTransacao,
                CaminhoComprovativo = "/" + relativePath.Replace("\\", "/"),
                Status = "Pendente",
                DataSolicitacao = DateTime.Now,
                UsuarioId = usuario.Id,
                TipoDeposito = "Manual"
            };

            _context.SolicitacoesDeposito.Add(solicitacao);
            await _context.SaveChangesAsync();

            MensagemSucesso = $"Depósito de AOA {solicitacao.ValorInformado:N2} solicitado com sucesso.";

            // ✅ E-mail para o usuário
            if (!string.IsNullOrWhiteSpace(usuario.Email))
            {
                await _emailSender.SendEmailAsync(
                    usuario.Email,
                    "Solicitação de Depósito Recebida",
                    $@"
                    <p>Olá {usuario.Nome},</p>
                    <p>Recebemos sua solicitação de depósito no valor de <strong>AOA {solicitacao.ValorInformado:N2}</strong>.</p>
                    <p>Seu depósito está em análise e será validado em breve.</p>
                    <p>Equipe KwendaMoney</p>"
                );
            }

            // ✅ E-mail para admins
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                if (!string.IsNullOrEmpty(admin.Email))
                {
                    await _emailSender.SendEmailAsync(
                        admin.Email,
                        "Nova Solicitação de Depósito",
                        $@"
                        <p>Olá Admin,</p>
                        <p>O usuário <strong>{usuario.Nome}</strong> solicitou um depósito de <strong>AOA {solicitacao.ValorInformado:N2}</strong>.</p>
                        <p>ID da transação: {solicitacao.IdTransacao}</p>
                        <p><a href='{solicitacao.CaminhoComprovativo}' target='_blank'>Ver Comprovativo</a></p>"
                    );
                }
            }

            return RedirectToPage();
        }
    }
}
