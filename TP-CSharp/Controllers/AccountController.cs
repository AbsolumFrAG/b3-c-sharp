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
                    new(ClaimTypes.NameIdentifier, user?.Id.ToString() ?? string.Empty),
                    new(ClaimTypes.Name, user?.FullName ?? string.Empty),
                    new(ClaimTypes.Role, user?.Role),
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

        [Authorize]
        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(User user)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            var userDb = _context.Users.SingleOrDefault(u => u.Id == userId);

            if (userDb == null)
                return RedirectToAction("NotFound", "Error");

            if (userDb.Email != user.Email)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "L'email existe déjà");
                    return View(user);
                }
            }

            if (Request.Form.Files.Count > 0)
            {
                var image = Request.Form.Files[0];

                if (image.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await image.CopyToAsync(stream);
                    userDb.ProfileImage = Convert.ToBase64String(stream.ToArray());
                }
            }

            userDb.FullName = user.FullName;
            userDb.Email = user.Email;

            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "Account");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            var user = _context.Users.SingleOrDefault(u => u.Id == id);

            if (user == null)
                return RedirectToAction("NotFound", "Error");

            var relatedBlogs = _context.Blogs.Where(b => b.UserId == user.Id).ToList();

            var relatedComments = _context.Comments.Where(c => c.UserId == user.Id).ToList();

            foreach (var blog in relatedBlogs)
                _context.Blogs.Remove(blog);

            foreach (var comment in relatedComments)
                _context.Comments.Remove(comment);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
