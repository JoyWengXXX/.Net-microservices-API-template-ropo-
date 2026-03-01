using CommonFilesManagement.Interfaces;
using System.Reflection;

namespace CommonFilesManagement
{
    public class CommonFileManager : ICommonFileManager
    {
        private readonly string _environment;

        public CommonFileManager()
        {
            _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "dev";
        }

        private string GetAssemblyDirectory()
        {
            // 獲取 CommonFilesManagement 程序集的位置
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyLocation = assembly.Location;
            return Path.GetDirectoryName(assemblyLocation) ??
                throw new InvalidOperationException("無法獲取程序集位置");
        }

        public string GetFilePath(string relativePath)
        {
            try
            {
                // 移除開頭的斜線（如果存在）
                relativePath = relativePath.TrimStart('/', '\\');

                // 獲取檔案的完整路徑
                string assemblyDirectory = GetAssemblyDirectory();
                string fullPath = Path.GetFullPath(Path.Combine(assemblyDirectory, relativePath));

                if (!File.Exists(fullPath))
                {
                    // 如果在當前目錄找不到，嘗試往上層目錄尋找
                    string projectRootPath = Path.GetFullPath(Path.Combine(assemblyDirectory, ".."));
                    string alternativePath = Path.GetFullPath(Path.Combine(projectRootPath, relativePath));


                    if (File.Exists(alternativePath))
                    {
                        return alternativePath;
                    }

                    throw new FileNotFoundException(
                        $"找不到檔案。嘗試過以下路徑：\n" +
                        $"1. {fullPath}\n" +
                        $"2. {alternativePath}");
                }

                return fullPath;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public string GetFileContent(string relativePath)
        {
            string fullPath = GetFilePath(relativePath);
            return File.ReadAllText(fullPath);
        }

        public Stream GetFileStream(string relativePath)
        {
            string fullPath = GetFilePath(relativePath);
            return File.OpenRead(fullPath);
        }

        public byte[] GetFileBytes(string relativePath)
        {
            string fullPath = GetFilePath(relativePath);
            return File.ReadAllBytes(fullPath);
        }

        public string GetEnvironmentFilePath(string category, string fileName)
        {
            string relativePath = $"Files/{category}/{_environment}/{fileName}";
            return GetFilePath(relativePath);
        }

        public void ValidateCertificates()
        {
            var requiredFiles = new[]
            {
                $"Files/AWS/{_environment}/AmazonRootCA1.pem",
                $"Files/AWS/{_environment}/EgmServer-certificate.pfx",
                $"Files/FCM/{_environment}/firebase-adminsdk.json"
            };

            foreach (var file in requiredFiles)
            {
                try
                {
                    GetFilePath(file);
                }
                catch (FileNotFoundException)
                {
                    throw new FileNotFoundException(
                        $"缺少必要的憑證檔案：{file}，目前環境為：{_environment}。請確認已將憑證放入對應的環境目錄中。");
                }
            }
        }
    }
}

