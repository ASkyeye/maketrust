using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace maketrust
{
    class CertificateInjector
    {
        public static void Inject(string from, string to)
        {
            byte[] buffer;
            using (FileStream fs = File.Open(from.NormalizePath(), FileMode.Open, FileAccess.Read))
            using (BinaryReader rd = new BinaryReader(fs))
            {
                fs.Seek(0x3c, SeekOrigin.Begin);
                int e_lfanew = rd.ReadInt32();
                Console.WriteLine("e_lfanew from source: {0}", e_lfanew.ToString("x8"));
                fs.Seek(e_lfanew + 0x98, SeekOrigin.Begin);
                Console.WriteLine("Security location in source: {0}", (e_lfanew + 0x98).ToString("x8"));
                int address = rd.ReadInt32();
                Console.WriteLine("address from source: {0}", address.ToString("x8"));
                int size = rd.ReadInt32();
                Console.WriteLine("size from source: {0}", size.ToString("x8"));
                buffer = new byte[size];
                fs.Seek(address, SeekOrigin.Begin);
                fs.Read(buffer, 0, buffer.Length);
            }
            using (FileStream fs = File.Open(to.NormalizePath(), FileMode.Open, FileAccess.ReadWrite))
            using (BinaryWriter wr = new BinaryWriter(fs))
                using (BinaryReader rd = new BinaryReader(fs))
            {
                fs.Seek(0x3c, SeekOrigin.Begin);
                int e_lfanew = rd.ReadInt32();
                Console.WriteLine("e_lfanew from target: {0}", e_lfanew.ToString("x8"));
                Console.WriteLine("Security location in target: {0}", (e_lfanew + 0x98).ToString("x8"));
                fs.Seek(e_lfanew + 0x98, SeekOrigin.Begin );
                wr.Write((int)fs.Length);
                wr.Write(buffer.Length);
                fs.Seek(0, SeekOrigin.End);
                wr.Write(buffer, 0, buffer.Length);
                wr.Flush();
            }
        }
    }
}
