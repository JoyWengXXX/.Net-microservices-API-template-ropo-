using System.ComponentModel.DataAnnotations;

namespace UserRoleService.Query.Domain.DTOs
{
    public class ReturnRoleInStoreDTO
    {
        public Guid roleId {  get; set; }
        public int roleOrder {  get; set; }
    }
}

