using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BE_PHOITRON.Domain.Entities
{
    public class PA_Snapshot_TPHH
    {
        [Key]
        public int ID { get; set; }
        public int ID_Phuong_An { get; set; }
        public int ID_Quang { get; set; }
        public int ID_TPHH { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Gia_Tri_PhanTram { get; set; }

        
    }
}
