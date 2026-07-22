using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Rules.Models;

/// <summary>A code's rules plus the two fields its whole-set invariants are checked against.</summary>
/// <param name="Mode">How the code's symbol resolves; absent on update, where mode is immutable and not re-supplied.</param>
/// <param name="ContentType">The kind of content every rule must carry.</param>
/// <param name="Rules">The rules being validated.</param>
public sealed record CodeRuleSet(ContentMode? Mode, CodeContentType ContentType, IReadOnlyList<CodeRule> Rules);
