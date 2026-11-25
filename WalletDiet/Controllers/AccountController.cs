using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using WalletDiet.Data;
using WalletDiet.Models;
using WalletDiet.ViewModels;
using WalletDiet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace WalletDiet.Controllers
{
    public class AccountController : Controller
    {
/*----------------------------------------------- Dependency Injection (DI) -----------------------------------------------------------------------------*/
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _environment;

        public AccountController(AppDbContext context, EmailService emailService, ILogger<AccountController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _environment = environment;
        }

/*----------------------------------------------- Login Method -----------------------------------------------------------------------------*/
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {   
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword (user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Dashboard", "Dashboard");
        }

/*----------------------------------------------- Logout Method --------------------------------------------------------------------------------*/
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

/*----------------------------------------------- Acces Denied Method ---------------------------------------------------------------------------*/
        [HttpGet]
        public IActionResult AccesDenied()
        {
            return View();
        }


/*----------------------------------------------- Register Method --------------------------------------------------------------------------------*/
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
            model.Email = model.Email.Trim();

            bool emailExist = await _context.Users.AnyAsync(e => e.Email == model.Email);
            if (emailExist)
            {
                ModelState.AddModelError("Email", "Email already taken");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();

            var user = new User
            {
                Email = model.Email,
                Username = model.Username,
                Role = "User"
            };

            user.PasswordHash = hasher.HashPassword(user, model.Password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login", "Account");
        }

/*----------------------------------------------- Forgot Password Method --------------------------------------------------------------------------------*/
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailExist = await _context.Users.FirstOrDefaultAsync(e => e.Email == model.Email);

            if (emailExist != null)
            {
                var oldTokens = await _context.PasswordResetTokens
                    .Where(t => t.UserId == emailExist.Id)
                    .ToListAsync();

                _context.PasswordResetTokens.RemoveRange(oldTokens);

                byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
                string token = Convert.ToBase64String(tokenBytes);

                byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
                string tokenHash = Convert.ToBase64String(hashBytes);

                DateTime expiresAt = DateTime.UtcNow.AddMinutes(30);

                var resetToken = new PasswordResetToken
                {
                    UserId = emailExist.Id,
                    TokenHash = tokenHash,
                    ExpiresAt = expiresAt,
                    UsedAt = null
                };

                _context.PasswordResetTokens.Add(resetToken);
                await _context.SaveChangesAsync();

                string resetLink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new
                    {
                        email = emailExist.Email,
                        token = token
                    },
                    Request.Scheme
                )!;

                string emailBody = $@"
                   
                    <h2>Reset Password</h2>
                    <p>We received a request to reset the password for your account.</p>
                    <p>Click the button below to continue:</p>
                    <a href='{System.Net.WebUtility.HtmlEncode(resetLink)}'>
                        Reset Password
                    </a>
                    <p>This link will expire in 30 minutes.</p>
                    <p>If you did not request a password reset, please ignore this email.</p>

                ";

                try
                {
                    await _emailService.SendEmailAsync(
                        emailExist.Email,
                        "Reset Password - Project Penghitaman",
                        emailBody
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send password reset email."
                    );
                }
            }

            model.Message = "Reset code has been sent.";
            return View(model);
        }

/*----------------------------------------------- Reset Password Method --------------------------------------------------------------------------------*/
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Reset link is invalid or has expired.");
            }

            var emailExist = await _context.Users.FirstOrDefaultAsync(e => e.Email == email);

            if (emailExist == null)
            {
                return BadRequest("Reset link is invalid or has expired.");
            }

            byte[] hasBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            string tokenHash = Convert.ToBase64String(hasBytes);

            var activeTokens = await _context.PasswordResetTokens.Where(t =>
                t.UserId == emailExist.Id &&
                t.UsedAt == null &&
                t.ExpiresAt > DateTime.UtcNow
            )
            .ToListAsync();

            PasswordResetToken? resetToken = null;

            foreach (var candidate in activeTokens)
            {
                byte[] storedHasBytes = Convert.FromBase64String(candidate.TokenHash);

                bool tokenMatches = CryptographicOperations.FixedTimeEquals(hasBytes, storedHasBytes);

                if (tokenMatches)
                {
                    resetToken = candidate;
                    break;
                }
            }

            if (resetToken == null)
            {
                return BadRequest("Reset link is invalid or has expired.");
            }

            var model = new ResetPasswordViewModel
            {
                Email = emailExist.Email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return BadRequest("Reset link is invalid or has expired.");
            }

            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(model.Token));
            string tokenHash = Convert.ToBase64String(hashBytes);

            var activeTokens = await _context.PasswordResetTokens.Where(t =>
               t.UserId == user.Id &&
               t.UsedAt == null &&
               t.ExpiresAt > DateTime.UtcNow
           )
           .ToListAsync();

            PasswordResetToken? resetToken = null;

            foreach (var candidate in activeTokens)
            {
                byte[] storedHasBytes = Convert.FromBase64String(candidate.TokenHash);

                bool tokenMatches = CryptographicOperations.FixedTimeEquals(hashBytes, storedHasBytes);

                if (tokenMatches)
                {
                    resetToken = candidate;
                    break;
                }
            }

            if (resetToken == null) 
            {
    
                return BadRequest("Reset link is invalid or has expired.");
            }

            var hasher = new PasswordHasher<User>();

            user.PasswordHash = hasher.HashPassword(
                user,
                model.NewPassword
            );

            _context.PasswordResetTokens.Remove(resetToken);
            await _context.SaveChangesAsync();

            TempData["SuccesMessage"] = "Password has been reset successfully.";

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail(
            [FromServices] EmailService emailService,
            [FromServices] IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
            {
                return NotFound();
            }

            await emailService.SendEmailAsync(
                "zahrulromadhon28@gmail.com",
                "Pengujian Email Service",
                "<h2>Halo!</h2><p>Email berhasil dikirim dari WalletDiet.</p>"
            );

            return Content("Email berhasil dikirim.");
        }

