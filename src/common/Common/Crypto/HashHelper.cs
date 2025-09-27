using System.Security.Cryptography;
using System.Text;

namespace Common.Crypto;

public static class HashHelper
{
    public static string ComputeSha256Hash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            var builder = new StringBuilder();
            foreach (var b in hashBytes)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}