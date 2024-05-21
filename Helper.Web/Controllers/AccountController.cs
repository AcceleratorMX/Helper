using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Helper.Web.Controllers;

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
    [ValidateAntiForgeryToken]
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
            return View("Login", model);
        }
        else if (!PasswordService.VerifyPassword(model.Password, user.Password!))
        {
            ModelState.AddModelError(nameof(LoginViewModel.Password), "Некоректний пароль");
            return View("Login", model);
        }
        
        user.LastLoginDate = DateTime.Now;;
        await _userRepository.UpdateAsync(user);

        await AuthenticateAsync(user);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            ViewBag.Error = "Користувач з таким ім'ям вже існує";
            return View("Register", model);
        }

        var user = new User
        {
            Username = model.Username,
            Password = PasswordService.HashPassword(model.Password),
        };

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

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new Exception($"User with id {userId} not found");
        }
        
        var model = ProfileViewModel(user);

        return View(model);
    }

    private static ProfileViewModel ProfileViewModel(User user)
    {
        var model = new ProfileViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            City = user.City,
            RegisterDate = user.RegisterDate,
            LastLoginDate = user.LastLoginDate,
            CreatedJobs = user.CreatedJobs,
            AcceptedJobs = user.AcceptedJobs,
            CompletedJobs = user.CompletedJobs,
            FailedJobs = user.FailedJobs
        };
        return model;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Profile", model);
        }
        
        var user = await _userRepository.GetByIdAsync(model.Id);
        if (user == null)
        {
            throw new Exception($"User with id {model.Id} not found");
        }
    
        if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != user.Email)
        {
            var existingUser = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Така адреса вже існує");
                return View("Profile", ProfileViewModel(await _userRepository.GetByIdAsync(model.Id)));
            }
        }
        
        if(model.Email != user.Email)
        {
            user.Email = model.Email;
            TempData["SuccessMessage"] = "Електронна пошта оновлена";
        }
        
        if (model.City != user.City)
        {
            user.City = model.City;
            TempData["SuccessMessage"] = "Місто оновлено";
        }
        
        await _userRepository.UpdateAsync(user);
        return RedirectToAction("Profile");
    }
}