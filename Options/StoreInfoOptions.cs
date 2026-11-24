namespace WebApplication1.Options;

/// <summary>Shop contact details shown in the footer and help page. Bound from appsettings.json → "StoreInfo".</summary>
public sealed class StoreInfoOptions
{
    public const string SectionName = "StoreInfo";

    public string Name { get; set; } = "LoveCat";

    public string Tagline { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string OpeningHours { get; set; } = string.Empty;

    public string Credit { get; set; } = string.Empty;

    public List<SocialAccount> Socials { get; set; } = [];
}

/// <summary>A social account. <see cref="Url"/> is optional: without it the handle is shown as text, not a link.</summary>
public sealed class SocialAccount
{
    public string Platform { get; set; } = string.Empty;

    public string Handle { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string? Url { get; set; }
}
