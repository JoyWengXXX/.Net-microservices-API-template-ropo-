using CommonLibrary.Helpers.Interfaces;

namespace CommonLibrary.Helpers
{
    public class BitCheckHelper : IBitCheckHelper
    {
        private static Lazy<BitCheckHelper> _instance = new Lazy<BitCheckHelper>(() => new BitCheckHelper());

        public static BitCheckHelper Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// 設定 Byte[]變數中特定位元的值
        /// </summary>
        /// <param name="bitsArray">位元組陣列</param>
        /// <param name="position">要設定的位元位置</param>
        /// <param name="value">要設定的布林值</param>
        public void SetBitStatus(byte[] bitsArray, int position, bool value)
        {
            int byteIndex = position / 8;
            int bitIndex = position % 8;
            if (value)
            {
                bitsArray[byteIndex] |= (byte)(1 << bitIndex); // 設定位元為1
            }
            else
            {
                bitsArray[byteIndex] &= (byte)~(1 << bitIndex); // 設定位元為0
            }
        }

        /// <summary>
        /// 從 Byte[]變數中 讀取特定位元狀態
        /// </summary>
        /// <param name="bitsArray">權限位元組陣列</param>
        /// <param name="bitPosition">位元位置</param>
        /// <returns>該位置的權限是否啟用</returns>
        public bool CheckBitsStatus(byte[] bitsArray, int bitPosition)
        {
            int byteIndex = bitPosition / 8;
            int bitIndex = bitPosition % 8;
            return (byteIndex < bitsArray.Length) && ((bitsArray[byteIndex] & (1 << bitIndex)) != 0);
        }
    }
}

