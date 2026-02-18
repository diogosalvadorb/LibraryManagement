namespace LibraryManagement.Application.Interfaces
{
    public interface IAuthService
    {
        string GenerateTokenJwt(string email, string role);
        string ComputeSha256Hash(string password);
    }
}
