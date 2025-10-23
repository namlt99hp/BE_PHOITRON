using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities
{
    [Table("CTP_BangChiPhi")]
    public class CTP_BangChiPhi
    {
        [Key]
        public int ID { get; set; }

        // SQL column name: ID_CongThucPhoi (not nullable)
        [Column("ID_CongThucPhoi")]
        public int ID_CongThuc_Phoi { get; set; }

        // SQL column name: ID_Quang (nullable)
        [Column("ID_Quang")]
        public int? ID_Quang { get; set; }

        // SQL column name: LineType (varchar(30), not nullable)
        [Required]
        [Column("LineType", TypeName = "varchar(30)")]
        public string LineType { get; set; } = string.Empty;

        // SQL column name: Tieuhao (decimal(18,6), nullable)
        [Column("Tieuhao", TypeName = "decimal(18,6)")]
        public decimal? Tieuhao { get; set; }

        // SQL column name: DonGiaVND (decimal(18,6), nullable)
        [Column("DonGiaVND", TypeName = "decimal(18,6)")]
        public decimal? DonGiaVND { get; set; }

        // SQL column name: DonGiaUSD (decimal(18,6), not nullable)
        [Column("DonGiaUSD", TypeName = "decimal(18,6)")]
        public decimal DonGiaUSD { get; set; }
    }
}


