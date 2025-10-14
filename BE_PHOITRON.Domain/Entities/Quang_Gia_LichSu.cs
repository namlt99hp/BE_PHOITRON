using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class Quang_Gia_LichSu
    {
        [Key]
        public int ID { get; set; }
        public int ID_Quang { get; set; }
        public decimal Don_Gia_USD_1Tan { get; set; }
        public decimal Don_Gia_VND_1Tan { get; set; }
        public decimal Ty_Gia_USD_VND { get; set; }
        public string Tien_Te { get; set; } = "USD";
        public DateTimeOffset Hieu_Luc_Tu { get; set; }
        public DateTimeOffset? Hieu_Luc_Den { get; set; }
        public string? Ghi_Chu { get; set; }
        public bool Da_Xoa { get; set; } = false;

        
    }
}
