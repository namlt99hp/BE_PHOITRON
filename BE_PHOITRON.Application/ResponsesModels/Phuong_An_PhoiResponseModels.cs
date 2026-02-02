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
public record ThieuKetOreComponentDto(int OreId, string MaQuang, string TenQuang, decimal TiLePhanTram);
        public record ThieuKetSectionDto(
            List<ThieuKetOreComponentDto> Components,
            decimal? TK_TIEU_HAO_QTK,
            decimal? TK_SIO2_QTK,
            decimal? TK_TFE,
            decimal? TK_R2,
            decimal? TK_PHAM_VI_VAO_LO,
            decimal? TK_COST
        );
    public record PlanThieuKetSectionDto(
            int PlanId,
            string Ten_Phuong_An,
            DateTimeOffset? Ngay_Tinh_Toan,
            List<ThieuKetOreComponentDto> Components,
            decimal? TK_TIEU_HAO_QTK,
            decimal? TK_SIO2_QTK,
            decimal? TK_TFE,
            decimal? TK_R2,
            decimal? TK_PHAM_VI_VAO_LO,
            decimal? TK_COST
        );

    // Lò Cao DTOs
    public record LoCaoOreComponentDto(int OreId, string MaQuang, string TenQuang, decimal TiLePhanTram, int? LoaiQuang);
    
    public record LoCaoSectionDto(
        List<LoCaoOreComponentDto> Components,
        decimal? LC_SAN_LUONG_GANG,
        decimal? LC_TIEU_HAO_QUANG,
        decimal? LC_COKE_25_80,
        decimal? LC_COKE_10_25,
        decimal? LC_THAN_PHUN,
        decimal? LC_TONG_NHIEU_LIEU,
        decimal? LC_XUAT_LUONG_XI,
        decimal? LC_R2,
        decimal? LC_TONG_KLK_VAO_LO,
        decimal? LC_TONG_ZN_VAO_LO,
        decimal? LC_PHAM_VI_VAO_LO,
        decimal? LC_TI_TRONG_GANG,
        decimal? LC_MN_TRONG_GANG
    );
    
    public record PlanLoCaoSectionDto(
        int PlanId,
        string Ten_Phuong_An,
        DateTimeOffset? Ngay_Tinh_Toan,
        List<LoCaoOreComponentDto> Components,
        decimal? LC_SAN_LUONG_GANG,
        decimal? LC_TIEU_HAO_QUANG,
        decimal? LC_COKE_25_80,
        decimal? LC_COKE_10_25,
        decimal? LC_THAN_PHUN,
        decimal? LC_TONG_NHIEU_LIEU,
        decimal? LC_XUAT_LUONG_XI,
        decimal? LC_R2,
        decimal? LC_TONG_KLK_VAO_LO,
        decimal? LC_TONG_ZN_VAO_LO,
        decimal? LC_PHAM_VI_VAO_LO,
        decimal? LC_TI_TRONG_GANG,
        decimal? LC_MN_TRONG_GANG
    );

    // Bảng chi phí LoCao DTO - đơn giản cho render
    public record BangChiPhiLoCaoDto(
        string TenQuang,
        decimal? Tieuhao,
        string LineType
    );

    // Combined DTO for both sections
    public record PlanSectionDto(
        int PlanId,
        string Ten_Phuong_An,
        DateTimeOffset? Ngay_Tinh_Toan,
        ThieuKetSectionDto? ThieuKet,
        LoCaoSectionDto? LoCao,
        List<BangChiPhiLoCaoDto>? BangChiPhiLoCao
    );
}
