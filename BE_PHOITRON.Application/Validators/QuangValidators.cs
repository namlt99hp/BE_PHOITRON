using BE_PHOITRON.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Application.Validators
{
    public class QuangCreateValidator : AbstractValidator<QuangCreateDto>
    {
        public QuangCreateValidator()
        {
            RuleFor(x => x.MaQuang).NotEmpty().MaximumLength(50);
            RuleFor(x => x.TenQuang).MaximumLength(150);
            RuleFor(x => x.GhiChu).MaximumLength(500);
            RuleFor(x => x.Gia).GreaterThanOrEqualTo(0).When(x => x.Gia.HasValue);
        }
    }

    public class QuangUpdateValidator : AbstractValidator<QuangUpdateDto>
    {
        public QuangUpdateValidator()
        {
            RuleFor(x => x.TenQuang).MaximumLength(150);
            RuleFor(x => x.GhiChu).MaximumLength(500);
            RuleFor(x => x.Gia).GreaterThanOrEqualTo(0).When(x => x.Gia.HasValue);
        }
    }
}
