namespace BE_PHOITRON.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // JWT info from external API
    public string? JwtSubject { get; set; } // sub claim from JWT
    public string? JwtIssuer { get; set; } // iss claim from JWT
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;

    public string? FullName { get; set; }
    public string? Phone { get; set; }

    // Reference by Id only (no FK constraint)
    public int? DepartmentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}


