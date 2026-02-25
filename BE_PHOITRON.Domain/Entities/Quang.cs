using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities
{
    public enum LoaiQuangEnum : byte
    {
        
        Tron = 1,
        Gang = 2,
        NhienLieu = 3,
        Xi = 4,
        QuangCo = 5,
        QuangVeVien = 6,
        QuangPA = 7,
        Mua = 8,
    }

    public class Quang
    {
        [Key]
        public int ID { get; set; }
        public string Ma_Quang { get; set; } = string.Empty;
        public string? Ten_Quang { get; set; }
        /// <summary>
        /// Khóa ngoại tới bảng LoaiQuang (danh mục loại quặng - ID trùng với enum LoaiQuangEnum value)
        /// </summary>
        [Required]
        public int ID_LoaiQuang { get; set; }

        /// <summary>
        /// Khóa ngoại tới bảng LoQuang (danh mục lô quặng)
        /// </summary>
        public int? ID_LoQuang { get; set; }

        public bool Dang_Hoat_Dong { get; set; } = true;
        public bool Da_Xoa { get; set; } = false;
        public string? Ghi_Chu { get; set; }
        public DateTimeOffset Ngay_Tao { get; set; } = DateTimeOffset.Now;
        public int? Nguoi_Tao { get; set; }
        public DateTimeOffset? Ngay_Sua { get; set; }
        public int? Nguoi_Sua { get; set; }

        // Link Xỉ (slag) ore to its target Gang ore when applicable
        public int? ID_Quang_Gang { get; set; }
        public bool? Is_Template { get; set; } = false;
    }
}