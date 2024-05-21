using Microsoft.EntityFrameworkCore;
using Services.Docker.Database.Models;

namespace Services.Docker.Database;

public class DockerDbContext: DbContext
{
    public DbSet<DockerContainerModel> DockerContainers { get; set; } = null!;
    public DbSet<DockerImageModel> DockerImages { get; set; } = null!;


    public DockerDbContext(DbContextOptions<DockerDbContext> options) : base(options) {}

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        DockerContainerModel.InitModelContext(modelBuilder);
        DockerImageModel.InitModelContext(modelBuilder);
    }
}