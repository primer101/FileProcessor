using System;
using System.Text;
using System.IO;
using System.Threading;

namespace FileProcessor
{
    static class FileProcesor
    {

        static Mutex _mutex = null;
        static bool _doesNotExist = false; //if mutex exists

        public static void ProcessFile(string path, string outputfile)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(outputfile))
            {
                //or throw new ArgumentNullException() for each parameter
                return;
            }
            else
            {
                if (Directory.Exists(path))
                {
                    while (true)
                    {
                        FileStream lockfile = null;
                        int count = 0;
                        try
                        {
                            string[] files = Directory.GetFiles(path);

                            foreach (var file in files)
                            {
                                //continue to the next file if not found or locked
                                if (!File.Exists(file) || IsALockFile(file))
                                {
                                    continue;
                                }
                                else
                                {
                                    // try to create a lock file for the current file
                                    try
                                    {
                                        lockfile = new FileStream(file + ".lock", FileMode.CreateNew);
                                    }
                                    catch (IOException)
                                    {
                                        //if I can't create a lock file then a file is being processed, so continue
                                        continue;
                                    }


                                    //start the file processing
                                    FileInfo fI = new FileInfo(file);
                                    FileInfo log = new FileInfo(outputfile);

                                    //try to access the output file
                                    //controlled by a mutex
                                    //if another proccess owns this
                                    //wait for it
                                    _mutex = OwnMutex(log);
                                    if (_mutex != null)
                                    {
                                        //Write to file
                                        WriteEntry(fI, log);

                                        //release the mutex so
                                        //other processes can access the output file
                                        _mutex.ReleaseMutex();

                                        //move the processed file, if the folder "Processed" doesn't exist create it
                                        MoveFile(path, fI);

                                        //remove the lock file (allowing other processes to access the file)
                                        CloseAndRemoveFile(lockfile);
                                        count++; //count a file proceced
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            DisplayException(ex); //also we can write to a log file
                        }
                        finally
                        {
                            //always remove the lock, 
                            //allowing other processes to access the file
                            if (lockfile != null)
                            {
                                CloseAndRemoveFile(lockfile);
                            }
                            if (_mutex != null)
                            {
                                _mutex.Dispose();
                            }
                            Console.Write(count + " "); //write the amount of processed files
                        }
                        Thread.Sleep(1000);//every 1 seconds check the folder
                    }//while

                }
                else //the input folder does not exist in app.config
                {
                    Console.WriteLine("The input folder does not exist. Do you want create it? (Y/N):");
                    if (char.ToUpper((char)Console.Read()) == 'Y')
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                            Console.WriteLine("Please, copy the files to process to {0}", path);
                        }
                        catch (Exception)
                        {

                            Console.WriteLine("The folder could not be created, please check the settings in the app.config");
                        }
                    }
                }
            }
        }

        private static void MoveFile(string path, FileInfo fI)
        {
            string procPath = Path.Combine(path, "Processed");
            if (!Directory.Exists(procPath))
            {
                Directory.CreateDirectory(procPath);
            }
            if (fI.Exists)
            {
                fI.MoveTo(procPath + "/" + fI.Name);
            }
        }

        private static void CloseAndRemoveFile(FileStream lockfile)
        {
            lockfile.Close(); //and Dispose
            if (File.Exists(lockfile.Name))
            {
                File.Delete(lockfile.Name);
            }
        }

        private static Mutex OwnMutex(FileInfo log)
        {
            _doesNotExist = false;
            try
            {
                // Try to open existing mutex.
                _mutex = Mutex.OpenExisting(log.Name + "_writing");
            }
            catch
            {
                _doesNotExist = true;
            }

            if (_doesNotExist)//create it
            {
                _mutex = new Mutex(true, log.Name + "_writing");
            }
            else
            {
                //Try to gain control of the named mutex. If the mutex is  
                // controlled by another thread wait for it to be released.
                _mutex.WaitOne();
            }
            return _mutex;
        }

        private static bool IsALockFile(string file)
        {
            return Path.GetExtension(file) == ".lock";
        }


        static void WriteEntry(FileInfo fI, FileInfo log)
        {
            using (StreamWriter sw = File.AppendText(log.FullName))
            {
                sw.WriteLine("{0}\t{1}\t{2} KBytes", DateTime.Now, fI.FullName, fI.Length / 1024);
            }
        }


        private static void DisplayException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("An exception of type \"");
            sb.Append(ex.GetType().FullName);
            sb.Append("\" has occurred.\r\n");
            sb.Append(ex.Message);
            Console.WriteLine(sb.ToString());
        }
    }
}
