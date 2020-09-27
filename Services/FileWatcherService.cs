using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConverterService.Services
{
    public class FileWatcherService : IDisposable
    {
        public delegate void FileChangedDelegate(string path);
        public event FileChangedDelegate FileAdded;
        public event FileChangedDelegate FileDeleted;
        private ILogger<FileWatcherService> Logger { get; set; }
        private string Filter { get; set; }
        private string Directory { get; set; }
        private FileSystemWatcher Watcher { get; set; }
        public FileWatcherService(string dir, string filter, ILogger<FileWatcherService> logger)
        {
            Console.WriteLine($"Watchin directory: {dir}");
            Directory = dir;
            Filter = filter;
            Logger = logger;

            var watcher = new FileSystemWatcher();
            watcher.Path = dir;

            watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            watcher.Filter = filter;

            watcher.Created += (source, e) => FileAdded?.Invoke(e.FullPath);
            watcher.Changed += (source, e) => FileAdded?.Invoke(e.FullPath);
            watcher.Renamed += (source, e) => FileAdded?.Invoke(e.FullPath);
            watcher.Deleted += (source, e) => FileDeleted?.Invoke(e.FullPath);

            watcher.EnableRaisingEvents = true;

            Watcher = watcher;
        }
        public List<string> DirectoryContents =>
            System.IO.Directory.GetFiles(Directory).Where(x => x.ToLower().EndsWith(Filter.Substring(1))).ToList();

        public void Dispose()
        {
            Watcher.EnableRaisingEvents = false;
            Watcher = null;
        }
    }
}
