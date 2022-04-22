using BlogProject.Data;
using BlogProject.Enums;
using BlogProject.Models;
using BlogProject.Services;
using BlogProject.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace BlogProject.Controllers
{
    public class HomeController : Controller
    {
        #region VARIALBES
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IConfiguration _configuration;
        #endregion

        #region CONSTRUCTOR
        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender, ApplicationDbContext context, IImageService imageService, IConfiguration configuration)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _imageService = imageService;
            _configuration = configuration;
        }
        #endregion

        #region INDEX
        public async Task<IActionResult> Index(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 3;

            var blogs = _context.Blogs
                                .Include(b => b.BlogUser)
                                .OrderByDescending(b => b.Created)
                                .ToPagedListAsync(pageNumber, pageSize);

            ViewData["HeaderImage"] = "/img/defaultBlogBackgroundImage.jpg";

            return View(await blogs);

        }
        #endregion

        #region ABOUT
        public IActionResult About()
        {
            return View();
        }
        #endregion

        #region CONTACT
        public IActionResult Contact()
        {
            ViewData["HeaderImage"] = "/img/defaultBlogBackgroundImage.jpg";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMe model)
        {
            model.Message = $"{model.Message}";

            await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);

            return RedirectToAction("Index");

        }
        #endregion

        #region ERROR
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        #endregion
    }
}
