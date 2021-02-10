using AuthSystem.Entities;
using AuthSystem.Helpers;
using AuthSystem.Models;
using AuthSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Controllers
{
    public class AuthController : Controller
    {
        public IAuthorizationService _authService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public AuthController(IAuthorizationService authService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _authService = authService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }
        
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult VerifyAccount()
        {
            return View();
        }

        [HttpPost]
        public async Task<Object> Authenticate(AuthenticateModel model)
        {
            var user = await _authService.Authenticate(model);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Gd.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Return user's Gd and JWT token which is 7 days valid
                return new
                {
                    Username = user.Username,
                    IsActivated = user.IsActivated,
                    Gd = user.Gd,
                    Token = tokenString
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        public async Task<User> RegisterUser(RegisterModel model)
        {
            // Map model to entity
            var user = _mapper.Map<User>(model);

            try
            {
                // Create user
                await _authService.Register(user, model.Password);
                return user;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [HttpPost]
        public async Task<bool> ActivateAccount(string pincode)
        {
            try
            {
                return await _authService.ActivateAccount(pincode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}