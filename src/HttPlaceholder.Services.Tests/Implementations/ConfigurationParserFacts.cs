﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ducode.Essentials.Assembly.Interfaces;
using Ducode.Essentials.Files.Interfaces;
using HttPlaceholder.Models;
using HttPlaceholder.Services.Implementations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HttPlaceholder.Services.Tests.Implementations
{
    [TestClass]
    public class ConfigurationParserFacts
    {
        private readonly Mock<IAssemblyService> _assemblyServiceMock = new Mock<IAssemblyService>();
        private readonly Mock<IFileService> _fileServiceMock = new Mock<IFileService>();
        private ConfigurationParser _parser;

        [TestInitialize]
        public void Initialize()
        {
            _parser = new ConfigurationParser(
                _assemblyServiceMock.Object,
                _fileServiceMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _assemblyServiceMock.VerifyAll();
            _fileServiceMock.VerifyAll();
        }

        [TestMethod]
        public void ConfigurationParser_ParseConfiguration_ArgumentsPassed_ShouldParseArguments()
        {
            // arrange
            var args = new[]
            {
                "--var1",
                "value1",
                "--var2",
                "value2"
            };

            _assemblyServiceMock
                .Setup(m => m.GetCallingAssemblyRootPath())
                .Returns(@"C:\httplaceholder");

            // act
            var result = _parser.ParseConfiguration(args);

            // assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["var1"]);
            Assert.AreEqual("value2", result["var2"]);
        }

        [TestMethod]
        public void ConfigurationParser_ParseConfiguration_ConfigFileLocationPassedAsArgument_FileNotFound_ShouldThrowFileNotFoundException()
        {
            // arrange
            string configJsonPath = @"F:\httplaceholder\config.json";
            var args = new[]
            {
                $"--{Constants.ConfigKeys.ConfigJsonLocationKey}",
                configJsonPath
            };

            _fileServiceMock
                .Setup(m => m.FileExists(configJsonPath))
                .Returns(false);

            // act / assert
            Assert.ThrowsException<FileNotFoundException>(() => _parser.ParseConfiguration(args));
        }

        [TestMethod]
        public void ConfigurationParser_ParseConfiguration_ConfigFileLocationPassedAsArgument_FileFound_ShouldParseFile()
        {
            // arrange
            string configJsonPath = @"F:\httplaceholder\config.json";
            var args = new[]
            {
                $"--{Constants.ConfigKeys.ConfigJsonLocationKey}",
                configJsonPath
            };
            string json = $@"{{
    ""var1"":""value1"",
    ""var2"":""value2""
}}";

            _fileServiceMock
                .Setup(m => m.FileExists(configJsonPath))
                .Returns(true);

            _fileServiceMock
                .Setup(m => m.ReadAllText(configJsonPath))
                .Returns(json);

            // act
            var result = _parser.ParseConfiguration(args);

            // assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["var1"]);
            Assert.AreEqual("value2", result["var2"]);
        }

        [TestMethod]
        public void ConfigurationParser_ParseConfiguration_ConfigFileFoundInInstallationFolder_ShouldParseFile()
        {
            // arrange
            string json = @"{
    ""var1"":""value1"",
    ""var2"":""value2""
}";
            string expectedPath = @"C:\httplaceholder\config.json";

            var args = new string[0];

            _assemblyServiceMock
                .Setup(m => m.GetCallingAssemblyRootPath())
                .Returns(@"C:\httplaceholder");

            _fileServiceMock
                .Setup(m => m.FileExists(expectedPath))
                .Returns(true);

            _fileServiceMock
                .Setup(m => m.ReadAllText(expectedPath))
                .Returns(json);

            // act
            var result = _parser.ParseConfiguration(args);

            // assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["var1"]);
            Assert.AreEqual("value2", result["var2"]);
        }
    }
}