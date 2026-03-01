namespace CommonLibrary.Helpers.Interfaces
{
    public interface IBitCheckHelper
    {
        void SetBitStatus(byte[] bitsArray, int position, bool value);
        bool CheckBitsStatus(byte[] bitsArray, int bitPosition);
    }
}
