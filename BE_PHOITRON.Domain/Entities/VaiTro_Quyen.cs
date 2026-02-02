using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class VaiTro_Quyen
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ID_VaiTro { get; set; }   // Tham chiếu VaiTro.ID

        [Required]
        public int ID_Quyen { get; set; }    // Tham chiếu Quyen.ID

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}