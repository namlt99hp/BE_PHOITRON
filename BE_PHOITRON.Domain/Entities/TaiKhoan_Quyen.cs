using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class TaiKhoan_Quyen
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ID_TaiKhoan { get; set; }   // Tham chiếu TaiKhoan.ID

        [Required]
        public int ID_Quyen { get; set; }      // Tham chiếu Quyen.ID

        [Required]
        public bool DuocCap { get; set; } = true; // true: được cấp, false: thu hồi

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}