using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.DTOs
{
    public record TPHHCreateDto(
         string Ma_TPHH,
         string? Ten_TPHH,
         string? GhiChu
     );

    public record TPHHUpdateDto(
        string Ma_TPHH,
        string Ten_TPHH,
        string? GhiChu
    );
}
