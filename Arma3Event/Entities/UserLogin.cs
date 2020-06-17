using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Arma3Event.Entities
{
    public class UserLogin
    {
        public int UserLoginID { get; set; }

        [Required]
        public int UserID { get; set; }

        public User User { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        internal bool IsValidPassword(string password)
        {
            return Hash(password, Convert.FromBase64String(PasswordSalt)) == PasswordHash;
        }

        internal void SetPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            PasswordSalt = Convert.ToBase64String(salt);
            PasswordHash = Hash(password, salt);
        }

        private static string Hash(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: password,
                            salt: salt,
                            prf: KeyDerivationPrf.HMACSHA256,
                            iterationCount: 10000,
                            numBytesRequested: 256 / 8));
        }
    }
}