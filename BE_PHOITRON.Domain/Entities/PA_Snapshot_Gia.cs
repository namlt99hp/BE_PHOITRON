using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class PA_Snapshot_Gia
    {
        [Key]
        public int ID { get; set; }
        // Scope
        public int? ID_Phuong_An { get; set; } // null = standalone
        public int ID_Cong_Thuc_Phoi { get; set; }

        // Item context
        public int ID_Quang { get; set; }
        public decimal Ti_Le_Phan_Tram { get; set; } // ratio at snapshot time
        public decimal? He_So_Hao_Hut_DauVao { get; set; }
        public decimal Tieu_Hao_PhanTram { get; set; } // computed

        // Pricing
        public decimal Don_Gia_1Tan { get; set; }
        public string Tien_Te { get; set; } = "USD";
        public decimal? Ty_Gia_USD_VND { get; set; }
        public decimal? Don_Gia_VND_1Tan { get; set; }
        public decimal? Chi_Phi_Theo_Ti_Le_VND { get; set; }
        public int? Nguon_Gia_ID { get; set; }

        // Versioning & ownership
        public int Version_No { get; set; } = 1;
        public bool Is_Active { get; set; } = true;
        public int? Price_Override_By_User_ID { get; set; }
        public byte? Scope { get; set; } // 0=Global,1=User,2=Department

        // Audit
        public string? Ghi_Chu { get; set; }
        public DateTimeOffset Created_At { get; set; } = DateTimeOffset.UtcNow;
        public int? Created_By_User_ID { get; set; }
        public DateTimeOffset? Effective_At { get; set; }
    }
}
