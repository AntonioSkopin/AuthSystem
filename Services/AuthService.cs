using AuthSystem.Entities;
using AuthSystem.Helpers;
using AuthSystem.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AuthSystem.Services
{
    public interface IAuthorizationService
    {
        Task<User> Authenticate(AuthenticateModel authenticateModel);
        Task<User> Register(User user, string password);
        Task<bool> ActivateAccount(string pincode);
    }

    public class AuthService : SqlService, IAuthorizationService
    {
        private DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration, DataContext context) : base(configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<User> Authenticate(AuthenticateModel authenticateModel)
        {
            if (string.IsNullOrEmpty(authenticateModel.Username) || string.IsNullOrEmpty(authenticateModel.Password))
                Console.WriteLine("Please enter your username and password!");

            // Query to get the user
            var getUserQuery =
            @"
                SELECT * FROM Users
                WHERE Username = @_username
            ";

            var user = await GetQuery<User>(getUserQuery, new
            {
                _username = authenticateModel.Username
            });

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

            // Query to get the user
            var listOfUsersQuery =
            @"
                SELECT * FROM Users
            ";

            // Execute and store response
            List<User> listUsers = await GetManyQuery<User>(listOfUsersQuery);

            // Validation:
            if (listOfUsersQuery.Contains(user.Username))
                Console.WriteLine("Username is already taken");

            if (listOfUsersQuery.Contains(user.Email))
                Console.WriteLine("Email is already taken");

            // Get the password hash
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            // Generate pin code and send email to registered user
            string pinCode = GeneratePin();
            SendRegisterConfirmationMail(user.Email, pinCode);

            user.Gd = Guid.NewGuid();

            var insertUserQuery =
            @"
                  INSERT INTO Users
                  VALUES(@_gd, @_firstname, @_lastname, @_username, @_email, @_hash, @_salt, @_isActivated, @_pin)
            ";

            // Execute query
            await PostQuery(insertUserQuery, new
            {
                _gd = user.Gd,
                _firstname = user.Firstname,
                _lastname = user.Lastname,
                _username = user.Username,
                _email = user.Email,
                _hash = passwordHash,
                _salt = passwordSalt,
                _isActivated = false,
                _pin = pinCode
            });

            return user;
        }

        public async Task<bool> ActivateAccount(string pincode)
        {
            // Query to search for a user with the pincode
            var getUserQuery =
            @"
                SELECT * FROM Users
                WHERE ActivationPin = @_pin
            ";

            // Get result of query
            var user = await GetQuery<User>(getUserQuery, new
            {
                _pin = pincode
            });

            if (user == null)
            {
                Console.WriteLine("Invalid PIN is entered!");
                return false;
            }

            // Activate the account
            user.IsActivated = true;

            // Set the Activation pin to null because we don't need it anymore
            var updateIsActivatedQuery =
            @"
                UPDATE Users
                SET IsActivated = 1, ActivationPin = null
                WHERE ActivationPin = @_pin
            ";

            await PutQuery(updateIsActivatedQuery, new
            {
                _pin = pincode
            });

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
