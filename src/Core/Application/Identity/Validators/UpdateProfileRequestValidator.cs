using DN.WebApi.Application.Common.Validation;
using DN.WebApi.Application.FileStorage;
using DN.WebApi.Shared.DTOs.Identity;
using FluentValidation;

namespace DN.WebApi.Application.Identity.Validators;

public class UpdateProfileRequestValidator : CustomValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(p => p.FirstName).MaximumLength(75).NotEmpty();
        RuleFor(p => p.LastName).MaximumLength(75).NotEmpty();
        RuleFor(p => p.Email).NotEmpty();
        RuleFor(p => p.Image).SetNonNullableValidator(new FileUploadRequestValidator());
    }
}