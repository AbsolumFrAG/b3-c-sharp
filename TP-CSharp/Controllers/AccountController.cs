using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt;
using System.Security.Claims;
using TP_CSharp.Models;

namespace TP_CSharp.Controllers
{
    public class AccountController : Controller
    {
        private readonly MainContext _context;
        private readonly IConfiguration _configuration;

        public AccountController(MainContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginUser)
        {
            if (ModelState.IsValid)
            {
                var md5Salt = _configuration.GetValue<string>("AppSettings:MD5SaltStr");
                var saltedPassword = loginUser.Password + md5Salt;
                var hashedPassword = EncryptProvider.Md5(saltedPassword);

                var user = _context.Users.SingleOrDefault(u =>
                    string.Equals(u.Email, loginUser.Email, StringComparison.CurrentCultureIgnoreCase) && u.Password == hashedPassword);

                if (user is { Locked: true })
                {
                    ModelState.AddModelError("", "Votre compte est verrouillé");
                    return View(loginUser);
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name, user.FullName ?? string.Empty),
                    new(ClaimTypes.Role, user.Role),
                    new("Email", user.Email)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return RedirectToAction("Index", "Blog");
            }

            ModelState.AddModelError("", "L'email ou le mot de passe est incorrect");
            return View(loginUser);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerUser)
        {
            if (!ModelState.IsValid) return View(registerUser);
            if (_context.Users.Any(u => string.Equals(u.Email, registerUser.Email, StringComparison.CurrentCultureIgnoreCase)))
            {
                ModelState.AddModelError("Email", "L'email existe déjà");
                return View(registerUser);
            }

            var md5Salt = _configuration.GetValue<string>("AppSettings:MD5SaltStr");
            var saltedPassword = registerUser.Password + md5Salt;
            var hashedPassword = EncryptProvider.Md5(saltedPassword);

            var user = new User
            {
                Email = registerUser.Email,
                Password = hashedPassword
            };

            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public IActionResult Profile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            return View(user);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
