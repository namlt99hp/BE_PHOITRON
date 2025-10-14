namespace BE_PHOITRON.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., ADMIN
    public string Name { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


