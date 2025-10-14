using System;

namespace BE_PHOITRON.Domain.Entities
{
    public class LoCao_ProcessParam
    {
        public int ID { get; set; }
        public string Code { get; set; } = default!;     // e.g. ME_COKE_25_80, ME_COKE_10_25, THAN_PHUN, ME_LIEU, PHU_TAI, TOC_DO_LIEU
        public string Ten { get; set; } = default!;
        public string DonVi { get; set; } = default!;
        public int? ID_Quang_LienKet { get; set; }       // FK -> Quang.ID (nullable)
        public int? Scope { get; set; }                   // 0=LinkedOre, 1=AllNonLoai3, 2=FormulaOnly
        public int ThuTu { get; set; }
        public bool? Da_Xoa { get; set; } = false;
        public DateTime Ngay_Tao { get; set; }
        public string? Nguoi_Tao { get; set; }
        public bool? IsCalculated { get; set; } = false;
        public string? CalcFormula { get; set; }          // Biểu thức/công thức tham chiếu

    }
}


