namespace BE_PHOITRON.Application.ResponsesModels
{
    public record PA_LuaChon_CongThucResponse(
        int ID,
        int ID_Phuong_An,
        int ID_Quang_DauRa,
        int ID_Cong_Thuc_Phoi,
        int? Milestone,
        // Navigation properties
        string? Phuong_An_Ten,
        string? Quang_DauRa_Ma,
        string? Quang_DauRa_Ten,
        string? Cong_Thuc_Phoi_Ma,
        string? Cong_Thuc_Phoi_Ten
    );
}
