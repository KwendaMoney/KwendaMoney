using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KwendaMoney.Models
{
    public class ContaAdmin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NomeBancoOuOperadora { get; set; } // Ex: BAI, BFA, Multicaixa Express

        [Required]
        public string NomeTitular { get; set; }

        [Required]
        public string NumeroOuIban { get; set; } // IBAN ou número Multicaixa Express

        [Required]
        public string Tipo { get; set; } // "IBAN" ou "MulticaixaExpress"

        [ForeignKey("Usuario")]
        public string UsuarioId { get; set; }

        public Usuario Usuario { get; set; }
        public string Descricao => $"{Tipo} - {NumeroOuIban}";
    }
}
