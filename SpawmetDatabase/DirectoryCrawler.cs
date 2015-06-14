using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Word;

namespace SpawmetDatabase
{
    public class DirectoryCrawler : IDisposable
    {
        public event EventHandler<string> DirectoryReached;
        public event EventHandler<string> FileReached;

        public string Path { get; set; }

        private FileStream _file;
        private StreamWriter _writer;

        public DirectoryCrawler()
            : this("")
        {
        }

        public DirectoryCrawler(string path)
        {
            Path = path;

            //_file = new FileStream(@"D:\directories.txt", FileMode.Create, FileAccess.ReadWrite);
            //_writer = new StreamWriter(_file);
        }

        public void Dispose()
        {
            //_writer.Close();
            //_writer.Dispose();
            //_file.Dispose();
        }

        ///// <summary>
        ///// Gets paths of all files from start directory and its subdirectories.
        ///// </summary>
        ///// <param name="startDir">Start directory.</param>
        ///// <returns></returns>
        //public List<string> GetFilesFromDirectories(string startDir)
        //{
        //    var files = new List<string>();
        //    AddFilesToCollectionFromDirectories(files, startDir);
        //    return files;
        //}

        ///// <summary>
        ///// Add files paths to collection from start directory and its subdirectories. Recursive.
        ///// </summary>
        ///// <param name="collection">Collection to which file will be added.</param>
        ///// <param name="startDir">Start directory.</param>
        //private void AddFilesToCollectionFromDirectories(ICollection<string> collection, string startDir)
        //{
        //    string[] directories = Directory.GetDirectories(startDir); // Get subdirectories.
        //    if (directories.Length == 0)
        //    {
        //        foreach (var file in Directory.GetFiles(startDir))
        //        {
        //            collection.Add(file);
        //        }
        //    }
        //    else
        //    {
        //        foreach (var dir in directories)
        //        {
        //            AddFilesToCollectionFromDirectories(collection, dir);
        //        }
        //    }
        //}

        /// <summary>
        /// Gets directories paths
        /// </summary>
        /// <returns></returns>
        //public List<string> GetDirectories(string startDir, string name)

        //public string GetPathByDirectoryName(string rootDirPath, string dirName)
        //{

        //}

        //public void GetPathNameFromChildren(string directoryName)
        //{
        //    string[] directories = Directory.GetDirectories(directoryName);
        //    if (directories.Length == 0)
        //    {
        //        return;
        //    }

        //    foreach (var directory in directories)
        //    {
        //        GetPathNameFromChildren(directory);
        //        if (Regex.IsMatch(directory, "do wypalania", RegexOptions.IgnoreCase))
        //        {
        //            Console.WriteLine(directory);
        //            _writer.WriteLine(directory);

        //            GetFilesFromDirectories(directory);
        //        }
        //    }
        //}

        /// <summary>
        /// Gets all files from directory and its subdirectories.
        /// </summary>
        /// <param name="directory">Root directory.</param>
        //public void GetFilesFromDirectories(string directory)
        //{
        //    foreach (var file in Directory.EnumerateFiles(directory))
        //    {
        //        Console.WriteLine(file);
        //    }

        //    string[] directories = Directory.GetDirectories(directory);
        //    if (directories.Length == 0)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        foreach (var dir in directories)
        //        {
        //            GetFilesFromDirectories(dir);
        //        }
        //    }
        //}

        public IEnumerable<string> EnumerateSubdirectories(string directory)
        {
            foreach (var dir in Directory.GetDirectories(directory))
            {
                //_writer.WriteLine(dir);
                yield return dir;

                foreach (var d in EnumerateSubdirectories(dir))
                {
                    yield return d;
                }
            }
        }

        public IEnumerable<string> EnumerateSubdirectories(string directory, string word)
        {
            foreach (var dir in Directory.GetDirectories(directory))
            {
                if (Regex.IsMatch(dir, word, RegexOptions.IgnoreCase))
                {
                    //_writer.WriteLine(dir);
                    yield return dir;
                }

                foreach (var d in EnumerateSubdirectories(dir, word))
                {
                    yield return d;
                }
            }
        }

        public List<string> GetSubdirectories(string directory)
        {
            var subdirs = new List<string>();

            AddSubdirectoriesToList(directory, subdirs);

            return subdirs;
        }

        private void AddSubdirectoriesToList(string directory, List<string> directories)
        {
            var dirs = Directory.GetDirectories(directory);
            foreach (var dir in dirs)
            {
                directories.Add(dir);
                AddSubdirectoriesToList(dir, directories);
            }
        }

        /// <summary>
        /// Gets in lazy way all files paths from directory and its subdirectories. Recursive.
        /// </summary>
        /// <param name="directory">Start directory.</param>
        /// <returns></returns>
        public IEnumerable<string> EnumerateFilesFromDirectories(string directory)
        {
            OnDirectoryReached(directory);

            foreach (var file in Directory.GetFiles(directory))
            {
                OnFileReached(file);
                yield return file;
            }

            string[] directories = Directory.GetDirectories(directory);
            if (directories.Length == 0)
            {
                yield break;
            }
            else
            {
                foreach (var dir in directories)
                {
                    foreach (var file in EnumerateFilesFromDirectories(dir))
                    {
                        yield return file;
                    }
                }
            }
        }

        /// <summary>
        /// Gets in lazy way all file paths (that contains word) from directory and its subdirectories. Recursive.
        /// </summary>
        /// <param name="directory">Start directory.</param>
        /// <param name="word">Word that has to be in path.</param>
        /// <returns></returns>
        public IEnumerable<string> EnumerateFilesFromDirectories(string directory, string word)
        {
            OnDirectoryReached(directory);

            foreach (var file in Directory.GetFiles(directory))
            {
                OnFileReached(file);
                if (Regex.IsMatch(file, word, RegexOptions.IgnoreCase))
                {
                    yield return file;
                }
            }

            string[] directories = Directory.GetDirectories(directory);
            if (directories.Length == 0)
            {
                yield break;
            }
            else
            {
                foreach (var dir in directories)
                {
                    foreach (var file in EnumerateFilesFromDirectories(dir, word))
                    {
                        yield return file;
                    }
                }
            }
        }

        public List<string> GetFilesFromDirectories(string directory)
        {
            var files = new List<string>();

            AddFilesFromDirectoriesToList(directory, files);

            return files;
        }

        private void AddFilesFromDirectoriesToList(string directory, List<string> files)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                files.Add(file);
            }

            string[] directories = Directory.GetDirectories(directory);
            if (directories.Length == 0)
            {
                return;
            }
            foreach (var dir in directories)
            {
                AddFilesFromDirectoriesToList(dir, files);
            }
        }

        private void OnDirectoryReached(string dir)
        {
            if (DirectoryReached != null)
            {
                DirectoryReached(this, dir);
            }
        }

        private void OnFileReached(string file)
        {
            if (FileReached != null)
            {
                FileReached(this, file);
            }
        }

    }
}
