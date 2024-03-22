﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AuthorizationApp.Server.Models
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<ApplicationUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var x = builder.Entity<ApplicationUser>(config =>
            {
                config.ToTable("AspNetUsers");
            });
            PasswordHasher<ApplicationUser> hasher = new PasswordHasher<ApplicationUser>();
            ApplicationUser admin = new ApplicationUser() { UserName = "admin@mail.ru", Email = "admin@mail.ru", NormalizedUserName = "admin@mail.ru".ToUpper(), NormalizedEmail = "admin@mail.ru".ToUpper() };
            admin.PasswordHash = hasher.HashPassword(admin, "Alex123456!");
            builder.Entity<ApplicationUser>().HasData(admin);
            IdentityRole adminrole = new IdentityRole() { Id = Constants.AdminRole.ToString(), Name = "Admin", NormalizedName = "Admin".ToUpper() };
            builder.Entity<IdentityRole>().HasData(adminrole);
            IdentityUserRole<string> identityUserRole = new IdentityUserRole<string>() { RoleId = adminrole.Id, UserId = admin.Id };
            builder.Entity<IdentityUserRole<string>>().HasNoKey().HasData(identityUserRole);
            base.OnModelCreating(builder);
        }
        
    }
}
