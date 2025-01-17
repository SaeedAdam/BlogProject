﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogProject.Data;
using BlogProject.Models;

namespace BlogProject.Controllers
{
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region INDEX - COMMENTED OUT
        // GET: Tags
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.Tags.Include(t => t.BlogUser).Include(t => t.Post);
        //    return View(await applicationDbContext.ToListAsync());
        //} 
        #endregion

        #region DETAILS
        //// GET: Tags/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var tag = await _context.Tags
        //        .Include(t => t.BlogUser)
        //        .Include(t => t.Post)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (tag == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(tag);
        //} 
        #endregion

        #region CREATE
        #region GET
        //// GET: Tags/Create
        //public IActionResult Create()
        //{
        //    ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
        //    ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract");
        //    return View();
        //}
        #endregion

        #region POST
        //// POST: Tags/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,PostId,BlogUserId,Text")] Tag tag)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(tag);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", tag.BlogUserId);
        //    ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", tag.PostId);
        //    return View(tag);
        //} 
        #endregion
        #endregion

        #region EDIT
        #region GET
        // GET: Tags/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", tag.BlogUserId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", tag.PostId);
            return View(tag);
        }
        #endregion

        #region POST
        // POST: Tags/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PostId,BlogUserId,Text")] Tag tag)
        {
            if (id != tag.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
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
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", tag.BlogUserId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", tag.PostId);
            return View(tag);
        }
        #endregion
        #endregion

        #region DELETE
        #region GET
        //// GET: Tags/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var tag = await _context.Tags
        //        .Include(t => t.BlogUser)
        //        .Include(t => t.Post)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (tag == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(tag);
        //}
        #endregion

        #region POST
        //// POST: Tags/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var tag = await _context.Tags.FindAsync(id);
        //    _context.Tags.Remove(tag);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //} 
        #endregion
        #endregion

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}
