namespace BE_PHOITRON.Domain.Entities;

public class UserRoleMap
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


