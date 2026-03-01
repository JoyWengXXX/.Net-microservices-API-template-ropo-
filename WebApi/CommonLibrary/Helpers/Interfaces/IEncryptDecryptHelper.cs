namespace CommonLibrary.Helpers.Interfaces
{
    public interface IEncryptDecryptHelper
    {
        public string AESDecrypt(string encryptedData, string keyStr, int keySizeInBits = 256);
        public string AESEncrypt(string input, string keyStr, int keySizeInBits = 256);
        public byte[] CreateHash(string input, EncryptDecryptHelper.HashType hashType);
    }
}
