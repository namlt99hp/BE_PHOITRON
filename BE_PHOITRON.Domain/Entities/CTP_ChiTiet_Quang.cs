using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BE_PHOITRON.Domain.Entities
{
    public class CTP_ChiTiet_Quang
    {
        [Key]
        public int ID { get; set; }
        public int ID_Cong_Thuc_Phoi { get; set; }
        public int ID_Quang_DauVao { get; set; }
        [Column(TypeName = "decimal(18,4)")]
        public decimal Ti_Le_Phan_Tram { get; set; }
        [Column("Khau_Hao", TypeName = "decimal(18,4)")]
        public decimal? Khau_Hao { get; set; }
        public int? Thu_Tu { get; set; }
        public string? Ghi_Chu { get; set; }
        public bool Da_Xoa { get; set; } = false;

        // New columns present in DB schema
        [Column("Ti_Le_KhaoHao", TypeName = "decimal(18,4)")]
        public decimal? Ti_Le_KhaoHao { get; set; }

        [Column("KL_VaoLo", TypeName = "decimal(18,4)")]
        public decimal? KL_VaoLo { get; set; }

        [Column("Ti_Le_HoiQuang", TypeName = "decimal(18,4)")]
        public decimal? Ti_Le_HoiQuang { get; set; }

        [Column("KL_Nhan", TypeName = "decimal(18,4)")]
        public decimal? KL_Nhan { get; set; }

        // No navigation properties. Relations are handled via ID fields only.
    }
}
