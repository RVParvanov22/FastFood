using FastFoodMVC.Models;
using FastFoodMVC.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using FastFood.Services;

namespace FastFood.Controllers
{

    public class AccountController : Controller
    {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly FileStorageService _fileStorage;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        FileStorageService fileStorage)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _fileStorage = fileStorage;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();
        return View(new ProfileViewModel
        {
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            ProfileImagePath = user.ProfileImagePath
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model, IFormFile? imageFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        if (imageFile is { Length: > 0 })
        {
            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
                ModelState.AddModelError("imageFile", "Позволени са JPG, PNG и WEBP файлове.");
            if (imageFile.Length > 5 * 1024 * 1024)
                ModelState.AddModelError("imageFile", "Максималният размер е 5 MB.");
        }

        if (ModelState.IsValid)
        {
            var oldPath = user.ProfileImagePath;
            string? newPath = null;
            if (imageFile is { Length: > 0 })
            {
                newPath = await _fileStorage.SaveProfileImageAsync(imageFile);
                user.ProfileImagePath = newPath;
            }
            else if (model.RemoveProfileImage)
            {
                user.ProfileImagePath = null;
            }

            user.UserName = model.UserName.Trim();
            user.Email = model.Email.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                if (imageFile is { Length: > 0 } || model.RemoveProfileImage)
                    await _fileStorage.DeleteProfileImageAsync(oldPath);
                TempData["ProfileMessage"] = "Профилът е обновен успешно.";
                return RedirectToAction(nameof(Profile));
            }
            if (newPath is not null) await _fileStorage.DeleteProfileImageAsync(newPath);
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        }

        model.ProfileImagePath = user.ProfileImagePath;
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
        };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Client");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "Invalid email or password.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    }
    
}
