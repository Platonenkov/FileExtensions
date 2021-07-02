using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace TestConsoleLockFile
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var file = new FileInfo("Test.csv");
            await CheckAndWait(file);

            await File.WriteAllTextAsync(file.FullName, "1;2;3;4", Encoding.UTF8);

            new Process {StartInfo = new ProcessStartInfo(file.FullName) {UseShellExecute = true}}.Start();

            Console.WriteLine("File will open after 5 second");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await CheckAndWait(file);



        }

        static async Task CheckAndWait(FileInfo file)
        {
            if (file.IsLocked())
            {
                Console.WriteLine();
                Console.WriteLine($"File is locked {file.FullName} by processes:");
                foreach (var lock_process in file.EnumLockProcesses())
                {
                    Console.WriteLine($"{lock_process.ProcessName}");
                }

                Console.WriteLine();
                Console.WriteLine("Wait while file is lock");
                await file.WaitFileLockAsync();
                Console.WriteLine("file was unlocked");
                Console.WriteLine();
                Console.WriteLine("Press any key to close");
                Console.ReadKey();
            }

        }
    }
}
