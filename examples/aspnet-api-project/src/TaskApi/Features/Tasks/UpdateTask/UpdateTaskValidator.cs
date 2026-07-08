using FluentValidation;

namespace TaskApi.Features.Tasks.UpdateTask;

/// <summary>Validates <see cref="UpdateTaskRequest"/> before the update handler runs.</summary>
internal sealed class UpdateTaskValidator : AbstractValidator<UpdateTaskRequest>
{
    /// <summary>Configures the update-task rules.</summary>
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
    }
}
