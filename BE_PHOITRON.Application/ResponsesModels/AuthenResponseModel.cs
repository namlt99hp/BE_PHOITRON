namespace BE_PHOITRON.Application.ResponsesModels
{
    public record LoginResponse(
        int ID_TaiKhoan,
        string TenTaiKhoan,
        string HoVaTen,
        string ChuKy,
        string? PhongBan_API,
        string? TenPhongBan,
        string? TenNganPhongBan,
        int? ID_PhongBan,
        int? ID_PhanXuong,
        string? Xuong_API
    );
}