/*----------------------------------------------- Profile Edit Method --------------------------------------------------------------------------------*/
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUsername(ProfileViewModel model)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "Username cannot be empty.");

                model.Email = user.Email;
                model.ProfilePicture = user.ProfilePicture;

                return View("Profile", model);
            }

            user.Username = model.Username.Trim();
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Profile)); 
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmail(ProfileViewModel model)
        {
            var emailIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(emailIdValue, out var emailId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(emailId);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                model.Email = user.Email;
                model.ProfilePicture = user.ProfilePicture;

                ViewBag.IsEditingEmail = true;

                return View("Profile", model);
            }

            var email = await _context.Users.FirstOrDefaultAsync(e => e.Email == model.Email && e.Id != emailId);

            if (email != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email already taken.");

                model.Username = user.Username;
                model.Email = user.Email;
                model.ProfilePicture = user.ProfilePicture;

                ViewBag.IsEditingEmail = true;

                return View("Profile", model);
            }

            user.Email = model.Email!.Trim();
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(ProfileViewModel model)
        {
            var passwordIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(passwordIdValue, out var passwordId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(passwordId);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Current Password cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "New Password cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Confirm cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                model.Email = user.Email;
                model.ProfilePicture = user.ProfilePicture;

                ViewBag.IsEditingPassword = true;

                return View("Profile", model);
            }

            var hasher = new PasswordHasher<User>();

            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword!);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorect.");

                model.Username = user.Username;
                model.Email = user.Email;
                model.ProfilePicture = user.ProfilePicture;

                ViewBag.IsEditingPassword = true;

                return View("Profile", model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "New password and confirmation password do not match.");

                model.Username = user.Username;
                model.Email = user.Email;
                model.ProfilePicture = user.ProfilePicture;

                ViewBag.IsEditingPassword = true;

                return View("Profile", model);
            }

            user.PasswordHash = hasher.HashPassword(
                user,
                model.NewPassword!
            );

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Profile));
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(ProfileViewModel model)
        {
            // Mengambil ID user yang sedang login
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            var file = model.PictureFile;

            // Mengambil ekstensi file
            var extension = file == null
                ? string.Empty
                : Path.GetExtension(file.FileName).ToLowerInvariant();

            // Validasi file
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.PictureFile),
                    "Please select a picture."
                );
            }
            else
            {
                // Maksimal ukuran file 2 MB
                if (file.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(model.PictureFile),
                        "Picture size cannot exceed 2 MB."
                    );
                }

                // Ekstensi file yang diperbolehkan
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(model.PictureFile),
                        "Only JPG, JPEG, PNG, and WEBP files are allowed."
                    );
                }
            }

            // Jika validasi gagal
            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                model.Email = user.Email;
                model.ProfilePicture = user.ProfilePicture;

                return View("Profile", model);
            }

            // Menentukan lokasi penyimpanan gambar
            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "avatars",
                "uploads"
            );

            // Membuat folder jika belum tersedia
            Directory.CreateDirectory(folderPath);

            // Membuat nama file unik
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            // Menyimpan gambar
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Menyimpan nama gambar lama
            var oldPicture = user.ProfilePicture;

            // Memperbarui gambar pada database
            user.ProfilePicture = fileName;

            await _context.SaveChangesAsync();

            // Menghapus gambar lama jika tersedia
            if (!string.IsNullOrEmpty(oldPicture))
            {
                var oldFileName = Path.GetFileName(oldPicture);
                var oldFilePath = Path.Combine(folderPath, oldFileName);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            return RedirectToAction(nameof(Profile));
        }



        [HttpPost]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            // Mengambil ID user yang sedang login
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            // Menyimpan nama gambar lama
            var oldPicture = user.ProfilePicture;

            // Mengembalikan profile picture menjadi default
            user.ProfilePicture = null;

            await _context.SaveChangesAsync();

            // Menghapus gambar lama jika tersedia
            if (!string.IsNullOrEmpty(oldPicture))
            {
                var folderPath = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "avatars",
                    "uploads"
                );

                var oldFileName = Path.GetFileName(oldPicture);
                var oldFilePath = Path.Combine(folderPath, oldFileName);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            return RedirectToAction(nameof(Profile));
        }


        [HttpGet]
        public async Task<IActionResult> GetProfilePicture()
        {
            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";

            // Mengambil ID user yang sedang login
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            // Jika user belum memiliki gambar sendiri
            if (string.IsNullOrEmpty(user.ProfilePicture))
            {
                var defaultPath = Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "avatars",
                    "default-avatar.webp"
                );

                if (!System.IO.File.Exists(defaultPath))
                    return NotFound();

                return PhysicalFile(defaultPath, "image/webp");
            }

            // Lokasi folder gambar upload
            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "avatars",
                "uploads"
            );

            var fileName = Path.GetFileName(user.ProfilePicture);
            var filePath = Path.Combine(folderPath, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            // Menentukan tipe gambar
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType);
        }

    }
}
