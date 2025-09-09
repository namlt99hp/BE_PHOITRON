using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.DataEntities
{
    public class Quang
    {
        [Key]
        public int ID { get; set; }
        public string MaQuang { get; set; }
        public string? TenQuang { get; set; }
        public decimal? Gia { get; set; }
        public decimal? MatKhiNung { get; set; }
        public string? GhiChu { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? ID_NguoiTao { get; set; }
        public DateTime? NgaySua { get; set; }
        public int? ID_NguoiSua { get; set; }
        public bool IsDeleted { get; set; }
        public int LoaiQuang { get; set; }
        public int? ID_CongThucPhoi { get; set; }
    }
}
