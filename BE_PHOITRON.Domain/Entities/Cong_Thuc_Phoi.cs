using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class Cong_Thuc_Phoi
    {
        [Key]
        public int ID { get; set; }
        public int ID_Quang_DauRa { get; set; }
        public string Ma_Cong_Thuc { get; set; } = string.Empty;
        public string? Ten_Cong_Thuc { get; set; }
        public decimal He_So_Thu_Hoi { get; set; } = 1.0000m;
        public decimal Chi_Phi_Cong_Doạn_1Tan { get; set; } = 0;
        public int Phien_Ban { get; set; } = 1;
        public byte Trang_Thai { get; set; } = 0; // 0=Nháp,1=Hiệu lực,2=Lưu trữ
        public DateTimeOffset Hieu_Luc_Tu { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? Hieu_Luc_Den { get; set; }
        public string? Ghi_Chu { get; set; }
        public DateTimeOffset Ngay_Tao { get; set; } = DateTimeOffset.Now;
        public int? Nguoi_Tao { get; set; }
        public DateTimeOffset? Ngay_Sua { get; set; }
        public int? Nguoi_Sua { get; set; }
        public bool Da_Xoa { get; set; } = false;

        // No navigation properties. Relations are handled via ID fields only.
    }
}
