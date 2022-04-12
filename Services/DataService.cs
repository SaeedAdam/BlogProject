using BlogProject.Data;
using BlogProject.Enums;
using BlogProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogProject.Services
{
    public class DataService
    {
        #region PROPERTIES
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;

        #endregion

        #region CONSTRUCTOR
        public DataService(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<BlogUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        #endregion


        public async Task ManageDataAsync()
        {
            //CREATE DATABASE FROM MIGRATIONS
            await _dbContext.Database.MigrateAsync();
            //SEEDING ROLES INTO SYSTEM
            await SeedRolesAsync();

            //SEED USERS INTO SYSTEM
            await SeedUsersAsync();
        }

        #region SEED ROLES
        private async Task SeedRolesAsync()
        {
            //IF SYSTEM HAS ROLES, DO NOTHING
            if (_dbContext.Roles.Any())
            {
                return;
            }

            //CREATE ROLES
            foreach (var role in Enum.GetNames(typeof(BlogRole)))
            {
                //USER ROLE MANAGER TO CRETAE ROLES
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        #endregion

        #region SEED USERS
        private async Task SeedUsersAsync()
        {
            if (_dbContext.Users.Any())
            {
                return;
            }

            //CREATES A NEW BLOG USER
            BlogUser adminUser = new BlogUser()
            {
                Email = "thomaspereira94@gmail.com",
                UserName = "thomaspereira94@gmail.com",
                FirstName = "Thomas",
                LastName = "Pereira",
                DisplayName = "Peralta",
                PhoneNumber = "(800) 555-1212",
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(adminUser, "Test$123");

            await _userManager.AddToRoleAsync(adminUser, nameof(BlogRole.Administrator));

            BlogUser modUser = new BlogUser()
            {
                Email = "thomaspereira1994@outlook.com",
                UserName = "thomaspereira1994@outlook.com",
                FirstName = "Thomas",
                LastName = "Pereira",
                DisplayName = "Peralta",
                PhoneNumber = "(800) 555-1212",
                EmailConfirmed = true,
            };

            await _userManager.CreateAsync(modUser, "Test$123");
            await _userManager.AddToRoleAsync(modUser, nameof(BlogRole.Moderator));

        } 
        #endregion



    }
}
