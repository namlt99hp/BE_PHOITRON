namespace BE_PHOITRON.Domain.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // e.g., QUANG.READ
    public string Name { get; set; } = string.Empty;
    public string? Module { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


