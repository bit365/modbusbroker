namespace ModbusWorker
{
    public class ByteSwapper
    {
        public static byte[] ByteSwap(byte[] bytes, int swapSize)
        {
            MemoryStream memoryStream = new();

            var subarray = new byte[swapSize];

            for (int i = 0; i < bytes.Length; i += swapSize)
            {
                Array.Copy(bytes, i, subarray, 0, swapSize);
                Array.Reverse(subarray);
                memoryStream.Write(subarray, 0, swapSize);
            }

            return memoryStream.ToArray();
        }
    }
}
