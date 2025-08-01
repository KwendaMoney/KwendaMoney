using System;
using KwendaMoney.Models;

namespace KwendaMoney.Models
{
    public class Saque
    {
        public int Id { get; set; }

        public decimal ValorSolicitado { get; set; }

        public decimal ValorLiquido { get; set; } // Valor a receber após a taxa

        public decimal TaxaSaqueCarteiraGeral { get; set; } // 5% de taxa

        public decimal TaxaSaqueCarteiraInvestimento { get; set; } // Reservado para o futuro

        public string Status { get; set; } // Pendente, Aprovado, Rejeitado

        public string UsuarioId { get; set; }

        public Usuario Usuario { get; set; }

        public int ContaDestinoId { get; set; }

        public ContaAdmin ContaDestino { get; set; }

        public DateTime DataSolicitacao { get; set; } = DateTime.Now;
    }
}
