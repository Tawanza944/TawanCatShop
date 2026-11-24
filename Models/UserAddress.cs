namespace WebApplication1.Models;

/// <summary>A saved delivery address (table user_addresses). One address per user can be the default.</summary>
public class UserAddress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Label { get; set; } = string.Empty;

    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public SavedAddressRegion? Region { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
