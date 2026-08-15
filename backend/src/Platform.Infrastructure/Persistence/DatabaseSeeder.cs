using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence.Context;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Seeds essential reference data (Roles) into the database on application startup.
    /// The development admin is seeded only when explicitly enabled by the host.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        ILogger logger,
        bool seedDevelopmentAdmin = false,
        IPasswordHasher? passwordHasher = null)
    {
        var existingRoles = await db.Roles.ToListAsync();

        var required = new[]
        {
            (Name: "Student", Desc: "Learner enrolled in courses and challenges."),
            (Name: "Teacher", Desc: "Instructor who can create and manage courses."),
            (Name: "Admin",   Desc: "Platform administrator with full access.")
        };

        var added = false;
        foreach (var (name, desc) in required)
        {
            if (!existingRoles.Any(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                db.Roles.Add(new Role { Name = name, Description = desc });
                added = true;
            }
        }

        if (added)
        {
            await db.SaveChangesAsync();
            logger.LogInformation("Seeded missing roles into database.");
        }
        else
        {
            logger.LogInformation("All core roles (Student, Teacher, Admin) already exist.");
        }

        // ── Seed Development Admin Account (Development Environment ONLY) ────────
        if (seedDevelopmentAdmin)
        {
            var adminEmail = "admin@platform.com";
            var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                adminRole = new Role { Name = "Admin", Description = "Platform administrator with full access." };
                db.Roles.Add(adminRole);
                await db.SaveChangesAsync();
            }

            var rawPassword = "AdminPassword123!";
            var passwordHash = passwordHasher != null
                ? passwordHasher.Hash(rawPassword)
                : BCrypt.Net.BCrypt.HashPassword(rawPassword);

            var existingAdmin = await db.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    FullName = "System Admin",
                    Email = adminEmail,
                    PasswordHash = passwordHash,
                    RoleId = adminRole.Id,
                    IsActive = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Users.Add(adminUser);
                await db.SaveChangesAsync();
                logger.LogInformation("Seeded default development admin user ({Email}) with Admin RoleId={RoleId}.", adminEmail, adminRole.Id);
            }
            else if (existingAdmin.RoleId != adminRole.Id)
            {
                existingAdmin.RoleId = adminRole.Id;
                existingAdmin.PasswordHash = passwordHash;
                existingAdmin.IsActive = true;
                existingAdmin.UpdatedAt = DateTime.UtcNow;

                db.Users.Update(existingAdmin);
                await db.SaveChangesAsync();
                logger.LogInformation("Updated existing development user ({Email}) to Admin RoleId={RoleId}.", adminEmail, adminRole.Id);
            }
        }
        else
        {
            logger.LogInformation("Development admin account seeding is disabled.");
        }
    }
}
