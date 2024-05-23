namespace Shared.Utils.Database;

public abstract class BaseModel
{
    public long Id { get; set; }

    public DateTime UpdatedAt;
    public DateTime CreatedAt;
}