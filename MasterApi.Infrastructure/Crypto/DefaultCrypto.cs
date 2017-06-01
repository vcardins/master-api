using System;
using System.Linq;
using MasterApi.Core.Infrastructure.Crypto;

namespace MasterApi.Infrastructure.Crypto
{
    public class DefaultCrypto : ICrypto
    {

        public const char PasswordHashingIterationCountSeparator = '.';
        
        public string Hash(string value)
        {
            return Crypto.Hash(value);
        }
        
        public bool VerifyHash(string value, string hash)
        {
            var hashedValue = Hash(value);
            return SlowEquals(hashedValue, hash);
        }

        public string Hash(string value, string key)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            var alg = new System.Security.Cryptography.HMACSHA512(keyBytes);
            var hash = alg.ComputeHash(valueBytes);

            var result = Crypto.BinaryToHex(hash);
            return result;
        }
        
        public bool VerifyHash(string value, string key, string hash)
        {
            var hashedValue = Hash(value, key);
            return SlowEquals(hashedValue, hash);
        }

        public string GenerateNumericCode(int digits)
        {
            // 18 is good size for a long
            if (digits > 18) digits = 18;
            if (digits <= 0) digits = 6;

            var bytes = Crypto.GenerateSaltInternal(sizeof(long));
            var val = BitConverter.ToInt64(bytes, 0);
            var mod = (int)Math.Pow(10, digits);
            val %= mod;
            val = Math.Abs(val);

            return val.ToString("D" + digits);
        }

        public string GenerateSalt()
        {
            return Crypto.GenerateSalt();
        }

        public string HashPassword(string password, int iterations)
        {
            var count = iterations;
            if (count <= 0)
            {
                count = GetIterationsFromYear(GetCurrentYear());
            }
            var result = Crypto.HashPassword(password, count);
            return EncodeIterations(count) + PasswordHashingIterationCountSeparator + result;
        }

        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (!hashedPassword.Contains(PasswordHashingIterationCountSeparator)) { 
                return Crypto.VerifyHashedPassword(hashedPassword, password);
            }
            var parts = hashedPassword.Split(PasswordHashingIterationCountSeparator);
            if (parts.Length != 2) return false;

            var count = DecodeIterations(parts[0]);
            if (count <= 0) return false;

            hashedPassword = parts[1];

            return Crypto.VerifyHashedPassword(hashedPassword, password, count);
        }

        public bool SlowEquals(string a, string b)
        {
            return SlowEqualsInternal(a, b);
        }

        public string EncodeIterations(int count)
        {
            return count.ToString("X");
        }

        public int DecodeIterations(string prefix)
        {
            int val;
            if (int.TryParse(prefix, System.Globalization.NumberStyles.HexNumber, null, out val))
            {
                return val;
            }
            return -1;
        }

        // from OWASP : https://www.owasp.org/index.php/Password_Storage_Cheat_Sheet
        private const int StartYear = 2000;
        private const int StartCount = 1000;
        public int GetIterationsFromYear(int year)
        {
            if (year <= StartYear) return StartCount;
            var diff = (year - StartYear) / 2;
            var mul = (int)Math.Pow(2, diff);
            var count = StartCount * mul;
            // if we go negative, then we wrapped (expected in year ~2044). 
            // Int32.Max is best we can do at this point
            if (count < 0) count = int.MaxValue;
            return count;
        }
        
        //[MethodImpl(MethodImplOptions.NoOptimization)]
        internal static bool SlowEqualsInternal(string a, string b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var same = true;
            for (var i = 0; i < a.Length; i++)
            {
                same &= a[i] == b[i];
            }
            return same;
        }

        public virtual int GetCurrentYear()
        {
            return DateTime.Now.Year;
        }
    }
}
