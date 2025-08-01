using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KwendaMoney.Models
{
    public class SolicitacaoDeposito
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal ValorInformado { get; set; }

        [Required]
        public string IdTransacao { get; set; }

        [Required]
        public string TipoDeposito { get; set; } // "IBAN" ou "MulticaixaExpress"

        public int ContaDestinoId { get; set; } // Referência à conta bancária ou número multicaixa

        [Required]
        public string CaminhoComprovativo { get; set; }

        public string Status { get; set; } = "Pendente"; // Pendente, Aprovado, Rejeitado

        public DateTime DataSolicitacao { get; set; } = DateTime.Now;

        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; }

        public Usuario Usuario { get; set; }


        [ForeignKey("ContaDestinoId")]
        public ContaAdmin ContaAdmin { get; set; }

    }
}
