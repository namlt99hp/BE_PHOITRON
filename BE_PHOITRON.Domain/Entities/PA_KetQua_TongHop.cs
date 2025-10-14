using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class PA_KetQua_TongHop
    {
        [Key]
        public int ID { get; set; }
        public int ID_Phuong_An { get; set; }
        public decimal Khoi_Luong_DauRa { get; set; } = 1.0m;
        public decimal Tong_Chi_Phi_1Tan { get; set; }
        public string Json_TPHH_DauRa { get; set; } = string.Empty;
        public string? Json_CoCau_QuangTho { get; set; }
        public DateTimeOffset Ngay_Tinh { get; set; } = DateTimeOffset.Now;

        
    }
}
