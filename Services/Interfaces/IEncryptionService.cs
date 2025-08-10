namespace UserAuthAPI.Services.Interfaces;

public interface IEncryptionService
{
    byte[] Encrypt(byte[] plainBytes);
    byte[] Decrypt(byte[] cipherBytes);
}