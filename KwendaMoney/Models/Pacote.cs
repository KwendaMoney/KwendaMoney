using System.ComponentModel.DataAnnotations;

namespace KwendaMoney.Models
{
    public class Pacote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public decimal ValorInvestido { get; set; }

        [Required]
        public int DuracaoDias { get; set; }

        [Required]
        public decimal LucroDiario { get; set; }
    }
}

