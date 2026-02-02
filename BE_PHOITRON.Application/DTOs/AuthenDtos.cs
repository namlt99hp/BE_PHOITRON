using System.ComponentModel.DataAnnotations;

namespace BE_PHOITRON.Application.DTOs
{
    public record LoginDto(
        [Required] string username,
        [Required] string password
    );
}
