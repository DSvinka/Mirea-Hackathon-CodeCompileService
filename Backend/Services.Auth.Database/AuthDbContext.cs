using Microsoft.EntityFrameworkCore;
using Services.Auth.Database.Models;

namespace Services.Auth.Database;

public class AuthDbContext: DbContext
{
    public DbSet<UserModel> Users { get; set; } = null!;


    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        UserModel.InitModelContext(modelBuilder);
    }
}