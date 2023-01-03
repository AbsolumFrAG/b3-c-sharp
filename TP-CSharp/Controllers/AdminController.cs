using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TP_CSharp.Models;

namespace TP_CSharp.Controllers
{
    public class AdminController : Controller
    {
        private readonly MainContext _context;

        public AdminController(MainContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Blogs");
        }

        public IActionResult Comments()
        {
            var comments = _context.Comments.Include(c => c.Blog).Include(c => c.User).ToList();

            return View(comments);
        }

        public IActionResult Users()
        {
            var users = _context.Users.ToList();

            return View(users);
        }

        public IActionResult Blogs()
        {
            var blogs = _context.Blogs.Include(b => b.User).ToList();
            return View(blogs);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(Guid? id)
        {
            if (id == null)
                return RedirectToAction("NotFound", "Error");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return RedirectToAction("NotFound", "Error");

            var model = new UpdateUserViewModel
            {
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName,
                ProfileImage = user.ProfileImage
            };

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> EditUser(Guid id, UpdateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return RedirectToAction("NotFound", "Error");

            user.Email = model.Email;
            user.Role = model.Role;
            user.FullName = model.FullName;

            if (Request.Form.Files.Count > 0)
            {
                var image = Request.Form.Files[0];

                if (image.Length > 0)
                {
                    using var stream = new MemoryStream();
                    await image.CopyToAsync(stream);
                    user.ProfileImage = Convert.ToBase64String(stream.ToArray());
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(Guid? id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            if (id == null)
                return RedirectToAction("NotFound", "Error");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return RedirectToAction("NotFound", "Error");

            var relatedBlogs = _context.Blogs.Where(b => b.UserId == user.Id).ToList();

            var relatedComments = _context.Comments.Where(c => c.UserId == user.Id).ToList();

            foreach (var blog in relatedBlogs)
                _context.Blogs.Remove(blog);

            foreach (var comment in relatedComments)
                _context.Comments.Remove(comment);

            if (user.Id == userId)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Index", "Home");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }
    }
}
