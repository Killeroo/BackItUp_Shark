using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackItUp_Shark
{
    class Program
    {
        static void Main(string[] args)
        {
            // Local variable declaration
            string customBackupName = "";
            string customNameIdentifier = "/name:"; // Used to find the argument used to specify a custom backup name
            bool silentBackup = false; // Whether to display output or not
            bool mergeBackup = false; // Merge existing with existing backup

            /* ARGUMENTS CHECK */

            if (args.Length == 0) // Check program has any arguments
            {
                simpleTUI(); // Run simple drive backup menu
                System.Environment.Exit(1);
            }
            if (args[0] == "/?") // Print help message
            {
                displayHelpMsg();
                System.Environment.Exit(1);
            }
            if (!System.IO.Directory.Exists(args[0]) || !System.IO.Directory.Exists(args[1])) // Check valid paths have been provided
            {
                Console.WriteLine();
                Console.WriteLine("Inputted Paths do not exist, please provide valid paths to backup.");
                Console.WriteLine("Please input paths in the following format:");
                Console.WriteLine("'C:\\'  or 'C:\\Some_Path' or 'C:\\Some_Other_Path\\'");
                Console.WriteLine();
                Console.WriteLine("Type 'backitup.exe /?' to get more info.");
                System.Environment.Exit(1);
            }
            if (args.Length > 2) // Analyse other arguments
            {
                for (int count = 2; count < args.Length; count++) // Loop to end of arguments, ignore first 2 arguments (they are the source and destination paths)
                {
                    if (args[count].ToLower().Contains(customNameIdentifier))
                        customBackupName = args[count].Split(':')[1];
                    else if (args[count].ToLower().Contains("/silent"))
                        silentBackup = true;
                    else if (args[count].ToLower().Contains("/merge"))
                        mergeBackup = true;
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("Illegal arguments provided.");
                        Console.WriteLine();
                        Console.WriteLine("Type 'backitup.exe /?' for help");
                        System.Environment.Exit(1);
                    }
                }
            }

            /* INITIATE BACK UP */
            BackItUp_Shark_Core.Backup(args[0], args[1], customBackupName, silentBackup, mergeBackup);

            System.Environment.Exit(1); // Exit program
        }

        // Simple Terminal User Interface
        static void simpleTUI() 
        {
            while (true) // Main menu loop
            {
                List<System.IO.DriveInfo> driveList = new List<System.IO.DriveInfo>(); //= System.IO.DriveInfo.GetDrives().ToList();
                string backupTarget, backupLoc, input;
                int count = 1;

                Console.WriteLine("BackItUp Shark V1.2 [Build 28/10/2016]");
                Console.WriteLine("type 'help' for more, 'r' to refresh");
                Console.WriteLine();

                foreach (var drive in System.IO.DriveInfo.GetDrives())
                {
                    try
                    {
                        Console.Write("{0}      {1}       {2}GB free of {3}GB", count, drive.Name, (((drive.TotalFreeSpace / 1024) / 1024) / 1024), (((drive.TotalSize / 1024) / 1024) / 1024));
                        Console.SetCursorPosition(40, Console.CursorTop);
                        Console.WriteLine(drive.VolumeLabel);
                        driveList.Add(drive);
                        count++;
                    }
                    catch (System.IO.IOException) { }
                }

                // Get user input
                Console.WriteLine();
                Console.Write("Backup Source: ");
                input = Console.ReadLine();
                if (CheckMenuInput(input)) // Check user input
                    continue;
                backupTarget = driveList.ElementAt(Convert.ToInt32(input) - 1).Name;
                Console.Write("Backup Destination: ");
                input = Console.ReadLine();
                if (CheckMenuInput(input))
                    continue;
                backupLoc = driveList.ElementAt(Convert.ToInt32(input) - 1).Name;

                // Summary
                Console.WriteLine();
                Console.WriteLine("BackItUp_Shark will create back up of " + backupTarget);
                Console.WriteLine("Here : [" + backupLoc + "Backup]");

                // Confirm
                Console.WriteLine();
                Console.Write("Press Any Key to Start Backup . . .");
                Console.ReadKey();
                Console.WriteLine();

                // Initiate backup
                BackItUp_Shark_Core.Backup(backupTarget, backupLoc, "", false);
                
                // Pause before exit
                Console.Write("Press any key to exit...");
                Console.ReadLine();

                break; // Exit menu

            }
        }

        static bool CheckMenuInput(string input) // Check user input in simpleTUI()
        {
            bool SW = true;

            if (input == "help")
            {
                displayHelpMsg();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else if (input == "r")
            {

            }
            else if (input.IndexOfAny("1234567890".ToCharArray()) == -1) // Check input is number
            {
                Console.WriteLine("Please enter number of drive...");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else // Input is good and not command
            {
                SW = false;
            }

            return SW;
        }

        static void displayHelpMsg() 
        {
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("    BackItUp.exe /? [source] [destination] /silent /merge /name:[name] ");
            Console.WriteLine();
            Console.WriteLine("    [source]          Drive to back up (Required)");
            Console.WriteLine("    [destination]     Back up location (Required)");
            Console.WriteLine("    [silent]          Run back up with no output");
            Console.WriteLine("    [merge]           Merge with existing back up (NOTE Will only merge");
            Console.WriteLine("                      with back up of the same name");
            Console.WriteLine("    [name]            Name of back up");
            Console.WriteLine();
            Console.WriteLine("    Options:");
            Console.WriteLine("       /?             Displays help message");
            Console.WriteLine("       /name          Specifies a name for the backup folder");
            Console.WriteLine();
            Console.WriteLine("BackItUp_Shark is a drive backup utility.");
            Console.WriteLine();
            Console.WriteLine("Backed up files will be copied a folder called 'Backup' in [destination]");
            Console.WriteLine();
            Console.WriteLine("Default back up name is \"Backup_DAY-MONTH-YEAR\"");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("     >BackItUp G:\\ L:\\                       ... Backup drive G: to drive L:");
            Console.WriteLine("     >BackItUp Z:\\ C:\\ /name:Meow            ... Backup drive Z: to drive C: ");
            Console.WriteLine("                                               back up in folder named 'Meow'");
            Console.WriteLine("     >BackItUp C:\\ D:\\ /quiet /name:Meow     ... Backup drive Z: to drive C: ");
            Console.WriteLine("                                               back up in folder named 'Meow'");
            Console.WriteLine("                                               with no console output");
            Console.WriteLine();
            Console.WriteLine("Written by Matthew Carney (matthewcarney64@gmail.com) =^-^=");
        }

    }
}