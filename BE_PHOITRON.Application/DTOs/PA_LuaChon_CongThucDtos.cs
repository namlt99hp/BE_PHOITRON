using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record PA_LuaChon_CongThucCreateDto(
        [Required] int ID_Phuong_An,
        [Required] int ID_Quang_DauRa,
        [Required] int ID_Cong_Thuc_Phoi,
        int? Milestone = null
    );

    public record PA_LuaChon_CongThucUpdateDto(
        [Required] int ID,
        [Required] int ID_Phuong_An,
        [Required] int ID_Quang_DauRa,
        [Required] int ID_Cong_Thuc_Phoi,
        int? Milestone = null
    );

    public record PA_LuaChon_CongThucUpsertDto(
        int? ID,
        PA_LuaChon_CongThucCreateDto PA_LuaChon_CongThuc
    );
}
