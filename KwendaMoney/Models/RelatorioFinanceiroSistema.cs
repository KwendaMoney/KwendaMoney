using System.ComponentModel.DataAnnotations;

namespace KwendaMoney.Models
{
    public class RelatorioFinanceiroSistema
    {
        [Key]
        public int Id { get; set; }

        public decimal LucroTaxaSaqueCarteiraGeral { get; set; }
        public decimal LucroTaxaSaqueCarteiraInvestimento { get; set; }
        public decimal LucroAviator { get; set; }
        public decimal LucroGeral { get; set; }

        public decimal TotalCarteirasGeraisUsuarios { get; set; }
        public decimal TotalInvestidoCarteirasInvestimento { get; set; }
        public decimal TotalLucroCarteirasInvestimento { get; set; }
        public decimal TotalGeralCarteiraInvestimento { get; set; }
    }
}
