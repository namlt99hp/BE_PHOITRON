using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class Phuong_An_Phoi
    {
        [Key]
        public int ID { get; set; }
        public string Ten_Phuong_An { get; set; } = string.Empty;
        public int ID_Quang_Dich { get; set; }
        public int Phien_Ban { get; set; } = 1;
        public byte Trang_Thai { get; set; } = 0; // 0=Nháp,1=Hiệu lực,2=Lưu trữ
        public DateTimeOffset Ngay_Tinh_Toan { get; set; } = DateTimeOffset.Now;
        public byte? Muc_Tieu { get; set; }
        public string? Ghi_Chu { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public int? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool Da_Xoa { get; set; } = false;
        // No navigation properties. Relations are handled via ID fields only.
    }
}
