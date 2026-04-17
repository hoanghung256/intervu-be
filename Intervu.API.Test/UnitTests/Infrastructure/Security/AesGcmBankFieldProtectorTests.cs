using System;
using System.Security.Cryptography;
using Intervu.Infrastructure.Configuration;
using Intervu.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Intervu.API.Test.UnitTests.Infrastructure.Security
{
    public class AesGcmBankFieldProtectorTests
    {
        private static AesGcmBankFieldProtector CreateProtector()
        {
            var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var options = Options.Create(new EncryptionOptions { BankKey = key });
            return new AesGcmBankFieldProtector(options);
        }

        [Fact]
        public void Encrypt_Decrypt_RoundTrip_ReturnsOriginal()
        {
            var protector = CreateProtector();
            const string plain = "1234567890";

            var cipher = protector.Encrypt(plain);
            var result = protector.Decrypt(cipher);

            Assert.Equal(plain, result);
        }

        [Fact]
        public void Encrypt_SamePlaintext_ProducesDifferentCiphertexts()
        {
            var protector = CreateProtector();
            const string plain = "1234567890";

            var a = protector.Encrypt(plain);
            var b = protector.Encrypt(plain);

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Decrypt_TamperedCiphertext_Throws()
        {
            var protector = CreateProtector();
            var cipher = protector.Encrypt("1234567890");

            var bytes = Convert.FromBase64String(cipher);
            bytes[^1] ^= 0x01;
            var tampered = Convert.ToBase64String(bytes);

            Assert.Throws<AuthenticationTagMismatchException>(() => protector.Decrypt(tampered));
        }

        [Fact]
        public void Decrypt_InvalidBase64_Throws()
        {
            var protector = CreateProtector();
            Assert.Throws<CryptographicException>(() => protector.Decrypt("not-base64!!!"));
        }

        [Fact]
        public void Encrypt_Empty_ReturnsEmpty()
        {
            var protector = CreateProtector();
            Assert.Equal(string.Empty, protector.Encrypt(string.Empty));
            Assert.Equal(string.Empty, protector.Decrypt(string.Empty));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("12", "****")]
        [InlineData("1234", "****")]
        [InlineData("12345", "****2345")]
        [InlineData("1234567890", "****7890")]
        public void Mask_ReturnsExpectedShape(string input, string expected)
        {
            var protector = CreateProtector();
            Assert.Equal(expected, protector.Mask(input));
        }

        [Fact]
        public void Constructor_MissingKey_Throws()
        {
            var options = Options.Create(new EncryptionOptions { BankKey = string.Empty });
            Assert.Throws<InvalidOperationException>(() => new AesGcmBankFieldProtector(options));
        }

        [Fact]
        public void Constructor_WrongKeyLength_Throws()
        {
            var shortKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var options = Options.Create(new EncryptionOptions { BankKey = shortKey });
            Assert.Throws<InvalidOperationException>(() => new AesGcmBankFieldProtector(options));
        }

        [Fact]
        public void Constructor_NonBase64Key_Throws()
        {
            var options = Options.Create(new EncryptionOptions { BankKey = "not base 64!!!" });
            Assert.Throws<InvalidOperationException>(() => new AesGcmBankFieldProtector(options));
        }
    }
}
