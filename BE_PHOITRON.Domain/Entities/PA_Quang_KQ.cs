using System;

namespace BE_PHOITRON.Domain.Entities
{
	public class PA_Quang_KQ
	{
		public int ID { get; set; }
		public int ID_Quang { get; set; }
		public int ID_PhuongAn { get; set; }
		public int LoaiQuang { get; set; } // 2 for Gang, 4 for Xỉ
	}
}


