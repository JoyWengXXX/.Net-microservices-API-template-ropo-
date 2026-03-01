using System.ComponentModel.DataAnnotations;

namespace SignUpService.Query.Domain.DTOs
{
    public class QueryAllUserDTO
    {

        [Required]
        public int pageIndex { get; set; }
        public int pageSize { get; set; } = 50;
    }
}

