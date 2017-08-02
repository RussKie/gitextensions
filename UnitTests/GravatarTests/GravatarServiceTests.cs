using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Gravatar;
using GravatarTests.Properties;
using NSubstitute;
using NUnit.Framework;

namespace GravatarTests
{
    [TestFixture]
    public class GravatarServiceTests
    {
        private const string Email = "x@x.com";
        private IImageCache _cache;
        private IImageCacheFactory _imageCacheFactory;
        private GravatarService _service;

        [SetUp]
        public void Setup()
        {
            _cache = Substitute.For<IImageCache>();
            _imageCacheFactory = Substitute.For<IImageCacheFactory>();
            _imageCacheFactory.Create(Arg.Any<string>(), Arg.Any<int>()).Returns(x => _cache);

            _service = new GravatarService(_imageCacheFactory);
        }

        [Test]
        public void ClearCacheAsync_must_throw_if_cache_not_configured()
        {
            Func<Task> act = async () =>
            {
                await _service.ClearCacheAsync();
            };
            act.ShouldThrow<NotSupportedException>()
                .WithMessage("ConfigureCache must be called first");
        }

        [Test]
        public async void ClearCacheAsync_should_invoke_cache_clear()
        {
            _service.ConfigureCache("", 1);

            await _service.ClearCacheAsync();

           Received.InOrder(async () =>
           {
               await _cache.Received(1).RemoveAllAsync();
           });
        }

        [Test]
        public void GetAvatarAsync_must_throw_if_cache_not_configured()
        {
            Func<Task> act = async () =>
            {
                await _service.GetAvatarAsync(Email, 80, DefaultImageType.Identicon);
            };
            act.ShouldThrow<NotSupportedException>()
                .WithMessage("ConfigureCache must be called first");
        }

        [Test]
        public async void GetAvatarAsync_should_not_call_gravatar_if_exist_in_cache()
        {
            _service.ConfigureCache("", 1);
            var avatar = Resources.User;
            _cache.GetImageAsync(Arg.Any<string>(), null).Returns(avatar);

            var image = await _service.GetAvatarAsync(Email, 1, DefaultImageType.Identicon);

            image.Should().Be(avatar);
            Received.InOrder(async () =>
            {
                await _cache.Received(1).GetImageAsync($"{Email}.png", null);
            });
            await _cache.DidNotReceive().AddImageAsync(Arg.Any<string>(), Arg.Any<Stream>());
        }

        [Ignore("Need to abstract WebClient or replace with HttpClient")]
        public void GetAvatarAsync_should_call_gravatar_if_absent_from_cache()
        {
            //_service.ConfigureCache("", 1);
            //var avatar = Resources.User;
            //_cache.GetImageAsync(Arg.Any<string>(), null).Returns(_ => null, _ => avatar);

            //var image = await _service.GetAvatarAsync(Email, 1, DefaultImageType.Identicon);

            //image.Should().Be(avatar);
            //Received.InOrder(async () =>
            //{
            //    await _cache.Received(1).GetImageAsync($"{Email}.png", null);
            //    await _cache.Received(1).AddImageAsync($"{Email}.png", Arg.Any<Stream>());
            //    await _cache.Received(1).GetImageAsync($"{Email}.png", null);
            //});
        }

        [Test]
        public void RemoveAvatarAsync_must_throw_if_cache_not_configured()
        {
            Func<Task> act = async () =>
            {
                await _service.RemoveAvatarAsync(Email);
            };
            act.ShouldThrow<NotSupportedException>()
                .WithMessage("ConfigureCache must be called first");
        }

        [Test]
        public async void RemoveAvatarAsync_should_invoke_cache_remove()
        {
            _service.ConfigureCache("", 1);

            await _service.RemoveAvatarAsync(Email);

            Received.InOrder(async () =>
            {
                await _cache.Received(1).RemoveImageAsync($"{Email}.png");
            });
        }
    }
}
