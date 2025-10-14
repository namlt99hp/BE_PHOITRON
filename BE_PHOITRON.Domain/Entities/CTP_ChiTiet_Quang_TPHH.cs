using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities
{
    public class CTP_ChiTiet_Quang_TPHH
    {
        [Key]
        public int ID { get; set; }
        public int ID_CTP_ChiTiet_Quang { get; set; }
        public int ID_TPHH { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Gia_Tri_PhanTram { get; set; }
        public bool Da_Xoa { get; set; } = false;

        // No navigation properties. Relations are handled via ID fields only.
    }
}
