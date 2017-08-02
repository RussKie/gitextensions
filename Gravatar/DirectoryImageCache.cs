using System;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Gravatar
{
    public interface IImageCache
    {
        Task AddImageAsync(string imageFileName, Stream imageStream);
        Task<Image> GetImageAsync(string imageFileName, Bitmap defaultBitmap);
        Task RemoveAllAsync();
        Task RemoveImageAsync(string imageFileName);
    }

    public class DirectoryImageCache : IImageCache
    {
        private const int DefaultCacheDays = 30;
        private readonly string _cachePath;
        private readonly int _cacheDays;
        private readonly IFileSystem _fileSystem;


        public DirectoryImageCache(string cachePath, int cacheDays, IFileSystem fileSystem)
        {
            _cachePath = cachePath;
            _fileSystem = fileSystem;
            _cacheDays = cacheDays;
            if (_cacheDays < 1)
            {
                _cacheDays = DefaultCacheDays;
            }
        }

        public DirectoryImageCache(string cachePath, int cacheDays)
            : this(cachePath, cacheDays, new FileSystem())
        {
        }


        public async Task AddImageAsync(string imageFileName, Stream imageStream)
        {
            if (!_fileSystem.Directory.Exists(_cachePath))
            {
                _fileSystem.Directory.CreateDirectory(_cachePath);
            }

            try
            {
                string file = Path.Combine(_cachePath, imageFileName);
                using (var output = new FileStream(file, FileMode.Create))
                {
                    byte[] buffer = new byte[1024];
                    int read;

                    if (imageStream == null)
                    {
                        return;
                    }
                    while ((read = await imageStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                }
            }
            catch
            {
                // do nothing
            }
        }

        public async Task<Image> GetImageAsync(string imageFileName, Bitmap defaultBitmap)
        {
            string file = Path.Combine(_cachePath, imageFileName);
            try
            {
                if (HasExpired(file))
                {
                    return null;
                }
                return await Task.Run(() =>
                {
                    using (Stream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        return Image.FromStream(fileStream);
                    }
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task RemoveAllAsync()
        {
            if (!_fileSystem.Directory.Exists(_cachePath))
            {
                return;
            }
            foreach (var file in _fileSystem.Directory.GetFiles(_cachePath))
            {
                try
                {
                    await Task.Run(() => _fileSystem.File.Delete(file));
                }
                catch
                {
                    // do nothing
                }
            }
        }

        public async Task RemoveImageAsync(string imageFileName)
        {
            string file = Path.Combine(_cachePath, imageFileName);
            if (!_fileSystem.File.Exists(file))
            {
                return;
            }
            try
            {
                await Task.Run(() => _fileSystem.File.Delete(file));
            }
            catch
            {
                // do nothing
            }
        }


        private bool HasExpired(string fileName)
        {
            var file = _fileSystem.FileInfo.FromFileName(fileName);
            if (!file.Exists)
            {
                return true;
            }
            return file.LastWriteTime < DateTime.Now.AddDays(-_cacheDays);
        }
    }
}
