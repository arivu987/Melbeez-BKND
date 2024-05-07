using System;
using System.IO;

namespace Melbeez.Business.Common.Services
{
    public class LoggerService
    {
        public static void Log(string line, string dir = null)
        {
            string basePathOfLogFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Logs");

            try
            {
                DateTime now = DateTime.Now;
                string TargetDir = Path.Combine(basePathOfLogFile, dir);

                if (!Directory.Exists(TargetDir))
                {
                    Directory.CreateDirectory(TargetDir);
                }

                var newFileName = DateTime.Now.ToString("yyyy_MM_dd_HH") + ".txt";

                using (StreamWriter sw = System.IO.File.AppendText(Path.Combine(TargetDir, newFileName)))
                {
                    sw.WriteLine($"{DateTime.Now:yyyyMMdd_hh_mm_ss}--------{line}");
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
