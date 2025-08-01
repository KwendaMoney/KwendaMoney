using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KwendaMoney.Models
{
    public enum TipoTransacao
    {
        Deposito,
        Saque,
        Investimento,
        Resgate,
        TarefaDiaria,
        ApostaGanhar,
        ApostaPerder,
        Taxa
    }

    public class Transacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        public TipoTransacao Tipo { get; set; }

        [Required]
        public decimal Valor { get; set; }

        [Required]
        public DateTime Data { get; set; }
    }
}
