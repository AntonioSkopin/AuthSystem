using AuthSystem.Models;
using AuthSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.Controllers
{
    public class AuthController : Controller
    {
        public IAuthorizationService _authService;

        public AuthController(IAuthorizationService authService)
        {
            _authService = authService;
        }
        
        public IActionResult Index()
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
    }
}