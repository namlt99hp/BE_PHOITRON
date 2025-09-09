using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.ResponsesModels
{
    public record PagedResult<T>(
        int Total,
        int Page,
        int PageSize,
        IReadOnlyList<T> Data
    );
}
