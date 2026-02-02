using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Domain.Entities
{
    public class Tbl_PhongBan
{
    [Key]
    public int ID_PhongBan { get; set; }
    public string? TenPhongBan { get; set; }
    public string? TenNgan { get; set; }
    public int? ID_TrangThai { get; set; }
}
}