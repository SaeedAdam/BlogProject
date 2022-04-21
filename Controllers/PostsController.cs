using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using BlogProject.Enums;
using X.PagedList;
using BlogProject.ViewModels;

namespace BlogProject.Controllers
{
    public class PostsController : Controller
    {
        #region PRIVATE VARIABLES
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;
        #endregion

        #region CONSTRUCTOR
        public PostsController(ApplicationDbContext context, ISlugService slugService, IImageService imageService, UserManager<BlogUser> userManager, BlogSearchService blogSearchService)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
        }
        #endregion

        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm;

            var pageNumber = page ?? 1;
            var pageSize = 5;

            var posts = _blogSearchService.Search(searchTerm);

            return View(await posts.ToPagedListAsync(pageNumber, pageSize));
        }

        #region INDEX
        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Blog).Include(p => p.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }
        #endregion

        #region BLOG POST INDEX
        public async Task<IActionResult> BlogPostIndex(int? id, int? page)
        {
            if(id == null)
            {
                return NotFound();
            }

            var pageNumber = page ?? 1;
            var pageSize = 5;

            //var posts = _context.Posts.Where(p => p.BlogId == id).ToList();

            var posts = await _context.Posts.Where(p => p.BlogId == id && p.ReadyStatus == ReadyStatus.ProductionReady)
                                      .OrderByDescending(p => p.Created)
                                      .ToPagedListAsync(pageNumber, pageSize);

            Blog currentBlog = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);

            ViewData["HeaderImage"] = _imageService.DecodeImage(currentBlog.ImageData, currentBlog.ContentType);

            return View(posts);
        }
        #endregion

        #region DETAILS
        // GET: Posts/Details/5
        public async Task<IActionResult> Details(string slug)
        {
            ViewData["Title"] = "Post Details Page";

            if (string.IsNullOrEmpty(slug)) return NotFound();

            var post = await _context.Posts
                                     .Include(p => p.BlogUser)
                                     .Include(p => p.Tags)
                                     .Include(p => p.Comments).ThenInclude(c => c.BlogUser)
                                     .Include(p => p.Comments).ThenInclude(c => c.Moderator)
                                     .FirstOrDefaultAsync(m => m.Slug == slug);

            if (post == null) return NotFound();

            var dataVM = new PostDetailViewModel() 
            {
                Post = post,
                Tags = _context.Tags
                               .Select(t => t.Text.ToLower())
                               .Distinct().ToList()
            };

            ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ContentType);
            ViewData["MainText"] = post.Title;
            ViewData["Author"] = "by " + post.BlogUser.FullName;

            return View(dataVM);
        }
        #endregion

        #region CREATE
        #region GET
        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name");
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }
        #endregion

        #region POST
        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,ReadyStatus,Image")] Post post, List<string> tagValues)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.Now;

                string authorId = _userManager.GetUserId(User);
                post.BlogUserId = authorId;

                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ContentType = _imageService.ContentType(post.Image);

                string slug = _slugService.UrlFriendly(post.Title);

                //VARIABLE TO STORE WETHER AN ERROR HAS OCCURED
                bool validationError = false;

                if (string.IsNullOrEmpty(slug))
                {
                    validationError = true; 
                    ModelState.AddModelError("", "The title you provided cannot be used as it resulted in an empty slug.");
                }

                //DETECT INCOMING DUPLICATE SLUGS
                else if (!_slugService.IsUnique(slug))
                {
                    validationError = true;
                    ModelState.AddModelError("Title", "The title you provided cannot be used as it produced a duplicate slug.");
                }

                //else if (slug.Contains("test"))
                //{
                //    validationError = true;

                //    ModelState.AddModelError("", "Are you testing again?");

                //    ModelState.AddModelError("Title", "The title you provided cannot contain the word test");
                //}

                if (validationError)
                {
                    ViewData["TagValues"] = string.Join(",", tagValues);
                    return View(post);
                }

                post.Slug = slug;

                _context.Add(post);
                await _context.SaveChangesAsync();

                foreach (string tag in tagValues)
                {
                    _context.Add(new Tag()
                    {
                        PostId = post.Id,
                        BlogUserId = authorId,
                        Text = tag
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details));
            }

            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);

            return View(post);
        }
        #endregion
        #endregion

        #region EDIT
        #region GET
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Name", post.BlogId);
            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));

            return View(post);
        }
        #endregion

        #region POST
        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BlogId,Title,Abstract,Content,ReadyStatus")] Post post, IFormFile newImage, List<string> tagValues)
        {
            if (id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //ORIGINAL POST
                    Post newPost = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == post.Id);

                    newPost.Updated = DateTime.Now;
                    newPost.Title = post.Title;
                    newPost.Abstract = post.Abstract;
                    newPost.Content = post.Content;
                    newPost.ReadyStatus = post.ReadyStatus;

                    string newSlug = _slugService.UrlFriendly(newPost.Title);

                    if(newSlug != newPost.Slug)
                    {
                        if (_slugService.IsUnique(newSlug))
                        {
                            newPost.Title = post.Title;
                            newPost.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "This title cannot be used as it resulted in a duplicate slug");
                            ViewData["TagValues"] = string.Join(",", tagValues);

                            return View(post);
                        }
                    }

                    if (newImage is not null)
                    {
                        newPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newPost.ContentType = _imageService.ContentType(newImage);
                    }

                    //REMOVE ALL TAGS PREVIOUSLY ASSOCIATED WITH POST
                    _context.Tags.RemoveRange(newPost.Tags);

                    //ADD NEW TAG VALUES
                    foreach (var tagText in tagValues)
                    {
                        _context.Add(new Tag() 
                        {
                            PostId = post.Id,
                            BlogUserId = newPost.BlogUserId,
                            Text = tagText
                        });

                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogId"] = new SelectList(_context.Blogs, "Id", "Description", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        } 
        #endregion
        #endregion

        #region DELETE
        #region GET
        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Blog)
                .Include(p => p.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        #endregion

        #region POST
        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion
        #endregion

        #region DOES POST EXISTS
        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        } 
        #endregion
    }
}
