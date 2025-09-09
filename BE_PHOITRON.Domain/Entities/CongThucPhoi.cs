using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.DataEntities
{
    public class CongThucPhoi
    {
        [Key]
        public int ID { get; set; }
        public string MaCongThuc { get; set; }
        public string? TenCongThuc { get; set; }
        public decimal? TongPhanTram { get; set; }
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; }
        public int? ID_NguoiTao { get; set; }
        public DateTime? NgaySua { get; set; }
        public int? ID_NguoiSua { get; set; }
        public bool IsDeleted { get; set; }
        public int? ID_QuangNeo { get; set; }
    }
}
