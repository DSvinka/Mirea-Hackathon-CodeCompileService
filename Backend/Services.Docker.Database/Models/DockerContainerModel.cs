using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Services.Docker.Shared.Enums;
using Shared.Utils.Database;

namespace Services.Docker.Database.Models;

public class DockerContainerModel: BaseModel
{
    public required long UserId { get; set; }
    public required long ImageId { get; set; }
    
    [MaxLength(128)] public required string ContainerId { get; set; }
    
    public required EDockerStatus Status { get; set; }
    [MaxLength(2048)] public string? Logs { get; set; }
    
    
    [MaxLength(32)] public string ProgramCodeFolder { get; set; }
    [MaxLength(10240)] public string ProgramCode { get; set; }
    
    public int UsageMemory { get; set; }
    public int UsageCpu { get; set; }
    public int UsageStorage { get; set; }


    public DockerImageModel? DockerImage;
    
    
    public static void InitModelContext(ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Entity<DockerContainerModel>();
        model.ToTable("docker_containers");
        model.HasKey(b => b.Id);
        model.HasIndex(b => b.ContainerId).IsUnique();
        
        
        model.Property(b => b.Id)
            .IsRequired();
        
        model.Property(b => b.UserId)
            .IsRequired();
        
        model.Property(b => b.ImageId)
            .IsRequired();
        
        model.HasOne(e => e.DockerImage)
            .WithMany(e => e.Containers)
            .HasForeignKey(e => e.ImageId);
        
        
        model.Property(b => b.ContainerId)
            .IsRequired();
        

        model.Property(b => b.Status)
            .HasConversion<int>()
            .IsRequired();
        
        model.Property(b => b.Logs)
            .IsRequired(false)
            .HasDefaultValue("");


        model.Property(b => b.ProgramCodeFolder)
            .IsRequired();
        model.Property(b => b.ProgramCode)
            .IsRequired();
        
        
        model.Property(b => b.UsageMemory)
            .IsRequired();
        
        model.Property(b => b.UsageCpu)
            .IsRequired();
        
        model.Property(b => b.UsageStorage)
            .IsRequired();
        
        
        model.Property(b => b.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        model.Property(b => b.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);
    }
}