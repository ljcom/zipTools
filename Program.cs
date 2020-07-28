using System;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;

namespace zipTools
{
    class Program
    {
        static void Main(string[] args)
        {
            String err = "";
            if (args.Length == 0)
            {
                Console.WriteLine("Please choose function mode: add, remove, extract");
            }
            else if (args[0].ToLower() == "add")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Please complete output zip filename and input filenames or folders");
                }
                else
                {
                    String zFile = args[1];
                    for (int i = 2; i < args.Length; i++)
                    {

                        String fPath = args[i];
                        CheckEntryAtZip(fPath, zFile, "");
                    }
                }
            }
            else if (args[0].ToLower() == "extract")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Please complete existing filename and output folder");
                }
                else
                {
                    var zipFilePath = args[1];
                    var folderPath = args[2];
                    using (Package package = ZipPackage.Open(zipFilePath, FileMode.Open, FileAccess.Read))
                    {
                        foreach (PackagePart part in package.GetParts())
                        {
                            var target = Path.GetFullPath(Path.Combine(folderPath, part.Uri.OriginalString.TrimStart('/')));
                            var targetDir = target.Remove(target.LastIndexOf('\\'));

                            if (!Directory.Exists(targetDir))
                                Directory.CreateDirectory(targetDir);

                            using (Stream source = part.GetStream(FileMode.Open, FileAccess.Read))
                            {
                                source.CopyTo(File.OpenWrite(target));
                            }
                        }
                    }
                }
            }
        }

        static void CheckEntryAtZip(String fPath, String zFile, String startEntry)
        {
            if (File.Exists(fPath))
            {
                AddFileAtZip(fPath, zFile, startEntry);
            }
            else if (Directory.Exists(fPath))
            {
                int n = fPath.LastIndexOf("\\");
                String FolderName = fPath.Substring(n + 1, fPath.Length - n - 1);
                AddFolderAtZip(fPath, zFile, startEntry + (startEntry == "" ? "" : "/") + FolderName); ;
            }
            else Console.WriteLine(fPath + " is not exists. Skipped.");

        }
        static void AddFileAtZip(String fPath, String zFile, String startEntry)
        {
            using (ZipArchive archive = ZipFile.Open(zFile, ZipArchiveMode.Update))
            {
                for (int j = 0; j < archive.Entries.Count; j++)
                {
                    if (startEntry + Path.GetFileName(fPath) == startEntry + archive.Entries[j].Name)
                        archive.Entries[j].Delete();
                }
                archive.CreateEntryFromFile(fPath, startEntry + Path.GetFileName(fPath));
            }
        }
        static void AddFolderAtZip(String fPath, String zFile, String startEntry)
        {
            bool isExist = false;
            using (ZipArchive archive = ZipFile.Open(zFile, ZipArchiveMode.Update))
            {
                for (int j = 0; j < archive.Entries.Count; j++)
                {
                    if (Path.GetFileName(fPath)+"/" == archive.Entries[j].FullName)
                    {
                        isExist = true;
                        break;
                    }
                }
                if (!isExist)
                    archive.CreateEntry(startEntry+"/");
            }

            for (int i = 0; i < Directory.GetFiles(fPath).Length; i++)
            {
                String newPath = Directory.GetFiles(fPath)[i];
                AddFileAtZip(newPath, zFile, startEntry + "/" +Path.GetFileName(newPath));
            }
            for (int i = 0; i < Directory.GetDirectories(fPath).Length; i++)
            {
                String newPath = Directory.GetDirectories(fPath)[i];
                AddFolderAtZip(newPath, zFile, startEntry + "/" + Path.GetFileName(newPath));
            }


        }
    }
}
