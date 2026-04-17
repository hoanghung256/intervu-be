using System;
using System.Security.Cryptography;
using System.Text;
using Intervu.Application.Interfaces.ExternalServices;
using Intervu.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Intervu.Infrastructure.Security
{
    public sealed class AesGcmBankFieldProtector : IBankFieldProtector
    {
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private readonly byte[] _key;

        public AesGcmBankFieldProtector(IOptions<EncryptionOptions> options)
        {
            var raw = options.Value.BankKey;
            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("Encryption:BankKey is not configured.");

            byte[] key;
            try
            {
                key = Convert.FromBase64String(raw);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Encryption:BankKey must be a Base64 string.", ex);
            }

            if (key.Length != KeySize)
                throw new InvalidOperationException($"Encryption:BankKey must decode to {KeySize} bytes (got {key.Length}).");

            _key = key;
        }

        public string Encrypt(string plaintext)
        {
            if (plaintext is null) throw new ArgumentNullException(nameof(plaintext));
            if (plaintext.Length == 0) return string.Empty;

            var plainBytes = Encoding.UTF8.GetBytes(plaintext);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var cipher = new byte[plainBytes.Length];
            var tag = new byte[TagSize];

            using var aes = new AesGcm(_key, TagSize);
            aes.Encrypt(nonce, plainBytes, cipher, tag);

            var output = new byte[NonceSize + cipher.Length + TagSize];
            Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
            Buffer.BlockCopy(cipher, 0, output, NonceSize, cipher.Length);
            Buffer.BlockCopy(tag, 0, output, NonceSize + cipher.Length, TagSize);

            return Convert.ToBase64String(output);
        }

        public string Decrypt(string ciphertext)
        {
            if (ciphertext is null) throw new ArgumentNullException(nameof(ciphertext));
            if (ciphertext.Length == 0) return string.Empty;

            byte[] blob;
            try
            {
                blob = Convert.FromBase64String(ciphertext);
            }
            catch (FormatException ex)
            {
                throw new CryptographicException("Bank field ciphertext is not valid Base64.", ex);
            }

            if (blob.Length < NonceSize + TagSize)
                throw new CryptographicException("Bank field ciphertext is too short.");

            var nonce = new byte[NonceSize];
            var tag = new byte[TagSize];
            var cipher = new byte[blob.Length - NonceSize - TagSize];
            Buffer.BlockCopy(blob, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(blob, NonceSize, cipher, 0, cipher.Length);
            Buffer.BlockCopy(blob, NonceSize + cipher.Length, tag, 0, TagSize);

            var plain = new byte[cipher.Length];
            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(nonce, cipher, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }

        public string Mask(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext)) return string.Empty;
            return plaintext.Length <= 4 ? "****" : $"****{plaintext[^4..]}";
        }
    }
}
