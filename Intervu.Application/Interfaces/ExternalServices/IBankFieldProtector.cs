namespace Intervu.Application.Interfaces.ExternalServices
{
    public interface IBankFieldProtector
    {
        string Encrypt(string plaintext);

        string Decrypt(string ciphertext);

        string Mask(string plaintext);
    }
}
