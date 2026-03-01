
namespace Service.Background.Models
{
    public class FCMMessageRoot
    {
        public FCMMassage message { get; set; }
    }

    public class FCMMassage
    {
        public string token { get; set; }
        public FCMData data { get; set; }
        public FCMNotification notification { get; set; }
    }

    public class FCMData
    {
        public string title { get; set; }
        public string body { get; set; }
        public string key_1 { get; set; }
        public string key_2 { get; set; }
    }

    public class FCMNotification
    {
        public string title { get; set; }
        public string body { get; set; }
    }
}

