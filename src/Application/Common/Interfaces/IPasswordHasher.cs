namespace Application.Common.Interfaces;

/// <summary>
/// Service for hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that a password matches a hash.
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}

