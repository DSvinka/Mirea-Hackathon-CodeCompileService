using Microsoft.EntityFrameworkCore;

namespace Shared.Database.Models;

public class SettingsDockerModel: BaseModel
{
    public string DockerUri { get; set; }
    
    public bool DockerAuthEnable { get; set; }
    public string? DockerAuthUsername { get; set; }
    public string? DockerAuthPassword { get; set; }
    
    public int MaxContainersByUser { get; set; }
    
    
    public static void InitModelContext(ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Entity<SettingsDockerModel>();
        model.ToTable("settings_docker");
        model.HasKey(b => b.Id);
        

        model.Property(b => b.Id)
            .IsRequired();
        
        model.Property(b => b.DockerUri)
            .IsRequired();
        
        
        model.Property(b => b.DockerAuthEnable)
            .IsRequired();
        
        model.Property(b => b.DockerAuthUsername)
            .IsRequired(false)
            .HasDefaultValue(null);

        model.Property(b => b.DockerAuthPassword)
            .IsRequired(false)
            .HasDefaultValue(null);
        
        
        model.Property(b => b.MaxContainersByUser)
            .IsRequired();
        
        
        model.Property(b => b.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        model.Property(b => b.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);
    }
}