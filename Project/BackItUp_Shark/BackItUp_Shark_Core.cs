using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace BackItUp_Shark
{
    class BackItUp_Shark_Core
    {

        // Declarations
        private static List<System.IO.FileInfo> backupFiles = new List<System.IO.FileInfo>(); // Stores All files to be backed up
        private static bool quietMode = false;

        // Main backup function
        public static void Backup(string targetPath, string destPath, string backupName, bool silent, bool merge = false)
        {
            // Set console output
            quietMode = silent;

            // Local variable assignment
            System.Diagnostics.Stopwatch stopWatch = null; // Back up timer
            System.IO.FileInfo[] driveFiles = null; // List of files being backed up
            string backupDestPath; // Backup location
            int fileCount = 0;

            if (backupName == "") // Generate default backup name if not specified
                backupName = createDefaultBackupName(targetPath);
            else if (backupName.Contains("DATE")) // Add timestamp
                backupName.Replace("DATE", "Backup_" + DateTime.Today.Date.Day + "-" + DateTime.Today.Date.Month + "-" + DateTime.Today.Date.Year);

            /* SET DESTINATION PATH */ 
            backupDestPath = System.IO.Path.Combine(destPath, "Backup", backupName); // (*userinputted drive*\Backup\*Backup Name*)

            /* MERGE OR PURGE BACKUP */
            if (merge)
                if (System.IO.Directory.Exists(backupDestPath)) // Check if back up exists before merging
                    MergeExistingBackup(backupName, backupDestPath, targetPath, GetFiles(targetPath).ToList());
                else
                    SetupBackupDir(backupDestPath, backupName);
            else
                SetupBackupDir(backupDestPath, backupName);
            
            /* FILE DISCOVERY */
            log("Discovering files in [ " + targetPath + " ]... ", "");
            driveFiles = GetFiles(targetPath);
            log("DONE", "", true);
            log(driveFiles.Count() + " files found.", "", true);

            log("Starting backup... ", "", true);
            stopWatch = System.Diagnostics.Stopwatch.StartNew(); // Start timer

            /* FILE COPY */
            foreach (System.IO.FileInfo file in driveFiles)
            {
                // Work out current destination path for folders
                string curPath = backupDestPath + (System.IO.Path.GetDirectoryName(file.FullName)).Split(':')[1]; //(file.FullName.Split(':')[1]); 

                // If path doesnt exist, create it
                if (!System.IO.Directory.Exists(curPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(curPath); 
                    }
                    catch (Exception)
                    {
                        log("ERROR CREATING BACKUP DESTINATION SUB FOLDER", "error", true);
                    }
                }

                try
                {
                    /* COPY FILE */

                    // Update title bar
                    double num = driveFiles.Count();
                    double percent = (fileCount / num) * 100;
                    log("", "", false, null, ConsoleColor.Gray, "BackItUp_Shark [RUNNING] " + Math.Round(percent, 0) + "% complete");

                    // Write current file
                    //Console.CursorTop = Console.CursorTop - 2;
                    //Console.Write(new string(' ', 60));
                    //Console.CursorLeft = 0;
                    log("", "copying", false, file);
					int left = Console.CursorLeft;
					int top = Console.CursorTop;
					Console.WriteLine();
					
					// Draw loading bar
                    LoadingBar(Math.Round(percent, 0));
					
					// Copy file
					Console.CursorLeft = left;
					Console.CursorTop = top;
					System.IO.File.Copy(file.FullName, System.IO.Path.Combine(curPath, file.Name)); //backupDestPath + file.FullName.Split(':')[1]);
                    log("[DONE]", "", true, null, ConsoleColor.DarkGreen);
					fileCount++;

                }
                catch (UnauthorizedAccessException)
                {
                    log("[FAIL]", "", true, null, ConsoleColor.DarkRed);
                    log("Access denied.", "error", true);
                }
                catch (System.IO.FileNotFoundException)
                {
                    log("[FAIL]", "", true, null, ConsoleColor.DarkRed);
                    log("Cannot find " + file.Name, "error", true);
                }
                catch (System.IO.PathTooLongException)
                {
                    log("[FAIL]", "", true, null, ConsoleColor.DarkRed);
                    log("Target path too long. Cannot copy " + file.FullName, "error", true);
                }
                catch (Exception)
                {
                    log("[FAIL]", "", true, null, ConsoleColor.DarkRed);
                    log(file.Name + " Cannot be copied.", "error", true);
                }
            }
            stopWatch.Stop(); // Stop stopwatch

            /* BACKUP SUMMARY */
            if (!quietMode)
                BackupSummary(fileCount, backupName, targetPath, backupDestPath, stopWatch.Elapsed);
        }

        // Create default backup name
        public static String createDefaultBackupName(String driveLetter)
        {
            String name;
            System.IO.DriveInfo drive = null;
            driveLetter = System.IO.Path.GetPathRoot(driveLetter); // Make sure we are targetting the root

            foreach (var d in System.IO.DriveInfo.GetDrives())
                if (d.Name == driveLetter)
                    drive = d;

            if (drive.VolumeLabel == "")
                name = "Backup";
            else
                name = drive.VolumeLabel;

            name += "_" + DateTime.Today.Date.Day + "-" + DateTime.Today.Date.Month + "-" + DateTime.Today.Date.Year;

            return name;
        }

        static void SetupBackupDir(string backupDestPath, string backupName)
        {
            // Check directory exists
            if (!System.IO.Directory.Exists(backupDestPath))
            {
                /* CREATE BACKUP FOLDER */
                try
                {
                    System.IO.Directory.CreateDirectory(backupDestPath);
                }
                catch (UnauthorizedAccessException)
                {
                    log("ACCESS TO BACKUP DIRECTORY DENIED (Run as Admin then try again)", "error", true);
                }
                catch (System.IO.IOException)
                {
                    log("ERROR CREATING BACKUP DIRECTORY", "error", true);
                }
                catch (Exception)
                {
                    log("AN HAS ERROR OCCURED CREATING BACKUP FOLDER", "error", true);
                }

                log("Backup directory [" + backupName + "] Created.", "", true);
            }
            else
            {
                /* DELETE EXISTING BACKUP FOLDER */
                log("Backup folder [" + backupName + "] found.", "", true);
                log("", "", false, null, ConsoleColor.Gray, "BackItUp_Shark [PURGING]");
                log("Purging previous backup data ... ", "", true, null, ConsoleColor.DarkRed);

                // Delete files in folder root
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(backupDestPath);
                log(di.FullName, "deleting", true);
                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception)
                    {
                        log("File [" + file.Name + "] could not be deleted.", "warning", true);
                    }
                }

                // Delete files from sub folders
                foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        log(dir.FullName, "deleting", true);
                        dir.Delete(true);
                    }
                    catch (Exception)
                    {
                        log("Folder [ " + dir.Name + " ] could not be deleted.", "warning", true);
                    }
                }

                log("Purging complete.", "", true, null, ConsoleColor.DarkRed);
            }
        }

        static void MergeExistingBackup(string backupName, string existingBackupPath, string newFilesPath, List<FileInfo> newFiles)
        {
            // Local variable setup
            int fileCount = 0;
            List<System.IO.FileInfo> existingFiles = new List<FileInfo>(); // list containing files present in backup
            FileCompare myFileCompare = new FileCompare(); // Custom FileCompare object
            existingFiles = GetFiles(existingBackupPath).ToList(); // Get list of files in existing backup

            /* FIND NEW OR MODIFIED FILES */
            List<System.IO.FileInfo> missingFromBackupFiles = (from file in newFiles select file).Except(existingFiles, myFileCompare).ToList(); // use custom filecompare class and find missing or changed files between existing backup folder and backup targets

            log("Backup folder [" + backupName + "] found.", "", true);
            log("", "", false, null, ConsoleColor.Gray, "BackItUp_Shark [UPDATING]");
            log("Updating back up ... ", "", true, null, ConsoleColor.DarkCyan);

            /* BACKUP UPDATE & ADD */
            foreach (var file in missingFromBackupFiles)
            {
                // Work out new paths
                string[] seperators = { newFilesPath }; // Split by the newFiles parameter
                string[] splitPath = file.FullName.Split(seperators, StringSplitOptions.RemoveEmptyEntries); // Split against the full filename of of the missing file (result will be the path AFTER the backup root)
                string curFullDest = System.IO.Path.Combine(existingBackupPath, splitPath[0]); // Full path to backup (including file name)
                string curPath = System.IO.Path.GetDirectoryName(curFullDest); // current directory being copied too

                // Create new directory in backup if necessary
                if (!System.IO.Directory.Exists(curPath))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(curPath);
                    }
                    catch (Exception)
                    {
                        log("ERROR CREATING FOLDER IN EXISTING BACKUP", "error", true);
                    }
                }

                try
                {
                    /* FILE COPY */
                    log(file.FullName + " ... ", System.IO.File.Exists(curFullDest) ? "updating" : "adding");
                    System.IO.File.Copy(file.FullName, curFullDest, true);
                    log("DONE", "", true, null, System.IO.File.Exists(curFullDest) ? ConsoleColor.DarkCyan : ConsoleColor.DarkGreen);
                    fileCount++;
                    // Update title bar
                    double num = missingFromBackupFiles.Count();
                    double percent = (fileCount / num) * 100;
                    log("", "", false, null, ConsoleColor.DarkGray, "BackItUp_Shark [UPDATING] " + Math.Round(percent, 0) + "% complete");
                }
                catch (UnauthorizedAccessException)
                {
                    log("ACCESS DENIED (Run as Admin then try again)", "error", true);
                }
                catch (System.IO.FileNotFoundException)
                {
                    log("ERROR FILE [" + file.Name + "] COULD NOT BE FOUND", "error", true);
                }
                catch (Exception)
                {
                    log("ERROR FILE [" + file.Name + "] COUNT NOT BE COPIED", "error", true);
                }
            }

            log("[" + backupName + "] merge done.", "", true, null, ConsoleColor.DarkCyan);
            log("Backup complete.", "", false, null, ConsoleColor.Green);
            System.Environment.Exit(1); // Exit after updating
            return;
        }

        // Lists all files in specified path, maps all files in all sub dirs to global list driveFiles
        static System.IO.FileInfo[] GetFiles(string path)
        {
            System.IO.DirectoryInfo rootPath = new DirectoryInfo(path); // Get our root to look for files from
            backupFiles.Clear(); // Reset the list
            ListFiles(rootPath); // Populate list

            return backupFiles.ToArray(); // Convert list to array send back
        }

        // Recursive file discovery function
        static void ListFiles(System.IO.DirectoryInfo path)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            try
            {
                files = path.GetFiles();
            }
            catch (UnauthorizedAccessException)
            {
                log("ACCESS TO BACKUP DIRECTORY DENIED (Run as Admin then try again)", "error", true);
            }
            catch (System.IO.DriveNotFoundException)
            {
                log("ERROR FILE MISSING", "error", true);
            }
            catch (Exception e)
            {
                log("EXCEPTION [" + e.Message + "]", "error", true);
            }

            // Print each of the files
            if (files != null)
            {
                foreach (System.IO.FileInfo file in files)
                    backupFiles.Add(file); // Add to global list of files

                subDirs = path.GetDirectories();

                foreach (System.IO.DirectoryInfo dir in subDirs)
                    ListFiles(dir); // Recurse back into ourselves
            }
        }

        static void log(string message = "", string status = "", bool newLine = false, FileInfo file = null, ConsoleColor color = ConsoleColor.Gray, string windowBarMsg = "") //long fileSizeBytes = 0L)
        {
            if (!quietMode) // If quiet mode don't display output
            {
                // Change window bar text
                if (windowBarMsg != "")
                {
                    Console.Title = windowBarMsg;
                    return;
                }

                // Determine colour by status
                switch (status)
                {
                    case "copying":
                        double fileSizeKb = Math.Round((double)file.Length / 1024f, 2);
                        long fileSizeMb = (long)((file.Length / 1024f) / 1024f); // Convert to mb
                        if (fileSizeMb > 500L) // If file is over 500mb
                            color = ConsoleColor.DarkMagenta;
                        else if (fileSizeMb > 50L) // If file is over 50mb
                            color = ConsoleColor.DarkCyan;
                        else
                            color = ConsoleColor.DarkGreen;
                        message = "COPYING : " + file.Name + " Size: " + ((fileSizeMb > 1L) ? fileSizeMb + "mb... " : fileSizeKb + "kb... ");
                        break;

                    case "error":
                        color = ConsoleColor.DarkRed;
                        message = "ERROR : " + message;
                        break;

                    case "deleting":
                        color = ConsoleColor.DarkRed;
                        message = "DELETING : " + message;
                        break;

                    case "warning":
                        color = ConsoleColor.DarkMagenta;
                        message = "WARNING : " + message;
                        break;
                    
                    case "updating":
                        color = ConsoleColor.DarkCyan;
                        message = "UPDATING : " + message;
                        break;

                    case "adding":
                        color = ConsoleColor.DarkGreen;
                        message = "ADDING : " + message;
                        break;
                }

                // Write log message
                Console.ForegroundColor = color;
                Console.Write(message);

                if (newLine)
                    Console.WriteLine();

                // Reset console color
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        static void LoadingBar(double percent)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("{0}% [                        ] ", Math.Round(percent));
            Console.CursorLeft = 16;
            Console.WriteLine(new String('#', Convert.ToInt32(Math.Round((25 * (percent / 100)), 0))));
        }

        static void BackupSummary(int fileCount, string backupName, string backupTarget, string backupDestPath, TimeSpan timeTaken)//System.Diagnostics.Stopwatch timeTaken)
        {
            Console.Title = "BackItUp_Shark [COMPLETE]";
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("               BACKUP COMPLETED");
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("                  BACKUPLOG");
            Console.WriteLine("###############################################");
            Console.WriteLine();
            Console.WriteLine("[" + backupTarget + "] backed up");
            Console.WriteLine("Files copied: " + fileCount);
            Console.WriteLine("Time taken: {0} hour(s) {1} minutes(s) {2} second(s)", timeTaken.Hours, timeTaken.Minutes, timeTaken.Seconds.ToString() + "." + timeTaken.Milliseconds);
            Console.WriteLine("Time completed : " + DateTime.Now);
            Console.WriteLine("Backup name: " + backupName);
            Console.WriteLine("Location: " + backupDestPath);
            Console.WriteLine();
            Console.WriteLine("###############################################");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // Custom FileCompare class (inherits Generic comparer) which compares files based on file name, length and access time
        class FileCompare : System.Collections.Generic.IEqualityComparer<System.IO.FileInfo>
        {
            // Constructor
            public FileCompare() { }

            // Inherited class stating criteria to compare two lists by
            public bool Equals(System.IO.FileInfo file1, System.IO.FileInfo file2)
            {
                return (file1.Name == file2.Name &&
                        file1.Length == file2.Length &&
                        file1.LastAccessTime == file2.LastAccessTime &&
                        GetHashCode(file1) == GetHashCode(file2));
            }

            // Get hash code of a file
            public int GetHashCode(System.IO.FileInfo file)
            {
                string s = String.Format("{0}{1}", file.Name, file.Length);
                return s.GetHashCode();
            }
        }
    }
}