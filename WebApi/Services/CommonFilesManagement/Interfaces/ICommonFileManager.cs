

namespace CommonFilesManagement.Interfaces
{
    public interface ICommonFileManager
    {
        public string GetFileContent(string relativePath);
        public Stream GetFileStream(string relativePath);
        public byte[] GetFileBytes(string relativePath);
        public string GetFilePath(string relativePath);
        public string GetEnvironmentFilePath(string category, string fileName);
        public void ValidateCertificates();
    }
}

