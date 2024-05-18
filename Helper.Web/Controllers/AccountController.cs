using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Helper.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IRepository<User, Guid> userRepository, ILogger<AccountController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login model state is invalid.");
                return View("Login", model);
            }

            var user = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Некоректні дані для входу");
                _logger.LogWarning($"User with username {model.Username} not found.");
                return View("Login", model);
            }
            else if (!PasswordService.VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError(nameof(LoginViewModel.Password), "Некоректний пароль");
                _logger.LogWarning("Password verification failed.");
                return View("Login", model);
            }

            await AuthenticateAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register model state is invalid.");
                return View("Register", model);
            }

            var existingUser = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Username == model.Username);
            if (existingUser != null)
            {
                _logger.LogWarning($"User with username {model.Username} already exists.");
                ViewBag.Error = "Користувач з таким ім'ям вже існує";
                return View("Register", model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = PasswordService.HashPassword(model.Password),
            };

            _logger.LogInformation($"Creating user with username {model.Username}.");
            await _userRepository.CreateAsync(user);

            await AuthenticateAsync(user);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task AuthenticateAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Username)
            };

            var identity = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
