using System;
using System.Diagnostics;
using System.IO;

namespace LaunchInOfflineMode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Trying to launch the downgrader...");

                string downgraderExecutable = ".\\IVDowngrader.exe";

                if (!File.Exists(downgraderExecutable))
                {
                    Console.WriteLine("Could not find file IVDowngrader.exe!");
                    Console.WriteLine("Press any key to close this window.");
                    Console.ReadKey();
                    return;
                }

                Process.Start(downgraderExecutable, "-offline");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while trying to launch the downgrader! Details: {0}", ex.ToString());
                Console.WriteLine();
                Console.WriteLine("Please report this bug in the #bug-report channel on Clonk's Discord Server with the error message above. Thanks!");
                Console.WriteLine("https://discord.gg/QtAgvkMeJ5");
                Console.WriteLine();
                Console.WriteLine("Press any key to close this window.");
                Console.ReadKey();
            }
        }
    }
}
