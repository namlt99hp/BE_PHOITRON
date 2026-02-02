using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities
{
    public class Quang_TP_PhanTich
    {
        [Key]
        public int ID { get; set; }
        public int ID_Quang { get; set; }
        public int ID_TPHH { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Gia_Tri_PhanTram { get; set; }
        public DateTimeOffset Hieu_Luc_Tu { get; set; }
        public DateTimeOffset? Hieu_Luc_Den { get; set; }
        public string? Nguon_Du_Lieu { get; set; }
        public string? Ghi_Chu { get; set; }
        public int? ThuTuTPHH { get; set; }
        public bool Da_Xoa { get; set; } = false;
        public decimal? KhoiLuong { get; set; }
        // Formula calculation fields
        public string? CalcFormula { get; set; }
        public bool? IsCalculated { get; set; } = false;
        
        /// <summary>
        /// Đánh dấu đây là template (Gia_Tri_PhanTram chỉ là mẫu, không phải giá trị thực tế)
        /// Khi clone vào phương án, sẽ set Gia_Tri_PhanTram = 0 và Is_Template = false
        /// </summary>
        public bool? Is_Template { get; set; } = false;
    }
}
