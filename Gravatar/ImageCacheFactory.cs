namespace Gravatar
{
    public interface IImageCacheFactory
    {
        IImageCache Create(string cachePath, int cacheDays);
    }

    public class ImageCacheFactory : IImageCacheFactory
    {
        public IImageCache Create(string cachePath, int cacheDays)
        {
            return new DirectoryImageCache(cachePath, cacheDays);
        }
    }
}