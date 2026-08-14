using FluentValidation;
using SmartQr.Domain.Codes.Content.Calendar.Models;
using SmartQr.Domain.Codes.Content.Email.Models;
using SmartQr.Domain.Codes.Content.Geo.Models;
using SmartQr.Domain.Codes.Content.MobileApp.Models;
using SmartQr.Domain.Codes.Content.Phone.Models;
using SmartQr.Domain.Codes.Content.Sms.Models;
using SmartQr.Domain.Codes.Content.Text.Models;
using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Content.VCard.Models;
using SmartQr.Domain.Codes.Content.Wifi.Models;
using SmartQr.Domain.Codes.Content;

namespace SmartQr.Application.Codes.Content.Validators;

/// <summary>Validates the content a rule carries, dispatching to the matching per-type validator.</summary>
/// <remarks>Register a new content type here and in its own validator.</remarks>
public sealed class CodeContentValidator : AbstractValidator<CodeContentValueObject>
{
    /// <summary>Builds the per-type content dispatch.</summary>
    public CodeContentValidator()
    {
        RuleFor(content => content).SetInheritanceValidator(v =>
        {
            v.Add(new UrlContentValidator());
            v.Add(new MobileAppLinkContentValidator());
            v.Add(new TextContentValidator());
            v.Add(new EmailContentValidator());
            v.Add(new SmsContentValidator());
            v.Add(new PhoneContentValidator());
            v.Add(new GeoContentValidator());
            v.Add(new WifiContentValidator());
            v.Add(new VCardContentValidator());
            v.Add(new CalendarContentValidator());
        });
    }
}
