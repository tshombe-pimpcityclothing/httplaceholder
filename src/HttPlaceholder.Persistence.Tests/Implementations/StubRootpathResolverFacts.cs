﻿using Ducode.Essentials.Assembly.Interfaces;
using Ducode.Essentials.Files.Interfaces;
using HttPlaceholder.Configuration;
using HttPlaceholder.Persistence.Implementations;
using HttPlaceholder.TestUtilities.Options;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HttPlaceholder.Persistence.Tests.Implementations
{
    [TestClass]
    public class StubRootPathResolverFacts
    {
        private readonly IOptions<SettingsModel> _options = MockSettingsFactory.GetSettings();
        private Mock<IAssemblyService> _assemblyServiceMock;
        private Mock<IFileService> _fileServiceMock;
        private StubRootPathResolver _resolver;

        [TestInitialize]
        public void Initialize()
        {
            _assemblyServiceMock = new Mock<IAssemblyService>();
            _fileServiceMock = new Mock<IFileService>();
            _resolver = new StubRootPathResolver(
                _assemblyServiceMock.Object,
                _fileServiceMock.Object,
                _options);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _assemblyServiceMock.VerifyAll();
            _fileServiceMock.VerifyAll();
        }

        [TestMethod]
        public void StubRootPathResolver_GetStubRootPath_InputFileSet_InputFileIsDirectory_ShouldReturnInputFileAsIs()
        {
            // arrange
            var inputFile = @"C:\stubs";
            _options.Value.Storage.InputFile = inputFile;

            _fileServiceMock
               .Setup(m => m.IsDirectory(inputFile))
               .Returns(true);

            // act
            string result = _resolver.GetStubRootPath();

            // assert
            Assert.AreEqual(inputFile, result);
        }

        [TestMethod]
        public void StubRootPathResolver_GetStubRootPath_InputFileSet_InputFileIsFile_ShouldReturnInputFileFolder()
        {
            // arrange
            var inputFilePath = @"C:\stubs";
            var inputFile = $@"{inputFilePath}\stubs.yml";
            _options.Value.Storage.InputFile = inputFile;

            _fileServiceMock
               .Setup(m => m.IsDirectory(inputFile))
               .Returns(false);

            // act
            string result = _resolver.GetStubRootPath();

            // assert
            Assert.AreEqual(inputFilePath, result);
        }

        [TestMethod]
        public void StubRootPathResolver_GetStubRootPath_InputFileNotSet_ShouldReturnAssemblyPath()
        {
            // arrange
            var assemblyPath = @"C:\stubs\bin";

            _assemblyServiceMock
               .Setup(m => m.GetEntryAssemblyRootPath())
               .Returns(assemblyPath);

            // act
            string result = _resolver.GetStubRootPath();

            // assert
            Assert.AreEqual(assemblyPath, result);
        }
    }
}
