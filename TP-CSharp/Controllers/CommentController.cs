using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

            comment.CreatedAt = DateTime.Now;
            _context.Add(comment);

            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", "Blog", new { id = comment.BlogId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _context.Comments.SingleOrDefaultAsync(m => m.CommentId == id);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            if (comment != null && !comment.UserId.Equals(userId))
                if (!User.IsInRole("admin"))
                    return RedirectToAction("AccessDenied", "Home");

            var model = new UpdateBlogViewModel
            {
                Content = comment?.Content
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdateCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var comment = await _context.Comments.SingleOrDefaultAsync(m => m.CommentId == id);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            if (comment != null && !comment.UserId.Equals(userId))
            {
                if (!User.IsInRole("admin"))
                    return RedirectToAction("AccessDenied", "Home");
            }

            comment.Content = model.Content;
            await _context.SaveChangesAsync();

            return User.IsInRole("admin") ? RedirectToAction("Comments", "Admin") : RedirectToAction("Detail", "Blog", new { id = comment.BlogId });
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("NotFound", "Error");

            var comment = await _context.Comments
                .SingleOrDefaultAsync(m => m.CommentId == id);

            if (comment == null)
                return RedirectToAction("NotFound", "Error");

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

            if (!comment.UserId.Equals(userId))
                if (!User.IsInRole("admin"))
                    return RedirectToAction("AccessDenied", "Home");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", "Blog", new { id = comment.BlogId });
        }
    }
}
