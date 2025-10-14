using System;

namespace BE_PHOITRON.Domain.Entities
{
    public class PA_ProcessParamValue
    {
        public int ID { get; set; }
        public int ID_Phuong_An { get; set; }
        public int ID_ProcessParam { get; set; }
        public decimal? GiaTri { get; set; }
        public int ThuTuParam { get; set; }  // Thứ tự hiển thị của parameter trong plan
        public DateTime Ngay_Tao { get; set; }
        public string? Nguoi_Tao { get; set; }
    }
}


