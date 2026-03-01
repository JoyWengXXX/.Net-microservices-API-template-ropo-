
namespace SignUpService.Cmd.Domain.DTOs
{
    public class UpdateUserInfosDTO
    {
        public string? userName { get; set; }
        public bool? isShowInvisibleGoods { get; set; }
        public int? defaultStoreGorup { get; set; }
        public string? timeZoneId { get; set; }
    }
}

