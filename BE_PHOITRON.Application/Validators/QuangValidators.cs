/*
using BE_PHOITRON.Application.DTOs;
using FluentValidation;

namespace BE_PHOITRON.Application.Validators
{
    public class QuangCreateDtoValidator : AbstractValidator<QuangCreateDto>
    {
        public QuangCreateDtoValidator()
        {
            RuleFor(x => x.Ma_Quang)
                .NotEmpty().WithMessage("Mã quặng không được để trống")
                .MaximumLength(50).WithMessage("Mã quặng không được vượt quá 50 ký tự");

            RuleFor(x => x.Ten_Quang)
                .MaximumLength(200).WithMessage("Tên quặng không được vượt quá 200 ký tự");

            RuleFor(x => x.Loai_Quang)
                .InclusiveBetween(0, 2).WithMessage("Loại quặng phải từ 0 đến 2");

            RuleFor(x => x.Ghi_Chu)
                .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự");
        }
    }

    public class QuangUpdateDtoValidator : AbstractValidator<QuangUpdateDto>
    {
        public QuangUpdateDtoValidator()
        {
            RuleFor(x => x.ID)
                .GreaterThan(0).WithMessage("ID phải lớn hơn 0");

            RuleFor(x => x.Ma_Quang)
                .NotEmpty().WithMessage("Mã quặng không được để trống")
                .MaximumLength(50).WithMessage("Mã quặng không được vượt quá 50 ký tự");

            RuleFor(x => x.Ten_Quang)
                .MaximumLength(200).WithMessage("Tên quặng không được vượt quá 200 ký tự");

            RuleFor(x => x.Loai_Quang)
                .InclusiveBetween(0, 2).WithMessage("Loại quặng phải từ 0 đến 2");

            RuleFor(x => x.Ghi_Chu)
                .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự");
        }
    }
}
*/