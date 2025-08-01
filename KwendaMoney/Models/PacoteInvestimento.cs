namespace KwendaMoney.Models
{
    public class PacoteInvestimento
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int DiasMaximos { get; set; }
        public decimal LucroDiario { get; set; }
    }
}
