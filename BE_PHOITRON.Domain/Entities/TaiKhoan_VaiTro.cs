using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class TaiKhoan_VaiTro
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ID_TaiKhoan { get; set; }   // Tham chiếu tới bảng TaiKhoan.ID (không FK)

        [Required]
        public int ID_VaiTro { get; set; }     // Tham chiếu tới bảng VaiTro.ID (không FK)

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
    }
}