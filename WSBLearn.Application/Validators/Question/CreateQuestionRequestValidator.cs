﻿using FluentValidation;
using WSBLearn.Application.Requests.Question;

namespace WSBLearn.Application.Validators.Question
{
    public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
    {
        public CreateQuestionRequestValidator()
        {
            RuleFor(r => r.QuestionContent)
                .NotNull()
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(r => r.ImageUrl)
                .MaximumLength(400);

            RuleFor(r => r.A)
                .NotNull()
                .NotEmpty()
                .MaximumLength(30);

            RuleFor(r => r.B)
                .NotEmpty()
                .MaximumLength(30);

            RuleFor(r => r.C)
                .MaximumLength(30)
                .NotNull()
                .Unless(r => r.CorrectAnswer.ToString().ToLower() != "c")
                .WithMessage("Answer 'c' need to be specified when it is set as a CorrectAnswer")
                .NotEmpty();

            RuleFor(r => r.D)
                .MaximumLength(30)
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
        }
    }
}