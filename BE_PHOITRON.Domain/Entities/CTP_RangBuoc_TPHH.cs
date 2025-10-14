using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities
{
    public class CTP_RangBuoc_TPHH
    {
        [Key]
        public int ID { get; set; }
        public int ID_Cong_Thuc_Phoi { get; set; }
        public int ID_TPHH { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal? Min_PhanTram { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal? Max_PhanTram { get; set; }
        public bool Rang_Buoc_Cung { get; set; } = true;
        public byte? Uu_Tien { get; set; }
        public string? Ghi_Chu { get; set; }
        public bool Da_Xoa { get; set; } = false;

        // No navigation properties. Relations are handled via ID fields only.
    }
}
