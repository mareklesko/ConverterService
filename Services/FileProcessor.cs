using System;
using System.Diagnostics;
using System.IO;

namespace ConverterService.Services
{
    public class FileProcessor
    {
        private string FFMpegDir { get; set; }
        private string OutDir { get; set; }
        public FileProcessor(string ffmpegdir, string outDir)
        {
            FFMpegDir = ffmpegdir;
            OutDir = outDir;
        }

        public void ProcessFile(string file)
        {
            var f = new FileInfo(file);
            var fName = f.Name.Replace(f.Extension, "");
            fName += ".mp4";
            Console.WriteLine(file);

            Process myProcess = new Process();
            myProcess.StartInfo.FileName = Path.Combine(FFMpegDir, "ffmpeg.exe");
            myProcess.StartInfo.Arguments = $"-i \"{file}\" -c:v libx265 \"{Path.Combine(OutDir, fName)}\"";
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = false;
            myProcess.StartInfo.RedirectStandardOutput = false;
            myProcess.Start();
            myProcess.WaitForExit(50 * 1000);
        }

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);
    }
}
