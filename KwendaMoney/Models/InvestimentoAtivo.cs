namespace KwendaMoney.Models
{
    public class InvestimentoAtivo
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public int PacoteInvestimentoId { get; set; }
        public PacoteInvestimento PacoteInvestimento { get; set; }

        public decimal ValorInvestido { get; set; }
        public decimal LucroAcumulado { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? UltimaAcaoDiaria { get; set; }
        public bool Encerrado { get; set; } = false;
    }
}
