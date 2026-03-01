

namespace Service.Common.Models.DTOs
{
    public class ResponseDTO
    {
        public object result { get; set; }
        public bool isSuccess { get; set; }
        public string message { get; set; }
        public string? resultCode { get; set; } = null;
    }
}

