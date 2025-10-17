using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PresentationLayer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public AccountController(IAccountService accountService, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _accountService = accountService;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _accountService.AuthenticateAsync(email, password);

            // if not found in DB, check AdminAccount in configuration
            if (user == null)
            {
                var adminEmail = _config.GetValue<string>("AdminAccount:Email");
                var adminPassword = _config.GetValue<string>("AdminAccount:Password");
                if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword)
                    && string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase)
                    && password == adminPassword)
                {
                    // create a transient admin user object
                    user = new DataAccessLayer.Models.SystemAccount
                    {
                        AccountId = 0,
                        AccountEmail = adminEmail,
                        AccountName = "Administrator",
                        AccountRole = 0
                    };
                }
            }

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            // create claims and sign in with cookie authentication
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.AccountEmail ?? string.Empty),
                new System.Security.Claims.Claim("UserId", user.AccountId.ToString()),
            };
            var role = user.AccountRole switch
            {
                1 => "Staff",
                2 => "Lecturer",
                _ => "Admin"
            };
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));

            var identity = new System.Security.Claims.ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            HttpContext.Session.SetInt32("UserId", user.AccountId);
            HttpContext.Session.SetInt32("Role", user.AccountRole ?? 0);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Index(string? q, int? role)
        {
            var accounts = await _accountService.SearchAsync(q, role);
            return View(accounts);
        }

        // Allow a logged-in Staff (or Admin) to manage their own account
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!short.TryParse(userIdStr, out var userId)) return Unauthorized();
            var acc = await _accountService.GetByIdAsync(userId);
            if (acc == null) return NotFound();
            // For Manage, do not allow role editing
            ViewBag.CanEditRole = false;
            return View(acc);
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPost]
        public async Task<IActionResult> Manage(SystemAccount model)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!short.TryParse(userIdStr, out var userId)) return Unauthorized();
            if (model.AccountId != userId) return Forbid();

            // Lấy entity đang track
            var existing = await _accountService.GetByIdAsync(userId);
            if (existing == null) return NotFound();

            // Kiểm tra email duplicate
            if (await _accountService.EmailExistsAsync(model.AccountEmail ?? string.Empty, model.AccountId))
            {
                TempData["Error"] = "Email đã tồn tại";
                return RedirectToAction(nameof(Manage));
            }

            // Cập nhật từng trường (không đổi role cho self-manage)
            existing.AccountName = model.AccountName;
            existing.AccountEmail = model.AccountEmail;
            // existing.AccountRole giữ nguyên

            await _accountService.UpdateAsync(existing);

            TempData["Success"] = "Cập nhật tài khoản thành công";
            return RedirectToAction(nameof(Manage));
        }


        [Authorize(Roles = "Staff")]
        [HttpGet]
        public async Task<IActionResult> ManageModal()
        {
            // Lấy ID user hiện tại từ Claims
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!short.TryParse(userIdStr, out var userId)) return Unauthorized();

            var account = await _accountService.GetByIdAsync(userId);
            if (account == null) return NotFound();

            // Trả về partial view modal và truyền model
            return PartialView("_Manage", account);
        }

        // Return a partial for admin-only in-place modal editing
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditModal(short id)
        {
            var acc = await _accountService.GetByIdAsync(id);
            if (acc == null) return NotFound();
            return PartialView("_EditModal", acc);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(SystemAccount model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", new { openCreate = true });

            // check duplicate email
            if (await _accountService.EmailExistsAsync(model.AccountEmail ?? string.Empty))
            {
                ModelState.AddModelError("AccountEmail", "Email đã tồn tại");
                return RedirectToAction("Index", new { openCreate = true });
            }

            // ✅ Lấy ID lớn nhất và +1
            var allAccounts = await _accountService.GetAllAsync();
            short nextId = (short)((allAccounts?.OrderByDescending(a => a.AccountId).FirstOrDefault()?.AccountId ?? 0) + 1);
            model.AccountId = nextId;

            await _accountService.AddAsync(model);
            TempData["Success"] = $"Đã tạo tài khoản mới (ID = {nextId})";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(short id)
        {
            var acc = await _accountService.GetByIdAsync(id);
            return acc == null ? NotFound() : RedirectToAction(nameof(Index)); ;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(SystemAccount model)
        {
            if (ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Lấy entity đang track từ DB
                var existing = await _accountService.GetByIdAsync(model.AccountId);
                if (existing == null) return NotFound();

                // Kiểm tra email duplicate
                if (await _accountService.EmailExistsAsync(model.AccountEmail ?? string.Empty, model.AccountId))
                {
                    TempData["Error"] = "Email đã tồn tại";
                    return RedirectToAction(nameof(Index));
                }

                // Cập nhật từng trường (giữ password cũ)
                existing.AccountName = model.AccountName;
                existing.AccountEmail = model.AccountEmail;
                existing.AccountRole = model.AccountRole;

                await _accountService.UpdateAsync(existing);

                TempData["Success"] = "Cập nhật tài khoản thành công";
                return RedirectToAction(nameof(Index));
            }

            
        }



        [Authorize(Roles = "Staff,Admin")]
        [HttpGet]
        public async Task<IActionResult> ChangePassword(short id)
        {
            var acc = await _accountService.GetByIdAsync(id);
            if (acc == null) return NotFound();

            // Staff may only change their own password; Admin can change any
            if (!User.IsInRole("Admin"))
            {
                var userIdStr = User.FindFirst("UserId")?.Value;
                if (!short.TryParse(userIdStr, out var userId) || userId != id)
                    return Forbid();
            }

            return View(acc);
        }

        [Authorize(Roles = "Staff,Admin")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(short id, string currentPassword, string newPassword)
        {
            try
            {
                if (User.IsInRole("Admin"))
                {
                    // Admin can change any user's password without needing current password
                    var acc = await _accountService.GetByIdAsync(id);
                    if (acc == null) return NotFound();
                    acc.AccountPassword = newPassword;
                    await _accountService.UpdateAsync(acc);
                    TempData["Success"] = "Mật khẩu đã được cập nhật bởi Admin";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Staff must provide current password
                    await _accountService.ChangePasswordAsync(id, currentPassword, newPassword);
                    TempData["Success"] = "Đổi mật khẩu thành công";
                    return RedirectToAction("SharedManage", "Account");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return RedirectToAction("SharedManage", "Account");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                await _accountService.DeleteAsync(id);
                TempData["Success"] = "Account deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult SharedManage()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!short.TryParse(userIdStr, out var userId)) return Unauthorized();

            var acc = _accountService.GetByIdAsync(userId).Result;
            if (acc == null) return NotFound();

            return View("~/Views/Shared/Manage.cshtml", acc);
        }


    }
}
