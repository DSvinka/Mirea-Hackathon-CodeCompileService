using Microsoft.EntityFrameworkCore;

namespace Shared.Database.Models;

public class UserModel: BaseModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public string Email { get; set; }
    public string? Phone { get; set; }
    
    public string? PasswordHash { get; set; }


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
        
        
        model.Property(b => b.PasswordHash)
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