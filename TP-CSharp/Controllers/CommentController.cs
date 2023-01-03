using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP_CSharp.Models;

namespace TP_CSharp.Controllers
{
    public class CommentController : Controller
    {
        private readonly MainContext _context;

        public CommentController(MainContext context)
        {
            _context = context;
        }

        [HttpGet]
        public PartialViewResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comment comment)
        {
            if (!ModelState.IsValid) return View(comment);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            var user = _context.Users.SingleOrDefault(u => u.Id == userId);

            comment.UserId = user?.Id;

            comment.CreatedAt = DateTime.UtcNow;
            _context.Add(comment);

            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", "Blog", new { id = comment.BlogId });
        }
    }
}
