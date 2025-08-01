using Microsoft.AspNetCore.Identity;

namespace KwendaMoney.Models
{
    public class Usuario : IdentityUser
    {
        public string Nome { get; set; }
        public decimal SaldoCarteiraGeral { get; set; } = 0;
        public decimal SaldoCarteiraInvestimento { get; set; } = 0;
    }
}
