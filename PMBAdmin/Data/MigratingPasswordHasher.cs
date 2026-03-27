using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace PMBAdmin.Data;

/// <summary>
/// Custom password hasher that transparently migrates legacy PMBUser MD5 password hashes
/// to the ASP.NET Core Identity format on first successful login.
/// </summary>
public class MigratingPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private const string LegacyPrefix = "LEGACY-MD5:";
    private readonly PasswordHasher<ApplicationUser> _identityHasher = new();

    public string HashPassword(ApplicationUser user, string password)
        => _identityHasher.HashPassword(user, password);

    public PasswordVerificationResult VerifyHashedPassword(
        ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (hashedPassword.StartsWith(LegacyPrefix, StringComparison.Ordinal))
        {
            var legacyHash = hashedPassword[LegacyPrefix.Length..];
            if (VerifyMd5(providedPassword, legacyHash))
                return PasswordVerificationResult.SuccessRehashNeeded;

            return PasswordVerificationResult.Failed;
        }

        return _identityHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
    }

    private static bool VerifyMd5(string input, string legacyHash)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        var hex = Convert.ToHexString(hashBytes); // uppercase

        return string.Equals(hex, legacyHash, StringComparison.OrdinalIgnoreCase);
    }
}
