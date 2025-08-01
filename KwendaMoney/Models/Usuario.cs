using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace KwendaMoney.Models
{
    public class Usuario : IdentityUser
    {
        public string Nome { get; set; }
        public decimal SaldoCarteiraGeral { get; set; } = 0;
        public decimal SaldoCarteiraInvestimento { get; set; } = 0;

        public ICollection<Investimento> Investimentos { get; set; }
        public ICollection<Transacao> Transacoes { get; set; }
    }
}
