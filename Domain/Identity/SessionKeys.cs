namespace WebApplication1.Domain.Identity;

/// <summary>
/// Session key names shared by the code that writes the signed-in user and the code that reads it.
/// </summary>
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string UserName = "UserName";
    public const string UserEmail = "UserEmail";
    public const string FullName = "FullName";
    public const string UserRole = "UserRole";
    public const string UserAvatar = "UserAvatar";
}
