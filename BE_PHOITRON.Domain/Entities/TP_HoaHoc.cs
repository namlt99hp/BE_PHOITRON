using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.DataEntities
{
    public class TP_HoaHoc
    {
        [Key]
        public int ID { get; set; }
        public string Ma_TPHH { get; set; }
        public string? Ten_TPHH { get; set; }
        public string? GhiChu { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? ID_NguoiTao { get; set; }
        public DateTime? NgaySua { get; set; }
        public int? ID_NguoiSua { get; set; }
        public bool IsDeleted { get; set; }
    }
}
