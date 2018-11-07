﻿using Ducode.Essentials.Mvc.Interfaces;
using HttPlaceholder.BusinessLogic.Implementations.ConditionCheckers;
using HttPlaceholder.Models;
using HttPlaceholder.Models.Enums;
using HttPlaceholder.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HttPlaceholder.BusinessLogic.Tests.Implementations.ConditionCheckers
{
    [TestClass]
    public class BodyConditionCheckerFacts
    {
        private Mock<IHttpContextService> _httpContextServiceMock;
        private BodyConditionChecker _checker;

        [TestInitialize]
        public void Initialize()
        {
            _httpContextServiceMock = new Mock<IHttpContextService>();
            _checker = new BodyConditionChecker(
               _httpContextServiceMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _httpContextServiceMock.VerifyAll();
        }

        [TestMethod]
        public void BodyConditionChecker_Validate_StubsFound_ButNoBodyConditions_ShouldReturnNotExecuted()
        {
            // arrange
            var conditions = new StubConditionsModel
            {
                Body = null
            };

            // act
            var result = _checker.Validate("id", conditions);

            // assert
            Assert.AreEqual(ConditionValidationType.NotExecuted, result.ConditionValidation);
        }

        [TestMethod]
        public void BodyConditionChecker_Validate_StubsFound_AllBodyConditionsIncorrect_ShouldReturnInvalid()
        {
            // arrange
            string body = "this is a test";
            var conditions = new StubConditionsModel
            {
                Body = new[]
                  {
                  @"\bthat\b",
                  @"\btree\b"
               }
            };

            _httpContextServiceMock
               .Setup(m => m.GetBody())
               .Returns(body);

            // act
            var result = _checker.Validate("id", conditions);

            // assert
            Assert.AreEqual(ConditionValidationType.Invalid, result.ConditionValidation);
        }

        [TestMethod]
        public void BodyConditionChecker_Validate_StubsFound_OnlyOneBodyConditionCorrect_ShouldReturnInvalid()
        {
            // arrange
            string body = "this is a test";
            var conditions = new StubConditionsModel
            {
                Body = new[]
                  {
                  @"\bthis\b",
                  @"\btree\b"
               }
            };

            _httpContextServiceMock
               .Setup(m => m.GetBody())
               .Returns(body);

            // act
            var result = _checker.Validate("id", conditions);

            // assert
            Assert.AreEqual(ConditionValidationType.Invalid, result.ConditionValidation);
        }

        [TestMethod]
        public void BodyConditionChecker_Validate_StubsFound_HappyFlow_FullText()
        {
            // arrange
            string body = "this is a test";
            var conditions = new StubConditionsModel
            {
                Body = new[]
                  {
                  "this is a test"
               }
            };

            _httpContextServiceMock
               .Setup(m => m.GetBody())
               .Returns(body);

            // act
            var result = _checker.Validate("id", conditions);

            // assert
            Assert.AreEqual(ConditionValidationType.Valid, result.ConditionValidation);
        }

        [TestMethod]
        public void BodyConditionChecker_Validate_StubsFound_HappyFlow_Regex()
        {
            // arrange
            string body = "this is a test";
            var conditions = new StubConditionsModel
            {
                Body = new[]
                  {
                  @"\bthis\b",
                  @"\btest\b"
               }
            };

            _httpContextServiceMock
               .Setup(m => m.GetBody())
               .Returns(body);

            // act
            var result = _checker.Validate("id", conditions);

            // assert
            Assert.AreEqual(ConditionValidationType.Valid, result.ConditionValidation);
        }
    }
}