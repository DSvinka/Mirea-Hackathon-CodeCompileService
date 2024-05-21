using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Database.Models;

namespace Services.Auth.Database.Models;

public class UserModel: BaseModel
{
    [MaxLength(32)] public required string FirstName { get; set; }
    [MaxLength(32)] public required string LastName { get; set; }
    
    [MaxLength(32)] public required string Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    
    public bool IsAdministrator { get; set; }
    
    [MaxLength(64)] public string? PasswordHash { get; set; }
    
    [MaxLength(64)] public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpire { get; set; }


    public static void InitModelContext(ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Entity<UserModel>();
        model.ToTable("users");
        model.HasKey(b => b.Id);
        

        model.Property(b => b.Id)
            .IsRequired();
        
        model.Property(b => b.FirstName)
            .IsRequired();
        
        model.Property(b => b.LastName)
            .IsRequired();
        
        
        model.Property(b => b.Email)
            .IsRequired();

        model.Property(b => b.Phone)
            .IsRequired(false)
            .HasDefaultValue(null);
        
        
        model.Property(b => b.IsAdministrator)
            .IsRequired()
            .HasDefaultValue(false);
        
        
        model.Property(b => b.PasswordHash)
            .IsRequired(false)
            .HasDefaultValue(null);
        
        
        model.Property(b => b.RefreshToken)
            .IsRequired(false)
            .HasDefaultValue(null);
        
        model.Property(b => b.RefreshTokenExpire)
            .IsRequired(false)
            .HasDefaultValue(null);
        
        
        model.Property(b => b.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        model.Property(b => b.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);
    }
}