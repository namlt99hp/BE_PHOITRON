using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class TP_HoaHoc
    {
        [Key]
        public int ID { get; set; }
        public string Ma_TPHH { get; set; } = string.Empty;
        public string? Ten_TPHH { get; set; }
        public string Don_Vi { get; set; } = "%";
        public int? Thu_Tu { get; set; }
        public string? Ghi_Chu { get; set; }
        public bool Da_Xoa { get; set; } = false;
        public DateTimeOffset Ngay_Tao { get; set; } = DateTimeOffset.Now;
        public int? Nguoi_Tao { get; set; }
        public DateTimeOffset? Ngay_Sua { get; set; }
        public int? Nguoi_Sua { get; set; }
    }
}