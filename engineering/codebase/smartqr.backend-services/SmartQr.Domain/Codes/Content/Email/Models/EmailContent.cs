namespace SmartQr.Domain.Codes.Content.Email.Models;

/// <summary>Pre-filled email — encodes to a <c>mailto:</c> URI with optional form-encoded subject / body query params.</summary>
public sealed record EmailContent : CodeContent
{
    /// <summary>Recipient address.</summary>
    public required string To { get; init; }

    /// <summary>Optional subject line.</summary>
    public string? Subject { get; init; }

    /// <summary>Optional message body.</summary>
    public string? Body { get; init; }

    /// <inheritdoc />
    public override string Encode()
    {
        var parameters = new List<string>();

        var subject = ContentEncoding.Clean(Subject);
        if (subject.Length > 0)
            parameters.Add($"subject={ContentEncoding.FormUrlEncode(subject)}");

        var body = ContentEncoding.Clean(Body);
        if (body.Length > 0)
            parameters.Add($"body={ContentEncoding.FormUrlEncode(body)}");

        var query = string.Join("&", parameters);
        return $"mailto:{ContentEncoding.Clean(To)}{(query.Length > 0 ? $"?{query}" : string.Empty)}";
    }
}
