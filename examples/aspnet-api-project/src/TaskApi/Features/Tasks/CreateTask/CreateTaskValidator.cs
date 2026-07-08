using FluentValidation;

namespace TaskApi.Features.Tasks.CreateTask;

/// <summary>Validates <see cref="CreateTaskRequest"/> before the create handler runs.</summary>
internal sealed class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    /// <summary>Configures the create-task rules.</summary>
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Status!.Value).IsInEnum().When(x => x.Status.HasValue);
    }
}
