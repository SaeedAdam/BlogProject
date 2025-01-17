﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;
using BlogProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace BlogProject.Controllers
{
    public class BlogsController : Controller
    {
        #region VARIABLES
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IConfiguration _configuration;
        #endregion

        #region CONSTRUCTOR
        public BlogsController(ApplicationDbContext context, IImageService imageService, UserManager<BlogUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _imageService = imageService;
            _userManager = userManager;
            _configuration = configuration;
        }
        #endregion

        #region INDEX
        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Blogs.Include(b => b.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }
        #endregion

        #region DETAILS
        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        #endregion

        #region CREATE
        #region GET
        // GET: Blogs/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }
        #endregion

        #region POST
        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Name,Description,Image")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.Created = DateTime.Now;
                blog.BlogUserId = _userManager.GetUserId(User);

                blog.ImageData = (await _imageService.EncodeImageAsync(blog.Image)?? 
                                  await _imageService.EncondeImageAsync(_configuration["DefaultBlogImage"]));

                blog.ContentType = blog.Image is null ? Path.GetExtension(_configuration["DefaultBlogImage"]) :
                                                            _imageService.ContentType(blog.Image);

                _context.Add(blog);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }
        #endregion
        #endregion

        #region EDIT
        #region GET
        // GET: Blogs/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            return View(blog);
        }
        #endregion

        #region POST    
        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Blog blog, IFormFile newImage)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Blog newBlog = await _context.Blogs.FindAsync(blog.Id);

                    newBlog.Updated = DateTime.Now;

                    if (newBlog.Name != blog.Name)
                    {
                        newBlog.Name = blog.Name;
                    }

                    if(newBlog.Description != blog.Description)
                    {
                        newBlog.Description = blog.Description;
                    }

                    if(newImage is not null)
                    {
                        newBlog.ImageData = await _imageService.EncodeImageAsync(newImage);
                    }



                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
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
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }
        #endregion
        #endregion

        #region DELETE
        #region GET
        // GET: Blogs/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        #endregion

        #region POST
        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion
        #endregion

        #region DOES BLOG EXIST
        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
        #endregion
    }
}
