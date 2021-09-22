using System;
using System.IO;
using System.Text;

namespace DB.Unpacker
{
    class XpakUnpackV2
    {
        public static void iDoIt(FileStream TFileStream, String m_File, String m_DstFolder)
        {
            UInt32 dwArchiveSize = TFileStream.ReadUInt32();

            var lpHash = TFileStream.ReadBytes(16);
            Int32 dwReserved = TFileStream.ReadInt32(); // reserved / placeholder -> just zero ??

            UInt32 dwTableZSize = TFileStream.ReadUInt32();

            //Decrypt compressed size
            dwTableZSize ^= 0xAC54DF34;

            //Read and decrypt table
            var lpTable = TFileStream.ReadBytes((Int32)dwTableZSize);
            lpTable = DB_Cipher.iDecryptData(lpTable, (Int32)dwTableZSize);

            UInt32 dwBaseOffset = (UInt32)TFileStream.Position;

            //Decompress table
            var lpOut = Brotli_LZ77.iDecompress(lpTable);

            using (MemoryStream TMemoryStream = new MemoryStream(lpOut))
            {
                Int32 dwTotalFiles = TMemoryStream.ReadInt32();
                for (Int32 i = 0; i < dwTotalFiles; i++)
                {
                    String m_FileName = TMemoryStream.ReadStringLength();

                    Int32 dwCompressedSize = TMemoryStream.ReadInt32();
                    Int32 bByte = TMemoryStream.ReadByte(); // flag & 0x40

                    Console.WriteLine("[UNPACKING]: {0}", m_FileName);

                    //Create output directory
                    String m_FullPath = m_DstFolder + m_FileName.Replace("/", @"\");
                    Utils.iCreateDirectory(m_FullPath);

                    if ((bByte & 0x40) != 0)
                    {
                        var lpBuffer = TMemoryStream.ReadBytes((Int32)dwCompressedSize);
                        File.WriteAllBytes(m_FullPath, lpBuffer);
                    }
                    else
                    {
                        //Decrypt and decompress file data
                        TFileStream.Seek(dwBaseOffset, SeekOrigin.Begin);

                        var lpBuffer = TFileStream.ReadBytes((Int32)dwCompressedSize);
                        lpBuffer = DB_Cipher.iDecryptData(lpBuffer, (Int32)dwCompressedSize);

                        //Lazy check :B
                        if (bByte < 0x40)
                        {
                            File.WriteAllBytes(m_FullPath, lpBuffer);
                        }
                        else
                        {
                            var lpDest = Brotli_LZ77.iDecompress(lpBuffer);
                            File.WriteAllBytes(m_FullPath, lpDest);
                        }

                        dwBaseOffset += (UInt32)dwCompressedSize;
                    }
                }
            }
        }
    }
}
