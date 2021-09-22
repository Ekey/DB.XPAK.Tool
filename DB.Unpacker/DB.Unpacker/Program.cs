using System;
using System.IO;
using System.Reflection;

namespace DB.Unpacker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Dogbyte Games XPAK UnPacker");
            Console.WriteLine("(c) 2021 Ekey (h4x0r) / v{0}\n", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.ResetColor();

            if (args.Length != 2)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    DB.Unpacker <m_File> <m_Directory>\n");
                Console.WriteLine("    m_File - Source of XPAK file");
                Console.WriteLine("    m_Directory - Destination directory\n");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    DB.Unpacker D:\\main.33.com.dogbytegames.zombiesafari.obb D:\\Unpacked");
                Console.ResetColor();
                return;
            }

            String m_Input = args[0];
            String m_Output = Utils.iCheckArgumentsPath(args[1]);

            if (!File.Exists("brolib_x86.dll") || !File.Exists("Brotli.Core.dll"))
            {
                Utils.iSetError("[ERROR]: Unable to find Brotli modules");
                return;
            }

            if (!File.Exists(m_Input))
            {
                Utils.iSetError("[ERROR]: Input file -> " + m_Input + " <- does not exist!");
                return;
            }

            FileStream TFileStream = new FileStream(m_Input, FileMode.Open);

            UInt32 dwMagic = TFileStream.ReadUInt32();
            if (dwMagic != 0x4B415058)
            {
                Utils.iSetError("[ERROR]: Invalid magic of XPAK archive file!");
                return;
            }

            Int32 dwVersion = TFileStream.ReadInt32();
            switch (dwVersion)
            {
                case 0: XpakUnpackV1.iDoIt(TFileStream, m_Input, m_Output, false); break;
                case 1: XpakUnpackV1.iDoIt(TFileStream, m_Input, m_Output, true); break;
                case 2: XpakUnpackV2.iDoIt(TFileStream, m_Input, m_Output); break;
                default: Utils.iSetError("[ERROR]: Unsupported version of XPAK archive file > " + dwVersion); break;
            }
        }
    }
}
