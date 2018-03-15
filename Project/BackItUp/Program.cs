using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackItUp
{
    class Program
    {
        // Multi thread?
        // With max thread count?

        static string sourcePath;
        static string destinationPath;

        // Results variables
        static int files = 0;
        static int folders = 0;
        static int errors = 0;
        static int matches = 0;

        const string USAGE = "Backitup [Source Path] [Destination Path]";

        static void Main(string[] args)
        {
            // Arguments check
            if (args.Length != 2) {
                Warning("Not enough arguments.");
                Console.WriteLine(USAGE);
                Environment.Exit(1);
            }

            // Check path to backup exists
            if (!Directory.Exists(args[1])) {
                Error("The path you want to backup doesn't exist, please pick another path and try again.");
            }



            DiscoverFiles(args[0]);
        }

        // Recursive file discovery function
        private static void DiscoverFiles(string path)
        {

            try {

                // List all files in directory
                foreach (var file in Directory.GetFiles(path)) {

                    // Check file against search params
                    files++;
                    CopyFile(file);
                }

            } catch (Exception) {
                errors++;
            }


            try {

                // Go through directories
                foreach (var folder in Directory.GetDirectories(path)) {

                    // Recurse into each directory
                    folders++;
                    DiscoverFiles(folder);
                }

            } catch (Exception) {
                errors++;
            }
        }

        private static void CopyFile(string path)
        {
            Console.WriteLine("COPYING: {0}", Path.GetFileName(path));
        }

        static void SetupBackupDir(string path, string backupName)
        {
            // Check backup directory exists
            if (!System.IO.Directory.Exists(path)) {

                // If not create it
                try {
                    System.IO.Directory.CreateDirectory(path);
                } catch (UnauthorizedAccessException) {
                    Error("Access to backup directory denied (Run as Admin then try again)");
                } catch (System.IO.IOException) {
                    Error("Could not create backup directory (IO Error)");
                } catch (Exception) {
                    Error("An error occured while creating backup folder");
                }

                Console.WriteLine("Backup directory [{0}] created.", backupName);
            } else {
                
                // If it does, delete its contents
                Console.WriteLine("Backup folder [{0}] found.", backupName);
                Console.Title = "BackItUp_Shark [PURGING]";
                Console.WriteLine("Purging previous backup data ... ");

                // Delete files in folder root
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);
                Console.WriteLine("DELETING: {0}", di.FullName);
                foreach (System.IO.FileInfo file in di.GetFiles()) {
                    try {
                        file.Delete();
                    } catch (Exception) {
                        Console.WriteLine("File [{0}] could not be deleted.");
                    }
                }

                // Delete files from sub folders
                foreach (System.IO.DirectoryInfo dir in di.GetDirectories()) {
                    try {
                        Console.WriteLine("DELETING: {0}", dir.FullName);
                        dir.Delete(true);
                    } catch (Exception) {
                        Console.WriteLine("Folder [{0}] could not be deleted.");
                    }
                }

                Console.WriteLine("Purging complete.");
            }
        }

        private static void Warning(string msg)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: " + msg);
            Console.ForegroundColor = color;
        }

        private static void Error(string msg)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + msg);
            Console.ForegroundColor = color;

            Environment.Exit(1);
        }
    }
}
