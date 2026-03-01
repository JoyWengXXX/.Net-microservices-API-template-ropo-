using CommonLibrary.Helpers;

namespace ConfigurationHelper.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("ConfigurationHelper 測試程式");
            Console.WriteLine("========================================\n");

            // 取得 DBContexts 路徑（相對於此測試程式的位置）
            // 在 Docker 中,檔案會被複製到 /app/ 目錄
            var dbContextPath = "/app";
            
            Console.WriteLine($"測試路徑: {dbContextPath}\n");

            // 測試 1: 預設環境 (dev)
            Console.WriteLine("=== 測試 1: 預設環境 (無設定環境變數) ===");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("EGMIOT__RabbitMQServer__Password", null);
            TestConfiguration(dbContextPath);

            Console.WriteLine("\n\n");

            // 測試 2: interdev 環境
            Console.WriteLine("=== 測試 2: interdev 環境 ===");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "interdev");
            Environment.SetEnvironmentVariable("EGMIOT__RabbitMQServer__Password", null);
            TestConfiguration(dbContextPath);

            Console.WriteLine("\n\n");

            // 測試 3: 使用環境變數覆寫
            Console.WriteLine("=== 測試 3: 環境變數覆寫 (EGMIOT__) ===");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "interdev");
            Environment.SetEnvironmentVariable("EGMIOT__RabbitMQServer__Password", "ENV_VAR_OVERRIDE_PASSWORD");
            TestConfiguration(dbContextPath);

            Console.WriteLine("\n========================================");
            Console.WriteLine("測試完成");
            Console.WriteLine("========================================");
        }

        private static void TestConfiguration(string basePath)
        {
            try
            {
                var config = CommonLibrary.Helpers.ConfigurationHelper.LoadConfiguration(basePath, "DBContextAppsettings");
                Console.WriteLine("\n? 測試成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? 錯誤: {ex.Message}");
                Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            }
        }

        private static string MaskPassword(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "(null)";

            // 簡單的密碼遮罩（僅用於顯示，實際測試會完整輸出）
            var parts = connectionString.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                {
                    var passwordPart = parts[i].Split('=');
                    if (passwordPart.Length == 2)
                    {
                        parts[i] = $"Password=***{passwordPart[1].Substring(Math.Max(0, passwordPart[1].Length - 4))}";
                    }
                }
            }
            return string.Join(';', parts);
        }
    }
}


