namespace UserAuthAPI.Services.Interfaces;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    bool IsStrongPassword(string password);
    List<string> GetPasswordErrors(string password);
}