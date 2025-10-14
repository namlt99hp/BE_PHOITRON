namespace BE_PHOITRON.Application.ResponsesModels
{
    public record Phuong_An_PhoiResponse(
        int ID,
        string Ten_Phuong_An,
        int ID_Quang_Dich,
        int Phien_Ban,
        byte Trang_Thai,
        DateTimeOffset Ngay_Tinh_Toan,
        byte? Muc_Tieu,
        string? Ghi_Chu,
        DateTimeOffset CreatedAt,
        int? CreatedBy,
        DateTimeOffset? UpdatedAt,
        int? UpdatedBy,
        // Navigation properties
        string? Quang_Dich_Ma,
        string? Quang_Dich_Ten,
        // Calculated properties
        int? So_Luong_Cong_Thuc,
        decimal? Tong_Chi_Phi_1Tan,
        bool? Co_Vong_Lap
    );
}
