using FluentValidation;
using WSBLearn.Application.Extensions;
using WSBLearn.Application.Requests.Question;

namespace WSBLearn.Application.Validators.Question
{
    public class UpdateQuestionRequestValidator : AbstractValidator<UpdateQuestionRequest>
    {
        public UpdateQuestionRequestValidator()
        {
            RuleFor(r => r.QuestionContent)
                .NotNull()
                .NotEmpty()
                .MaximumLength(1000);

            RuleFor(r => r.ImageUrl)
                .MaximumLength(1000)
                .NotEmpty();

            RuleFor(r => r.A)
                .NotNull()
                .NotEmpty()
                .MaximumLength(1000);

            RuleFor(r => r.B)
                .NotEmpty()
                .MaximumLength(1000);

            RuleFor(r => r.C)
                .MaximumLength(1000)
                .NotNull()
                .Unless(r => r.CorrectAnswer.ToString().ToLower() != "c")
                .WithMessage("Answer 'c' need to be specified when it is set as a CorrectAnswer")
                .NotEmpty();

            RuleFor(r => r.D)
                .MaximumLength(1000)
                .NotNull()
                .Unless(r => r.CorrectAnswer.ToString().ToLower() != "d")
                .WithMessage("Answer 'd' need to be specified when it is set as a CorrectAnswer")
                .NotEmpty();

            RuleFor(r => r.CorrectAnswer)
                .NotNull()
                .NotEmpty()
                .Custom((value, context) =>
                {
                    string[] validCorrectAnswers = { "a", "b", "c", "d" };
                    if (!validCorrectAnswers.Contains(value.ToString().ToLower()))
                    {
                        context.AddFailure("CorrectAnswer", "CorrectAnswer must be either a, b, c or d");
                    }
                });

            RuleFor(r => r.Level)
                .NotNull()
                .NotEmpty()
                .Custom((value, context) =>
                {
                    int[] validLevels = { 1, 2, 3 };
                    if (!validLevels.Contains(value))
                    {
                        context.AddFailure("Level", "Level must be either 1, 2 or 3");
                    }
                });

            RuleFor(r => r.ImageUrl)
                .Custom((value, context) =>
                {
                    var isUrlOrEmpty = value!.UrlOrEmpty();
                    if (!isUrlOrEmpty)
                    {
                        context.AddFailure("IconUrl", "Field is not empty and not a valid fully-qualified http, https or ftp URL");
                    }
                });
        }
    }
}
