using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KwendaMoney.Models
{
    public class Investimento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [Required]
        public string Pacote { get; set; }

        [Required]
        public decimal ValorInvestido { get; set; }

        public decimal LucroDiario { get; set; }

        public DateTime DataInicio { get; set; }

        public int DiasDuracao { get; set; }

        public bool Ativo { get; set; } = true;
    }
}

