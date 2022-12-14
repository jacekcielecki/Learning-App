using FluentValidation;
using WSBLearn.Application.Requests.User;

namespace WSBLearn.Application.Validators.User
{
    public class UpdateUserPasswordRequestValidator : AbstractValidator<UpdateUserPasswordRequest>
    {
        public UpdateUserPasswordRequestValidator()
        {
            RuleFor(r => r.OldPassword)
                .NotNull()
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(40);

            RuleFor(r => r.NewPassword)
                .NotNull()
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(40);

            RuleFor(r => r.ConfirmNewPassword)
                .Equal(r => r.NewPassword);
        }
    }
}
