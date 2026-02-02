using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities
{
    public enum LoaiQuang : byte
    {
        Mua = 0,
        Tron = 1,
        Gang = 2,
        Khac = 3,
        Xi = 4
    }

    public class Quang
    {
        [Key]
        public int ID { get; set; }
        public string Ma_Quang { get; set; } = string.Empty;
        public string? Ten_Quang { get; set; }
        public int Loai_Quang { get; set; } // 0=Mua,1=Tron,2=Gang,3=Khac,4=Xi,7=TronTrongPhuongAn
        public bool Dang_Hoat_Dong { get; set; } = true;
        public bool Da_Xoa { get; set; } = false;
        public string? Ghi_Chu { get; set; }
        public DateTimeOffset Ngay_Tao { get; set; } = DateTimeOffset.Now;
        public int? Nguoi_Tao { get; set; }
        public DateTimeOffset? Ngay_Sua { get; set; }
        public int? Nguoi_Sua { get; set; }

        // Link Xỉ (slag) ore to its target Gang ore when applicable
        public int? ID_Quang_Gang { get; set; }
        public bool? Is_Template { get; set; } = false;
    }
}