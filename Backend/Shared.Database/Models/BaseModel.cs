namespace Shared.Database.Models;

public abstract class BaseModel
{
    public long Id { get; set; }

    public DateTime UpdatedAt;
    public DateTime CreatedAt;
}