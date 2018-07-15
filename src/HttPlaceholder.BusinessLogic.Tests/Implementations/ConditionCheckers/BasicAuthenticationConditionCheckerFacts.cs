﻿using System.Collections.Generic;
using HttPlaceholder.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HttPlaceholder.BusinessLogic.Implementations.ConditionCheckers;
using HttPlaceholder.BusinessLogic.Tests.Utilities;
using HttPlaceholder.Models;
using HttPlaceholder.Models.Enums;

namespace HttPlaceholder.BusinessLogic.Tests.Implementations.ConditionCheckers
{
   [TestClass]
   public class BasicAuthenticationConditionCheckerFacts
   {
      private Mock<IHttpContextService> _httpContextServiceMock;
      private BasicAuthenticationConditionChecker _checker;

      [TestInitialize]
      public void Initialize()
      {
         _httpContextServiceMock = new Mock<IHttpContextService>();
         _checker = new BasicAuthenticationConditionChecker(
            _httpContextServiceMock.Object);
      }

      [TestCleanup]
      public void Cleanup()
      {
         _httpContextServiceMock.VerifyAll();
      }

      [TestMethod]
      public void BasicAuthenticationConditionChecker_Validate_StubsFound_ButNoBasicAuthenticationCondition_ShouldReturnNotExecuted()
      {
         // arrange
         var conditions = new StubConditionsModel
         {
            BasicAuthentication = null
         };

         // act
         var result = _checker.Validate("id", conditions);

         // assert
         Assert.AreEqual(ConditionValidationType.NotExecuted, result.ConditionValidation);
      }

      [TestMethod]
      public void BasicAuthenticationConditionChecker_Validate_NoAuthorizationHeader_ShouldReturnInvalid()
      {
         // arrange
         var conditions = new StubConditionsModel
         {
            BasicAuthentication = new StubBasicAuthenticationModel
            {
               Username = "username",
               Password = "password"
            }
         };

         var headers = new Dictionary<string, string>
         {
            { "X-Api-Key", "1" }
         };

         _httpContextServiceMock
            .Setup(m => m.GetHeaders())
            .Returns(headers);

         // act
         var result = _checker.Validate("id", conditions);

         // assert
         Assert.AreEqual(ConditionValidationType.Invalid, result.ConditionValidation);
      }

      [TestMethod]
      public void BasicAuthenticationConditionChecker_Validate_BasicAuthenticationIncorrect_ShouldReturnInvalid()
      {
         // arrange
         var conditions = new StubConditionsModel
         {
            BasicAuthentication = new StubBasicAuthenticationModel
            {
               Username = "username",
               Password = "password"
            }
         };

         var headers = new Dictionary<string, string>
         {
            { "Authorization", "Basic dXNlcm5hbWU6cGFzc3dvcmRk" }
         };

         _httpContextServiceMock
            .Setup(m => m.GetHeaders())
            .Returns(headers);

         // act
         var result = _checker.Validate("id", conditions);

         // assert
         Assert.AreEqual(ConditionValidationType.Invalid, result.ConditionValidation);
      }

      [TestMethod]
      public void BasicAuthenticationConditionChecker_Validate_HappyFlow()
      {
         // arrange
         var conditions = new StubConditionsModel
         {
            BasicAuthentication = new StubBasicAuthenticationModel
            {
               Username = "username",
               Password = "password"
            }
         };

         var headers = new Dictionary<string, string>
         {
            { "Authorization", "Basic dXNlcm5hbWU6cGFzc3dvcmQ=" }
         };

         _httpContextServiceMock
            .Setup(m => m.GetHeaders())
            .Returns(headers);

         // act
         var result = _checker.Validate("id", conditions);

         // assert
         Assert.AreEqual(ConditionValidationType.Valid, result.ConditionValidation);
      }
   }
}