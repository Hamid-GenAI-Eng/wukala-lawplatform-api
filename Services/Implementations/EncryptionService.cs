using System.Security.Cryptography;
using System.Text;
using UserAuthAPI.Services.Interfaces;

namespace UserAuthAPI.Services.Implementations;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyString = configuration["Encryption:Key"];
        if (string.IsNullOrWhiteSpace(keyString))
        {
            throw new InvalidOperationException("Encryption key not configured. Set Encryption:Key in configuration.");
        }

        // Expect base64 or hex; otherwise use UTF8 but enforce 32 bytes
        byte[] keyCandidate;
        if (IsBase64String(keyString))
        {
            keyCandidate = Convert.FromBase64String(keyString);
        }
        else if (IsHexString(keyString))
        {
            keyCandidate = Convert.FromHexString(keyString);
        }
        else
        {
            keyCandidate = Encoding.UTF8.GetBytes(keyString);
        }

        if (keyCandidate.Length < 32)
        {
            // Pad with zeros to 32 bytes
            Array.Resize(ref keyCandidate, 32);
        }
        else if (keyCandidate.Length > 32)
        {
            // Truncate to 32 bytes
            Array.Resize(ref keyCandidate, 32);
        }

        _key = keyCandidate;
    }

    public byte[] Encrypt(byte[] plainBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        // Prepend IV to ciphertext
        ms.Write(aes.IV, 0, aes.IV.Length);
        using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();
        }
        return ms.ToArray();
    }

    public byte[] Decrypt(byte[] cipherBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Extract IV from the beginning
        var iv = new byte[16];
        Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
        var actualCipher = new byte[cipherBytes.Length - iv.Length];
        Array.Copy(cipherBytes, iv.Length, actualCipher, 0, actualCipher.Length);

        using var decryptor = aes.CreateDecryptor(aes.Key, iv);
        using var ms = new MemoryStream();
        using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
        {
            cryptoStream.Write(actualCipher, 0, actualCipher.Length);
            cryptoStream.FlushFinalBlock();
        }
        return ms.ToArray();
    }

    private static bool IsBase64String(string s)
    {
        Span<byte> buffer = new Span<byte>(new byte[s.Length]);
        return Convert.TryFromBase64String(s, buffer, out _);
    }

    private static bool IsHexString(string s)
    {
        if (s.Length % 2 != 0) return false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            bool isHex = (c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F');
            if (!isHex) return false;
        }
        return true;
    }
}