using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KwendaMoney.Models
{
    public class CarteiraInvestimento
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public int PacoteInvestimentoId { get; set; }
        public PacoteInvestimento Pacote { get; set; }

        public decimal ValorInvestido { get; set; }
        public decimal LucroGerado { get; set; }

        public DateTime DataInicio { get; set; } = DateTime.Now;

        public DateTime DataUltimaTarefa { get; set; } = DateTime.Now;

        public DateTime DataUltimoDeposito { get; set; } = DateTime.MinValue;

        public int DiasAcumulados { get; set; } = 0;

        public bool Encerrado { get; set; } = false;
    }
}
