using GitCommands.Gpg;
using GitUI.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public sealed class FormBrowseControllerTests
    {
        private GpgInfoProvider _controller;
        private IGitGpgController _gitGpgController;

        [SetUp]
        public void Setup()
        {
            _gitGpgController = Substitute.For<IGitGpgController>();

            _controller = new GpgInfoProvider(_gitGpgController);
        }
    }
}
