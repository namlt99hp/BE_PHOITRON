// using System;
// using System.ComponentModel.DataAnnotations;

// namespace BE_PHOITRON.Domain.Entities
// {
//     /// <summary>
//     /// Bảng danh mục loại quặng (A, B, Fe62...)
//     /// </summary>
//     public class LoaiQuang
//     {
//         [Key]
//         public int ID { get; set; }

//         /// <summary>
//         /// Mã loại quặng (A, B, Fe62...)
//         /// </summary>
//         [Required]
//         [MaxLength(50)]
//         public string MaLoaiQuang { get; set; } = string.Empty;

//         /// <summary>
//         /// Tên loại quặng
//         /// </summary>
//         [Required]
//         [MaxLength(255)]
//         public string TenLoaiQuang { get; set; } = string.Empty;

//         [MaxLength(500)]
//         public string? MoTa { get; set; }

//         public bool IsActive { get; set; } = true;

//         public DateTimeOffset NgayTao { get; set; } = DateTimeOffset.Now;
//         public int? NguoiTao { get; set; }

//         public DateTimeOffset? NgaySua { get; set; }
//         public int? NguoiSua { get; set; }
//     }
// }

