using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;

namespace Gravatar
{
    public interface IGravatarService
    {
        /// <summary>
        /// Removes all cached images.
        /// </summary>
        Task ClearCacheAsync();

        /// <summary>
        /// Configures the local cache.
        /// </summary>
        /// <param name="imageCachePath">The path to the cache folder.</param>
        /// <param name="cacheDays">The number of days to keep images in the cache.</param>
        void ConfigureCache(string imageCachePath, int cacheDays);

        /// <summary>
        /// Loads avatar either from the local cache or from the remote service.
        /// </summary>
        Task<Image> GetAvatarAsync(string email, int imageSize, DefaultImageType defaultImageType);

        /// <summary>
        /// Removes the avatar from the local cache.
        /// </summary>
        Task RemoveAvatarAsync(string email);
    }

    public class GravatarService : IGravatarService
    {
        private IImageCache _cache;
        private readonly IImageCacheFactory _imageCacheFactory;


        public GravatarService(IImageCacheFactory imageCacheFactory)
        {
            _imageCacheFactory = imageCacheFactory;
        }

        public GravatarService()
            : this(new ImageCacheFactory())
        {
        }


        /// <summary>
        /// Removes all cached images.
        /// </summary>
        public async Task ClearCacheAsync()
        {
            EnsureCacheConfigured();
            await _cache.RemoveAllAsync();
        }

        /// <summary>
        /// Configures the local cache.
        /// </summary>
        /// <param name="imageCachePath">The path to the cache folder.</param>
        /// <param name="cacheDays">The number of days to keep images in the cache.</param>
        public void ConfigureCache(string imageCachePath, int cacheDays)
        {
            _cache = _imageCacheFactory.Create(imageCachePath, cacheDays);
        }

        /// <summary>
        /// Loads avatar either from the local cache or from the remote service.
        /// </summary>
        public async Task<Image> GetAvatarAsync(string email, int imageSize, DefaultImageType defaultImageType)
        {
            EnsureCacheConfigured();
            var imageFileName = GetImageFileName(email);
            var image = await _cache.GetImageAsync(imageFileName, null);
            if (image == null)
            {
                image = await LoadFromGravatarAsync(imageFileName, email, imageSize, defaultImageType);
            }
            return image;
        }

        /// <summary>
        /// Removes the avatar from the local cache.
        /// </summary>
        public async Task RemoveAvatarAsync(string email)
        {
            EnsureCacheConfigured();
            var imageFileName = GetImageFileName(email);
            await _cache.RemoveImageAsync(imageFileName);
        }


        /// <summary>
        /// Builds a <see cref="Uri"/> corresponding to a given email address.
        /// </summary>
        /// <param name="email">The email address for which to build the <see cref="Uri"/>.</param>
        /// <param name="size">The size of the image to request.  The default is 32.</param>
        /// <param name="useHttps">Indicates whether or not the request should be performed over Secure HTTP.</param>
        /// <param name="rating">The mazimum rating of the returned image.</param>
        /// <param name="defaultImageType">The Gravatar service that will be used for fall-back.</param>
        /// <returns>The constructed <see cref="Uri"/>.</returns>
        private static Uri BuildGravatarUrl(string email, int size, bool useHttps, Rating rating, DefaultImageType defaultImageType)
        {
            var builder = new UriBuilder("http://www.gravatar.com/avatar/");
            if (useHttps)
            {
                builder.Scheme = "https";
            }
            builder.Path += HashEmail(email);

            var query = string.Format("s={0}&r={1}&d={2}",
                size,
                rating.ToString().ToLowerInvariant(),
                GetDefaultImageString(defaultImageType));

            builder.Query = query;

            return builder.Uri;
        }

        private void EnsureCacheConfigured()
        {
            if (_cache != null)
            {
                return;
            }
            throw new NotSupportedException($"{nameof(ConfigureCache)} must be called first");
        }

        /// <summary>
        /// Provides a mapping for the image defaults.
        /// </summary>
        private static string GetDefaultImageString(DefaultImageType defaultImageType)
        {
            switch (defaultImageType)
            {
                case DefaultImageType.Identicon: return "identicon";
                case DefaultImageType.MonsterId: return "monsterid";
                case DefaultImageType.Wavatar: return "wavatar";
                case DefaultImageType.Retro: return "retro";
                default: return "404";
            }
        }

        private static string GetImageFileName(string email)
        {
            return $"{email}.png";
        }

        /// <summary>
        /// Generates an email hash as per the Gravatar specifications.
        /// </summary>
        /// <param name="email">The email to hash.</param>
        /// <returns>The hash of the email.</returns>
        /// <remarks>
        /// The process of creating the hash are specified at http://en.gravatar.com/site/implement/hash/
        /// </remarks>
        private static string HashEmail(string email)
        {
            return MD5.CalcMD5(email.Trim().ToLowerInvariant());
        }

        private async Task<Image> LoadFromGravatarAsync(string imageFileName, string email, int imageSize, DefaultImageType defaultImageType)
        {
            try
            {
                var imageUrl = BuildGravatarUrl(email, imageSize, false, Rating.G, defaultImageType);
                using (var webClient = new WebClient { Proxy = WebRequest.DefaultWebProxy })
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    using (var imageStream = await webClient.OpenReadTaskAsync(imageUrl))
                    {
                        await _cache.AddImageAsync(imageFileName, imageStream);
                    }
                    return await _cache.GetImageAsync(imageFileName, null);
                }
            }
            catch (Exception ex)
            {
                //catch IO errors
                Trace.WriteLine(ex.Message);
            }
            return null;
        }

    }
}