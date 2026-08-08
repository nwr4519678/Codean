using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence.Context;

namespace Platform.Infrastructure.Persistence;

/// <summary>
/// Seeds essential reference data (Roles, Permissions) into the database
/// on application startup if that data doesn't already exist.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
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
    }
}
