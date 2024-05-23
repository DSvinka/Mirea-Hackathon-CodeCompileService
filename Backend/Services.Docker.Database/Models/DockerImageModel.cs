using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Utils.Database;

namespace Services.Docker.Database.Models;

public class DockerImageModel: BaseModel
{
    [MaxLength(32)] public required string DisplayName { get; set; }
    [MaxLength(512)] public required string Description { get; set; }
    
    [MaxLength(128)] public required string DockerImage { get; set; }
    [MaxLength(128)] public required string DockerImageTag { get; set; }
    
    [MaxLength(16)] public required string CodeFileExtension { get; set; }
    [MaxLength(16)] public required string CodeEditorLang { get; set; }
    
    
    [MaxLength(256)] public string? CodeInitCommand { get; set; }
    [MaxLength(256)] public required string CodeStartCommand { get; set; }
    
    public required int MaxMemory { get; set; }
    public required int MaxCpuShares { get; set; } = 256;
    
    /// <summary>
    /// In Megabytes
    /// </summary>
    public required int MaxStorage { get; set; }
    
    public required int MaxCountByUser { get; set; }
    public required int MaxCountByServer { get; set; }

    
    public List<DockerContainerModel>? Containers { get; set; }

    public static void InitModelContext(ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Entity<DockerImageModel>();
        model.ToTable("docker_images");
        model.HasKey(b => b.Id);
        
        model.Property(b => b.Id)
            .IsRequired();
        
        model.Property(b => b.DisplayName)
            .IsRequired();

        model.Property(b => b.Description)
            .IsRequired();
        
        model.Property(b => b.DockerImage)
            .IsRequired();
        
        model.Property(b => b.DockerImageTag)
            .IsRequired();

        
        model.Property(b => b.CodeFileExtension)
            .IsRequired();
        
        model.Property(b => b.CodeEditorLang)
            .IsRequired();
        
        model.Property(b => b.CodeInitCommand)
            .IsRequired(false);
        
        model.Property(b => b.CodeStartCommand)
            .IsRequired();
        
        
        model.Property(b => b.MaxMemory)
            .IsRequired();
        
        model.Property(b => b.MaxCpuShares)
            .IsRequired();
        
        model.Property(b => b.MaxStorage)
            .IsRequired();

        
        model.Property(b => b.MaxCountByUser)
            .IsRequired();
        
        model.Property(b => b.MaxCountByServer)
            .IsRequired();
        
        
        model.Property(b => b.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        model.Property(b => b.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);
    }
}