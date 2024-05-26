using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.AccountModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Helper.Web.Controllers;

public class AccountController(IRepository<User, Guid> userRepository, ILogger<AccountController> logger)
    : Controller
{
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Login model state is invalid.");
            return View("Login", model);
        }

        var user = (await userRepository.GetAllAsync())
            .FirstOrDefault(u => u.Username == model.Username);

        if (user == null)
        {
            ModelState.AddModelError("Username", "Користувач з таким ім'ям не існує!");
            return View("Login", model);
        }
        else if (!PasswordService.VerifyPassword(model.Password, user.Password!))
        {
            ModelState.AddModelError("Password", "Не вірний пароль!");
            return View("Login", model);
        }

        user.LastLoginDate = DateTime.Now;
        await userRepository.UpdateAsync(user);

        await AuthenticateAsync(user);
        return RedirectToAction("Index", "Home");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Register model state is invalid.");
            return View("Register", model);
        }

        var existingUser = (await userRepository.GetAllAsync())
            .FirstOrDefault(u => u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));

        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Username), "Користувач з таким ім'ям вже існує");
            return View("Register", model);
        }

        var user = new User
        {
            Username = model.Username,
            Password = PasswordService.HashPassword(model.Password),
        };

        await userRepository.CreateAsync(user);
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
        var user = await userRepository.GetByIdAsync(userId);

        if (user == null) throw new Exception($"User with id {userId} not found");

        var model = ProfileViewModel(user);
        return View(model);
    }

    private static ProfileViewModel ProfileViewModel(User user)
    {
        return new ProfileViewModel
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
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfileAsync(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View("Profile", model);

        var user = await userRepository.GetByIdAsync(model.Id);
        if (user == null) throw new Exception($"User with id {model.Id} not found");

        bool isUpdated = false;

        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            var existingUser = (await userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Email!.Equals(model.Email, StringComparison.OrdinalIgnoreCase) && u.Id != model.Id);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Така адреса вже існує");
                return View("Profile", ProfileViewModel(await userRepository.GetByIdAsync(model.Id)));
            }
            else if (model.Email != user.Email)
            {
                user.Email = model.Email;
                TempData["SuccessMessage"] = "Електронна пошта оновлена!";
                isUpdated = true;
            }
        }

        if (model.City != user.City)
        {
            user.City = model.City;
            TempData["SuccessMessage"] = "Місто оновлено!";
            isUpdated = true;
        }

        if (isUpdated)
        {
            await userRepository.UpdateAsync(user);
        }

        return RedirectToAction("Profile");
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePasswordAsync(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Profile", model);
        }

        var user = await userRepository.GetByIdAsync(model.Id);
        if (user == null)
        {
            throw new Exception($"User with id {model.Id} not found");
        }

        if (string.IsNullOrWhiteSpace(model.OldPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
            return RedirectToAction("Profile");

        if (!PasswordService.VerifyPassword(model.OldPassword, user.Password!))
        {
            ModelState.AddModelError(nameof(model.OldPassword), "Старий пароль невірний");
            return View("Profile", ProfileViewModel(await userRepository.GetByIdAsync(model.Id)));
        }

        if (model.NewPassword != model.RepeatPassword)
        {
            ModelState.AddModelError(nameof(model.RepeatPassword), "Паролі не співпадають");
            return View("Profile", ProfileViewModel(await userRepository.GetByIdAsync(model.Id)));
        }

        user.Password = PasswordService.HashPassword(model.NewPassword);
        TempData["SuccessMessage"] = "Пароль оновлено";
        await userRepository.UpdateAsync(user);
        return RedirectToAction("Profile");
    }
}