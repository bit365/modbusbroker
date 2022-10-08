namespace ModbusWorker.Handlers
{
    public class ModbusCrcCalculator
    {
        public static ushort GetModbusCrc(byte[] bytes, int length)
        {
            ushort crc = 0xFFFF;

            for (int i = 0; i < length; i++)
            {
                crc ^= bytes[i];               // XOR byte into least sig. byte of crc

                for (int j = 8; j != 0; j--)   // Loop over each bit
                {
                    if ((crc & 0x0001) != 0)   // If the LSB is set
                    {
                        crc >>= 1;             // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                       // Else LSB is not set
                    {
                        crc >>= 1;             // Just shift right
                    }
                }
            }
            // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
            return crc;
        }
    }
}
