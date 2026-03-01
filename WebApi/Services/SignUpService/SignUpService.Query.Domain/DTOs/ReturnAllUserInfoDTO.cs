

namespace SignUpService.Query.Domain.DTOs
{
    public class ReturnAllUserInfoDTO
    {
        public List<AllUser> allUsers { get; set; }
        public int totalPage { get; set; }
    }

    public class AllUser
    {
        public string userId { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public bool isVarified { get; set; }
        public int SSOType { get; set; }
        public string timeZoneId { get; set; }
    }
}

