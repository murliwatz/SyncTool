using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace SyncTool
{
    class SyncFolderObject
    {
        Thread t;
        public int Interval = 500;
        public string f1 = "";
        public string f2 = "";
        private bool stopped = false;

        public SyncFolderObject() {
           
        }

        private void sync() {
            if (!Directory.Exists(f1) || !Directory.Exists(f2)) this.Stop();
           Dictionary<string, string> tmp1 = new Dictionary<string,string>();
            Dictionary<string, string> tmp2 = new Dictionary<string,string>();
            Dictionary<string, string> tmp3 = new Dictionary<string, string>();
            Dictionary<string, string> tmp4 = new Dictionary<string, string>();
            Dictionary<string, SyncFolderObject> subsyncs = new Dictionary<string, SyncFolderObject>();
            while (!stopped)
            {
                if (!Directory.Exists(f1) || !Directory.Exists(f2)) break;
                List<string> fpaths1 = Directory.GetFiles(f1).ToList<string>();
                List<string> fpaths2 = Directory.GetFiles(f2).ToList<string>();
                List<string> fopaths1 = Directory.GetDirectories(f1).ToList<string>();
                List<string> fopaths2 = Directory.GetDirectories(f2).ToList<string>();
                Dictionary<string, string> files1 = new Dictionary<string, string>();
                Dictionary<string, string> files2 = new Dictionary<string, string>();
                Dictionary<string, string> folders1 = new Dictionary<string, string>();
                Dictionary<string, string> folders2 = new Dictionary<string, string>();
                for (int i = 0; i < fpaths1.Count; i++)
                {
                    FileInfo fi = new FileInfo(fpaths1[i]);
                    files1.Add(fi.Name, fi.FullName);
                }
                for (int i = 0; i < fpaths2.Count; i++)
                {
                    FileInfo fi = new FileInfo(fpaths2[i]);
                    files2.Add(fi.Name, fi.FullName);
                }
                for (int i = 0; i < fopaths1.Count; i++)
                {
                    DirectoryInfo di = new DirectoryInfo(fopaths1[i]);
                    folders1.Add(di.Name, di.FullName);
                    if (!subsyncs.ContainsKey(Path.Combine(f1, di.Name)))
                    {
                        SyncFolderObject s = new SyncFolderObject();
                        s.f1 = Path.Combine(f1,di.Name);
                        s.f2 = Path.Combine(f2, di.Name);
                        s.Interval = this.Interval;
                        s.Start();
                        subsyncs.Add(s.f1, s);
                    }
                }
                for (int i = 0; i < fopaths2.Count; i++)
                {
                    DirectoryInfo di = new DirectoryInfo(fopaths2[i]);
                    folders2.Add(di.Name, di.FullName);
                }

                try
                {
                    // File Actions
                    foreach (string file in tmp1.Keys)
                    {
                        if (!files1.ContainsKey(file))
                        {
                            File.Delete(files2[file]);
                            files2.Remove(file);
                        }
                    }

                    foreach (string file in tmp2.Keys)
                    {
                        if (!files2.ContainsKey(file))
                        {
                            File.Delete(files1[file]);
                            files1.Remove(file);
                        }
                    }

                    foreach (string file in files2.Keys)
                    {
                        if (!files1.ContainsKey(file))
                        {
                            File.Copy(files2[file], Path.Combine(f1,file));
                            files1.Add(file, files2[file]);
                        }
                    }

                    foreach (string file in files1.Keys)
                    {

                        if (!files2.ContainsKey(file))
                        {
                            File.Copy(files1[file], Path.Combine(f2,file));
                            files2.Add(file, files1[file]);
                        }
                        else
                        {
                            FileInfo fi1 = new FileInfo(files1[file]);
                            FileInfo fi2 = new FileInfo(files2[file]);
                            Int64 f1ts = GetTimeStamp(fi1.LastWriteTime);
                            Int64 f2ts = GetTimeStamp(fi2.LastWriteTime);
                            //Console.WriteLine(f1ts + " " + f2ts);
                            if (f1ts > f2ts)
                                File.Copy(files1[file], files2[file], true);
                            else if (f1ts < f2ts)
                                File.Copy(files2[file], files1[file], true);
                        }
                    }


                    //Folder Actions
                    foreach (string folder in tmp3.Keys)
                    {
                        if (!folders1.ContainsKey(folder))
                        {
                            DirectoryInfo di = new DirectoryInfo(folders2[folder]);
                            di.Delete(true);
                            folders2.Remove(folder);
                        }
                    }

                    foreach (string folder in tmp4.Keys)
                    {
                        if (!folders2.ContainsKey(folder))
                        {
                            DirectoryInfo di = new DirectoryInfo(folders1[folder]);
                            di.Delete(true);
                            folders1.Remove(folder);
                        }
                    }

                    foreach (string folder in folders2.Keys)
                    {
                        if (!folders1.ContainsKey(folder))
                        {
                            Directory.CreateDirectory(Path.Combine(f1, folder));
                            //CopyDirectoryWithIncludedFiles(folders2[folder], f1);
                            folders1.Add(folder, folders2[folder]);
                            SyncFolderObject s = new SyncFolderObject();
                            s.f1 = Path.Combine(f1, folder);
                            s.f2 = Path.Combine(f2, folder);
                            s.Interval = this.Interval;
                            s.Start();
                            subsyncs.Add(s.f1, s);
                        }
                    }

                    foreach (string folder in folders1.Keys)
                    {

                        if (!folders2.ContainsKey(folder))
                        {
                            Directory.CreateDirectory(Path.Combine(f2,folder));
                            //CopyDirectoryWithIncludedFiles(folders1[folder], f2);
                            folders2.Add(folder, folders1[folder]);
                            SyncFolderObject s = new SyncFolderObject();
                            s.f1 = Path.Combine(f1, folder);
                            s.f2 = Path.Combine(f2, folder);
                            s.Interval = this.Interval;
                            s.Start();
                            subsyncs.Add(s.f1, s);
                        }
                        /*else
                        {
                            DirectoryInfo di1 = new DirectoryInfo(folders1[folder]);
                            DirectoryInfo di2 = new DirectoryInfo(folders2[folder]);
                            Int64 f1ts = GetTimeStamp(di1.LastWriteTime);
                            Int64 f2ts = GetTimeStamp(di2.LastWriteTime);
                            //Console.WriteLine(f1ts + " " + f2ts);
                            if (f1ts > f2ts)
                            {
                                //di2.Delete(true);
                                //CopyDirectoryWithIncludedFiles(folders1[folder], f2);
                                Directory.CreateDirectory(Path.Combine(f2, folder));
                            }
                            else if (f1ts < f2ts)
                            {
                                //di1.Delete(true);
                                //CopyDirectoryWithIncludedFiles(folders2[folder], f1);
                                Directory.CreateDirectory(Path.Combine(f1, folder));
                            }
                        }*/
                    }
                }
                catch (Exception ex) { }

                tmp1 = files1;
                tmp2 = files2;
                tmp3 = folders1;
                tmp4 = folders2;

                System.Threading.Thread.Sleep(this.Interval);
            }
            foreach (SyncFolderObject s in subsyncs.Values)
                s.Stop();
        }

        public void Start() {
            t = new Thread(new ThreadStart(sync));
            stopped = false;
            t.Start();
        }

        public void Stop() {
            stopped = true;
        }

        public static string GetExactTimeStamp(DateTime time)
        {
            return time.ToString("yyyyMMddHHmmssfff");
        }

        public static Int64 GetTimeStamp(DateTime time)
        {
            // HH zeigt 24-Stunden-Format, hh zeigt 12-Stunden-Format.         
            return Convert.ToInt64(time.ToString("yyyyMMddHHmmsss"));
        }

        public static void CopyDirectoryWithIncludedFiles(string dirCopySource, string dirCopyTarget)
        {
            // alle Unterverzeichnisse des aktuellen Verzeichnisses ermitteln
            string[] subDirectories = Directory.GetDirectories(dirCopySource);

            // Zielpfad erzeugen
            StringBuilder newTargetPath = new StringBuilder();
            {
                newTargetPath.Append(dirCopyTarget);
                newTargetPath.Append(dirCopySource.Substring(dirCopySource.LastIndexOf(@"\")));
            }

            // wenn aktueller Ordner nicht existiert -> ersstellen
            if (!Directory.Exists(newTargetPath.ToString()))
                Directory.CreateDirectory(newTargetPath.ToString());


            // Unterverzeichnise durchlaufen und Funktion mit dazu gehörigen Zielpfad erneut aufrufen (Rekursion)
            foreach (string subDirectory in subDirectories)
            {
                string newDirectoryPath = subDirectory;

                // wenn ''/'' an letzter Stelle dann entfernen
                if (newDirectoryPath.LastIndexOf(@"\") == (newDirectoryPath.Length - 1))
                    newDirectoryPath = newDirectoryPath.Substring(0, newDirectoryPath.Length - 1);

                // rekursiever Aufruf
                CopyDirectoryWithIncludedFiles(newDirectoryPath, newTargetPath.ToString());
            }


            // alle enthaltenden Dateien des aktuellen Verzeichnisses ermitteln
            string[] fileNames = Directory.GetFiles(dirCopySource);
            foreach (string fileSource in fileNames)
            {
                // Zielpfad + Dateiname
                StringBuilder fileTarget = new StringBuilder();
                {
                    fileTarget.Append(newTargetPath);
                    fileTarget.Append(fileSource.Substring(fileSource.LastIndexOf(@"\")));
                }

                // Datei kopieren, wenn schon vorhanden überschreiben
                File.Copy(fileSource, fileTarget.ToString(), true);
            }
        }
    }
}
