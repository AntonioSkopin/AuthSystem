using AuthSystem.Entities;
using AuthSystem.Models;
using AuthSystem.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthSystem.Controllers
{
    public class AuthController : Controller
    {
        public IAuthorizationService _authService;
        private IMapper _mapper;

        public AuthController(IAuthorizationService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }
        
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public string Authenticate(AuthenticateModel model)
        {
            var user = _authService.Authenticate(model);

            // User = null when invalid input info is provided
            if (user == null)
            {
                return null;
            }
            return user.Username;
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
    }
}