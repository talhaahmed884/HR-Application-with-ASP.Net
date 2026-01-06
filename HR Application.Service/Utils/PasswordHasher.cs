using System.Security.Cryptography;
using System.Text;

namespace HR_Application.Service.Utils;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
        }

        // Convert the password string to bytes
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        // Create SHA256 hash
        using (var sha256 = SHA256.Create())
        {
            // Compute hash from password bytes
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            // Convert byte array to hexadecimal string
            StringBuilder hashString = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashString.Append(b.ToString("X2")); // X2 formats as uppercase hexadecimal
            }

            return hashString.ToString();
        }
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        // Hash the provided password
        string passwordHash = HashPassword(password);

        // Compare the hashes (case-insensitive comparison)
        return string.Equals(passwordHash, hashedPassword, StringComparison.OrdinalIgnoreCase);
    }

    public static string GenerateSalt(int length = 16)
    {
        byte[] saltBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPasswordWithSalt(string password, string salt)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password), "Password cannot be null or empty");
        }

        if (string.IsNullOrWhiteSpace(salt))
        {
            throw new ArgumentNullException(nameof(salt), "Salt cannot be null or empty");
        }

        // Combine password and salt before hashing
        string saltedPassword = password + salt;
        return HashPassword(saltedPassword);
    }
}
