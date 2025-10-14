namespace BE_PHOITRON.Domain.Entities;

public class UserPermissionMap
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PermissionId { get; set; }
    public bool IsGranted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


