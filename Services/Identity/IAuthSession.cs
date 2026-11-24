using WebApplication1.Models;

namespace WebApplication1.Services.Identity;

/// <summary>
/// Writes and clears the signed-in user in the session. Paired with <see cref="ICurrentUserService"/>, which reads it.
/// </summary>
public interface IAuthSession
{
    void SignIn(User user, string role);

    /// <summary>Re-reads name, username and email after the user edits their profile.</summary>
    void RefreshProfile(User user);

    void SetAvatar(string avatar);

    void SetRole(string role);

    void SignOut();
}
