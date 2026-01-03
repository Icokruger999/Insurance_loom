using Microsoft.EntityFrameworkCore;

namespace InsuranceLoom.Api.Data;

public static class MigrationRunner
{
    public static async Task RunMigrationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            // Run Entity Framework migrations
            await context.Database.MigrateAsync();
            
            // Run custom SQL migrations
            await RunCustomMigrationsAsync(context);
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            Console.WriteLine($"Migration error: {ex.Message}");
        }
    }
    
    private static async Task RunCustomMigrationsAsync(ApplicationDbContext context)
    {
        var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Migrations");
        
        if (!Directory.Exists(migrationsPath))
        {
            Console.WriteLine("Migrations directory not found");
            return;
        }
        
        // Get all SQL migration files in order
        var migrationFiles = Directory.GetFiles(migrationsPath, "*.sql")
            .OrderBy(f => f)
            .ToList();
        
        foreach (var migrationFile in migrationFiles)
        {
            try
            {
                var sql = await File.ReadAllTextAsync(migrationFile);
                if (!string.IsNullOrWhiteSpace(sql))
                {
                    await context.Database.ExecuteSqlRawAsync(sql);
                    Console.WriteLine($"Executed migration: {Path.GetFileName(migrationFile)}");
                }
            }
            catch (Exception ex)
            {
                // Continue with other migrations even if one fails
                Console.WriteLine($"Error executing migration {Path.GetFileName(migrationFile)}: {ex.Message}");
            }
        }
    }
}

