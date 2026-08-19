using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace Sifp_Vue.Server.Helpers
{
    public interface ITotpService
    {
        /// <summary>Secret baru (160-bit, Base32) untuk satu akun yang baru mengaktifkan MFA.</summary>
        string GenerateSecret();

        /// <summary>URI "otpauth://" standar yang dibaca aplikasi authenticator lewat QR code.</summary>
        string BuildOtpAuthUri(string secretBase32, string accountName, string issuer);

        /// <summary>QR code dari <paramref name="otpAuthUri"/> sebagai data URI PNG, siap dipakai langsung di tag &lt;img&gt;.</summary>
        string GenerateQrCodeDataUri(string otpAuthUri);

        /// <summary>Cocokkan kode 6 digit terhadap waktu saat ini (±<paramref name="window"/> langkah 30 detik untuk toleransi jam).</summary>
        bool ValidateCode(string secretBase32, string code, int window = 1);
    }

    /// <summary>
    /// TOTP (RFC 6238) di atas HOTP (RFC 4226) — HMAC-SHA1, langkah 30 detik, 6 digit.
    /// Ini algoritma standar yang dipakai Google Authenticator/Microsoft Authenticator,
    /// diimplementasikan manual karena .NET tidak punya Base32 bawaan.
    /// </summary>
    public class TotpService : ITotpService
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private const int SecretBytesLength = 20; // 160-bit, ukuran standar untuk secret TOTP
        private const int Digits = 6;
        private const int StepSeconds = 30;

        public string GenerateSecret() => Base32Encode(RandomNumberGenerator.GetBytes(SecretBytesLength));

        public string BuildOtpAuthUri(string secretBase32, string accountName, string issuer)
        {
            var label = Uri.EscapeDataString($"{issuer}:{accountName}");
            var issuerParam = Uri.EscapeDataString(issuer);
            return $"otpauth://totp/{label}?secret={secretBase32}&issuer={issuerParam}&digits={Digits}&period={StepSeconds}&algorithm=SHA1";
        }

        public string GenerateQrCodeDataUri(string otpAuthUri)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(otpAuthUri, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(data);
            return $"data:image/png;base64,{Convert.ToBase64String(png.GetGraphic(10))}";
        }

        public bool ValidateCode(string secretBase32, string code, int window = 1)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != Digits || !code.All(char.IsDigit))
            {
                return false;
            }

            byte[] secretBytes;
            try
            {
                secretBytes = Base32Decode(secretBase32);
            }
            catch
            {
                return false;
            }

            var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;
            var codeBytes = Encoding.ASCII.GetBytes(code);

            for (var i = -window; i <= window; i++)
            {
                var candidate = Encoding.ASCII.GetBytes(ComputeTotp(secretBytes, counter + i).ToString("D6"));
                if (CryptographicOperations.FixedTimeEquals(candidate, codeBytes))
                {
                    return true;
                }
            }

            return false;
        }

        private static int ComputeTotp(byte[] secretBytes, long counter)
        {
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            using var hmac = new HMACSHA1(secretBytes);
            var hash = hmac.ComputeHash(counterBytes);

            var offset = hash[^1] & 0x0F;
            var binaryCode = ((hash[offset] & 0x7F) << 24)
                            | ((hash[offset + 1] & 0xFF) << 16)
                            | ((hash[offset + 2] & 0xFF) << 8)
                            | (hash[offset + 3] & 0xFF);

            return binaryCode % (int)Math.Pow(10, Digits);
        }

        private static string Base32Encode(byte[] data)
        {
            var sb = new StringBuilder((data.Length * 8 + 4) / 5);
            int bitBuffer = 0, bitsInBuffer = 0;

            foreach (var b in data)
            {
                bitBuffer = (bitBuffer << 8) | b;
                bitsInBuffer += 8;
                while (bitsInBuffer >= 5)
                {
                    bitsInBuffer -= 5;
                    sb.Append(Base32Alphabet[(bitBuffer >> bitsInBuffer) & 0x1F]);
                }
            }

            if (bitsInBuffer > 0)
            {
                sb.Append(Base32Alphabet[(bitBuffer << (5 - bitsInBuffer)) & 0x1F]);
            }

            return sb.ToString();
        }

        private static byte[] Base32Decode(string base32)
        {
            base32 = base32.Trim().TrimEnd('=').ToUpperInvariant();
            var bytes = new List<byte>(base32.Length * 5 / 8);
            int bitBuffer = 0, bitsInBuffer = 0;

            foreach (var c in base32)
            {
                var index = Base32Alphabet.IndexOf(c);
                if (index < 0)
                {
                    continue; // spasi/pemisah dari input yang diketik manual pengguna
                }

                bitBuffer = (bitBuffer << 5) | index;
                bitsInBuffer += 5;
                if (bitsInBuffer >= 8)
                {
                    bitsInBuffer -= 8;
                    bytes.Add((byte)((bitBuffer >> bitsInBuffer) & 0xFF));
                }
            }

            return bytes.ToArray();
        }
    }
}
