using System;
using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    /// <summary>
    /// Bảng quản lý Lô quặng (MaLoQuang: Lô 1, 2025-01-A...)
    /// </summary>
    public class LoQuang
    {
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Mã lô quặng (Lô 1, 2025-01-A...)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MaLoQuang { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? MoTa { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.Now;
        public int? NguoiTao { get; set; }

        public DateTimeOffset? NgaySua { get; set; }
        public int? NguoiSua { get; set; }
    }
}

