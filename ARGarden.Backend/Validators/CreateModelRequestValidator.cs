using System.Net;
using FluentValidation;
using ThreeXyNine.ARGarden.Api.Extensions;
using ThreeXyNine.ARGarden.Api.Models;
using static ThreeXyNine.ARGarden.Api.Constants.ContentTypes;
using static ThreeXyNine.ARGarden.Api.Constants.FileExtensions;

namespace ThreeXyNine.ARGarden.Api.Validators;

public class CreateModelRequestValidator : AbstractValidator<CreateModelRequest>
{
    private static readonly string ValidationErrorCode = $"{HttpStatusCode.UnprocessableEntity}";

    public CreateModelRequestValidator()
    {
        this.RuleFor(request => request.ModelGroup).IsInEnum();
        this.RuleFor(request => request.ModelName).NotEmpty().NotNull().WithErrorCode(ValidationErrorCode);
        this.RuleFor(request => request.ModelImageFile.ContentType).Must(SupportedImageContentTypes.Contains)
            .WithErrorCode(ValidationErrorCode)
            .WithMessage(ImageContentTypeErrorMessage);

        this.RuleFor(request => request.ModelBundleFile.ContentType).Must(SupportedAssetBundleContentTypes.Contains)
            .WithErrorCode(ValidationErrorCode)
            .WithMessage(BundleContentTypeErrorMessage);

        this.RuleFor(request => request.ModelImageFile).Must(ff =>
            {
                using var stream = ff.OpenReadStream();
                var extension = stream.GetExtension();
                return SupportedImageExtensions.Contains(extension);
            })
            .WithErrorCode(ValidationErrorCode)
            .WithMessage(ImageExtensionErrorMessage);

        this.RuleFor(request => request.ModelBundleFile).Must(ff =>
            {
                using var stream = ff.OpenReadStream();
                var extension = stream.GetExtension();
                return extension == Unity3dExtension;
            })
            .WithErrorCode(ValidationErrorCode)
            .WithMessage(BundleExtensionErrorMessage);
    }
}