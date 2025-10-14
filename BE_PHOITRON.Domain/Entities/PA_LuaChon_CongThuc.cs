using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class PA_LuaChon_CongThuc
    {
        [Key]
        public int ID { get; set; }
        public int ID_Phuong_An { get; set; }
        public int ID_Quang_DauRa { get; set; }
        public int ID_Cong_Thuc_Phoi { get; set; }
        public int? Milestone { get; set; }
        public int? ThuTuPhoi { get; set; } // Thứ tự phối trong plan (1, 2, 3, ...)
        public bool Da_Xoa { get; set; } = false;

        // No navigation properties. Relations are handled via ID fields only.
    }
}
