using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    /// <summary>
    /// Template configuration cho gang đích
    /// Lưu danh sách ProcessParam và ThongKe_Function để clone khi tạo phương án mới
    /// </summary>
    public class Gang_Dich_Template_Config
    {
        [Key]
        public int ID { get; set; }
        
        /// <summary>
        /// ID gang đích (FK -> Quang.ID where Loai_Quang = 2 and ID_Quang_Gang = null)
        /// </summary>
        public int ID_Gang_Dich { get; set; }
        
        /// <summary>
        /// Loại template: 1=ProcessParam, 2=ThongKe
        /// </summary>
        public int Loai_Template { get; set; }
        
        /// <summary>
        /// ID_ProcessParam (nếu Loai_Template=1) hoặc ID_ThongKe_Function (nếu Loai_Template=2)
        /// </summary>
        public int ID_Reference { get; set; }
        
        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        public int ThuTu { get; set; }
        
        public DateTime Ngay_Tao { get; set; } = DateTime.Now;
        public int? Nguoi_Tao { get; set; }
        public bool Da_Xoa { get; set; } = false;
    }
}



