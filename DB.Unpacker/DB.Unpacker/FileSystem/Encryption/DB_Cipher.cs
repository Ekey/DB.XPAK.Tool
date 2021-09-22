using System;

namespace DB.Unpacker
{
    class DB_Cipher
    {
        public static Byte[] iDecryptData(Byte[] lpBuffer, Int32 dwSize)
        {
            Int32 i, j;
            Int32 dwEncryptedSize;
            Int32 dwBlocks;

            i = 0;
            j = 0;
            dwEncryptedSize = dwSize / 4;
            if (dwEncryptedSize >= 4)
                dwBlocks = 4;
            else
                dwBlocks = dwEncryptedSize;
            while (i < dwBlocks)
            {
                UInt32 dwData = BitConverter.ToUInt32(lpBuffer, j);
                dwData ^= 0xAC54DF34;

                lpBuffer[j + 0] = (Byte)dwData;
                lpBuffer[j + 1] = (Byte)(dwData >> 8);
                lpBuffer[j + 2] = (Byte)(dwData >> 16);
                lpBuffer[j + 3] = (Byte)(dwData >> 24);
                ++i;
                j += 4;
            }

            return lpBuffer;
        }
    }
}
