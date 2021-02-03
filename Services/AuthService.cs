using AuthSystem.Entities;
using AuthSystem.Helpers;
using AuthSystem.Models;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AuthSystem.Services
{
    public interface IAuthorizationService
    {
        User Authenticate(AuthenticateModel authenticateModel);
        Task<User> Register(User user, string password);
        Task<bool> ActivateAccount(string pincode);
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

        public async Task<User> Register(User user, string password)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(password))
                Console.WriteLine("Please enter a password.");

            if (_context.Users.Any(usr => usr.Username == user.Username))
                Console.WriteLine("Username is already taken");

            if (_context.Users.Any(usr => usr.Email == user.Email))
                Console.WriteLine("Email is already taken");

            // Get the password hash
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            // Generate pin code and send email to registered user
            string pinCode = GeneratePin();
            SendRegisterConfirmationMail(user.Email, pinCode);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.IsActivated = false;
            user.ActivationPin = pinCode;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> ActivateAccount(string pincode)
        {
            // Searches for a user with the pincode
            var user = _context.Users.Where(usr => usr.ActivationPin == pincode).FirstOrDefault();

            if (user == null)
            {
                Console.WriteLine("Invalid PIN is entered!");
                return false;
            }

            // Activate the account
            user.IsActivated = true;
            // Set the Activation pin to null because we don't need it anymore
            user.ActivationPin = null;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
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

        private static void SendRegisterConfirmationMail(string userMail, string pincode)
        {
            string toPerson = userMail;
            string from = "antoniosko15@gmail.com";

            MailMessage message = new MailMessage(from, toPerson);
            message.Subject = "Please verify your account!";
            message.IsBodyHtml = true;
            message.Body = "<h3>Your unique PIN code to activate your account: " + pincode + ".</h3><p>Activate your account by clicking on this <a href='https://localhost:44306/Auth/VerifyAccount'>link</a>!</p>";

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "antoniosko15@gmail.com",
                Password = "CENSORED"
            };
            try
            {
                client.EnableSsl = true;
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in SendRegisterConfirmationMail: {0}", ex.ToString());
            }
        }

        // Generates a 4 digit PIN code
        private static string GeneratePin()
        {
            Random random = new Random();
            return random.Next(0, 9999).ToString("D4");
        }
    }
}
