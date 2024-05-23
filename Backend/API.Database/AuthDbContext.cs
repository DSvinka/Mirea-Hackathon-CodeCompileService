using API.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Database;

public class AuthDbContext: DbContext
{
    public DbSet<UserModel> Users { get; set; } = null!;


    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        UserModel.InitModelContext(modelBuilder);
    }
}