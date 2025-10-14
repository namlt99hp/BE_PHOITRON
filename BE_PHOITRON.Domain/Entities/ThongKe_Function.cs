using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_PHOITRON.Domain.Entities;

public class ThongKe_Function
{
    public int ID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Ten { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string DonVi { get; set; } = string.Empty;
    public string? HighlightClass { get; set; }
    public bool IsAutoCalculated { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime Ngay_Tao { get; set; } = DateTime.Now;
    public string? Nguoi_Tao { get; set; }
}
    