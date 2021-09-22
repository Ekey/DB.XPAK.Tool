using System;
using System.IO;
using System.Text;

namespace DB.Unpacker
{
    class XpakUnpackV1
    {
        public static Byte[] lpHash = new Byte[16];
        public static Byte[] lpLZMAProps = new Byte[5];

        public static void iDoIt(FileStream TFileStream, String m_File, String m_DstFolder, Boolean bHashedFile = false)
        {
            UInt32 dwArchiveSize = TFileStream.ReadUInt32();

            if (bHashedFile)
            {
                lpHash = TFileStream.ReadBytes(16);
            }

            lpLZMAProps = TFileStream.ReadBytes(5);
            lpLZMAProps = DB_Cipher.iDecryptData(lpLZMAProps, 5);

            UInt32 dwTableZSize = TFileStream.ReadUInt32();
			
			//Decrypt compressed size
            dwTableZSize ^= 0xAC54DF34;
			
			//Read and decrypt table
            var lpTable = TFileStream.ReadBytes((Int32)dwTableZSize);
            lpTable = DB_Cipher.iDecryptData(lpTable, (Int32)dwTableZSize);

            UInt32 dwBaseOffset = (UInt32)TFileStream.Position;
			
			//Decompress table
            var lpOut = LZMA.iDecompress(lpTable, lpLZMAProps, (Int32)dwTableZSize);
            using (MemoryStream TMemoryStream = new MemoryStream(lpOut))
            {
                Int32 dwTotalFiles = TMemoryStream.ReadInt32();
                for (Int32 i = 0; i < dwTotalFiles; i++)
                {
                    String m_FileName = TMemoryStream.ReadStringLength();

                    Int32 dwCompressedSize = TMemoryStream.ReadInt32();
                    Int32 bByte = TMemoryStream.ReadByte(); // 0x81, 0x85

                    Console.WriteLine("[UNPACKING]: {0}", m_FileName);

                    //Create output directory
                    String m_FullPath = m_DstFolder + m_FileName.Replace("/", @"\");
                    Utils.iCreateDirectory(m_FullPath);

                    //Decrypt and decompress file data
                    TFileStream.Seek(dwBaseOffset, SeekOrigin.Begin);

                    var lpBuffer = TFileStream.ReadBytes((Int32)dwCompressedSize);
                    lpBuffer = DB_Cipher.iDecryptData(lpBuffer, (Int32)dwCompressedSize);

                    if (bHashedFile)
                    {
                        if (bByte > 3)
                        {
                            var lpDest = LZMA.iDecompress(lpBuffer, lpLZMAProps, (Int32)dwCompressedSize);
                            File.WriteAllBytes(m_FullPath, lpDest);
                        }
                        else
                        {
                            File.WriteAllBytes(m_FullPath, lpBuffer);
                        }
                    }
                    else
                    {
                        if (bByte == 1)
                        {
                            var lpDest = LZMA.iDecompress(lpBuffer, lpLZMAProps, (Int32)dwCompressedSize);
                            File.WriteAllBytes(m_FullPath, lpDest);
                        }
                        else
                        {
                            File.WriteAllBytes(m_FullPath, lpBuffer);
                        }
                    }

                    dwBaseOffset += (UInt32)dwCompressedSize;
                }
            }
        }
    }
}
