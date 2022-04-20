using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using Microsoft.AspNetCore.Identity;

namespace BlogProject.Controllers
{
    public class CommentsController : Controller
    {
        #region PRIVATE VARIABLES
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;
        #endregion

        #region CONSTRUCTOR
        public CommentsController(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        #endregion

        #region INDEX
        #region ORIGINAL INDEX
        // GET: Comments
        public async Task<IActionResult> OriginalIndex()
        {
            var originalComments = await _context.Comments.ToListAsync();
            return View("Index", originalComments);
        }
        #endregion

        #region MODERATED INDEX
        // GET: Comments
        public async Task<IActionResult> ModeratedIndex()
        {
            var moderatedComments = await _context.Comments.Where(c => c.Moderated != null).ToListAsync();
            return View("Index", moderatedComments);
        }
        #endregion

        #region DELETED INDEX
        // GET: Comments
        public async Task<IActionResult> DeletedIndex()
        {
            var applicationDbContext = _context.Comments.Include(c => c.BlogUser).Include(c => c.Moderator).Include(c => c.Post);
            return View(await applicationDbContext.ToListAsync());
        }
        #endregion

        #region SCAFFOLDED ACTION - MOST LIKELY NOT USED
        //////// GET: Comments
        public async Task<IActionResult> Index()
        {
            var allComments = await _context.Comments.ToListAsync();
            return View(allComments);
        }
        #endregion
        #endregion

        #region DETAILS
        //DELETED
        #endregion

        #region CREATE
        #region GET
        // GET: Comments/Create
        //public IActionResult Create()
        //{
        //    ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
        //    ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id");
        //    ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract");
        //    return View();
        //}
        #endregion

        #region POST
        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Body")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.BlogUserId = _userManager.GetUserId(User);
                comment.Created = DateTime.Now;

                _context.Add(comment);
                await _context.SaveChangesAsync();

                comment = await _context.Comments
                    .Include(c => c.Post)
                    .Where(c => c.Id == comment.Id)
                    .FirstOrDefaultAsync();

                if (comment == null)
                {
                    return NotFound();
                }

                return RedirectToAction("Details", "Posts", new { Slug = comment.Post.Slug }, "commentSection");
            }

            return View(comment);
        }
        #endregion
        #endregion

        #region EDIT
        #region GET
        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
            return View(comment);
        }
        #endregion

        #region POST
        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Body")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);
                try
                {

                    newComment.Body = comment.Body;
                    newComment.Updated = DateTime.Now;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
            return View(comment);
        }
        #endregion
        #endregion

        #region DELETE
        #region GET
        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.BlogUser)
                .Include(c => c.Moderator)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }
        #endregion

        #region POST
        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string slug)
        {
            var comment = await _context.Comments.FindAsync(id);
            try
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            
            return RedirectToAction("Details", "Posts", new { slug }, "commentSection");

        }
        #endregion
        #endregion

        #region MODERATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Moderate(int id, [Bind("Id,Body,ModeratedBody,ModerationType")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);

                try
                {
                    newComment.Body = comment.ModeratedBody;
                    newComment.ModerationType = comment.ModerationType;

                    newComment.Moderated = DateTime.Now;
                    newComment.ModeratorId = _userManager.GetUserId(User);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
            }
            return View(comment);
        }
        #endregion

        #region DOES COMMENT EXIST
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
        #endregion
    }
}
