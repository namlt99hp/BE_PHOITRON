using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities;

[Table("PA_ThongKe_Result")]
public class PA_ThongKe_Result
{
    [Key]
    public int ID { get; set; }
    
    [Required]
    public int ID_PhuongAn { get; set; }
    
    [Required]
    public int ID_ThongKe_Function { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal GiaTri { get; set; }
    
    [Required]
    public DateTime Ngay_Tinh { get; set; } = DateTime.Now;
    
    [StringLength(100)]
    public string? Nguoi_Tinh { get; set; }

    // Thứ tự hiển thị
    public int? ThuTu { get; set; }
}
