using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class VaiTro
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(100)]
        public string MaVaiTro { get; set; } = string.Empty;  // Ví dụ: ADMIN, KEHOACH

        [Required, MaxLength(200)]
        public string TenVaiTro { get; set; } = string.Empty; // Tên hiển thị

        public bool LaHeThong { get; set; } = false;          // Có phải vai trò hệ thống không

        [MaxLength(500)]
        public string? MoTa { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int NguoiTao { get; set; }

        public DateTime? NgaySua { get; set; }
        public int? NguoiSua { get; set; }
    }
}