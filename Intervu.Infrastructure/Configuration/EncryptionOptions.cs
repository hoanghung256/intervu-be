namespace Intervu.Infrastructure.Configuration
{
    // Rotation: to rotate BankKey, generate a new 32-byte key, re-encrypt all
    // stored bank-account ciphertexts with the new key in a one-shot job, then
    // swap the env var. AES-GCM does not self-identify keys, so old ciphertexts
    // become unreadable the moment the key changes.
    public class EncryptionOptions
    {
        public const string SectionName = "Encryption";

        public string BankKey { get; set; } = string.Empty;
    }
}
