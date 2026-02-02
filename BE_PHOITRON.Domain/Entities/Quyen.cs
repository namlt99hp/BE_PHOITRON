using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class Quyen
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(100)]
        public string MaQuyen { get; set; } = string.Empty;  // Mã quyền, ví dụ: BM16.XEM

        [Required, MaxLength(200)]
        public string TenQuyen { get; set; } = string.Empty; // Tên hiển thị

        [MaxLength(100)]
        public string? NhomChucNang { get; set; }            // Nhóm/module (ví dụ: BM16, QUANG)

        [MaxLength(500)]
        public string? MoTa { get; set; }                    // Mô tả chi tiết

        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int NguoiTao { get; set; }

        public DateTime? NgaySua { get; set; }
        public int? NguoiSua { get; set; }
    }
}