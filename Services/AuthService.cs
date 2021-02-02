using AuthSystem.Entities;
using AuthSystem.Helpers;
using AuthSystem.Models;
using System;
using System.Linq;

namespace AuthSystem.Services
{
    public interface IAuthorizationService
    {
        User Authenticate(AuthenticateModel authenticateModel);
    }

    public class AuthService : IAuthorizationService
    {
        private DataContext _context;

        public AuthService(DataContext context)
        {
            _context = context;
        }

        public User Authenticate(AuthenticateModel authenticateModel)
        {
            if (string.IsNullOrEmpty(authenticateModel.Username) || string.IsNullOrEmpty(authenticateModel.Password))
                Console.WriteLine("Please enter your username and password!");

            var user = _context.Users.SingleOrDefault(usr => usr.Username == authenticateModel.Username);

            // Check if username exists
            if (user == null)
                return null;

            // Check if password is correct
            if (!VerifyPasswordHash(authenticateModel.Password, user.PasswordHash, user.PasswordSalt))
                return null;

            // Authentication successful
            return user;
        }

        // Private helper methods
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // Validation
            if (password == null)
                throw new ArgumentNullException("password");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // Validation:
            if (password == null)
                throw new ArgumentNullException("password");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            if (storedHash.Length != 64)
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");

            if (storedSalt.Length != 128)
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}
