﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Placeholder.Implementation.Implementations.ConditionCheckers;
using Placeholder.Implementation.Services;
using Placeholder.Models;
using Placeholder.Models.Enums;

namespace Placeholder.Implementation.Tests.Implementations.ConditionCheckers
{
   [TestClass]
   public class JsonPathConditionCheckerFacts
   {
      private Mock<ILogger<JsonPathConditionChecker>> _loggerMock;
      private Mock<IHttpContextService> _httpContextServiceMock;
      private JsonPathConditionChecker _checker;

      [TestInitialize]
      public void Initialize()
      {
         _loggerMock = new Mock<ILogger<JsonPathConditionChecker>>();
         _httpContextServiceMock = new Mock<IHttpContextService>();
         _checker = new JsonPathConditionChecker(
            _loggerMock.Object,
            _httpContextServiceMock.Object);
      }

      [TestCleanup]
      public void Cleanup()
      {
         _httpContextServiceMock.VerifyAll();
      }

      [TestMethod]
      public void JsonPathConditionChecker_ValidateAsync_StubsFound_ButNoJsonPathConditions_ShouldReturnNotExecuted()
      {
         // arrange
         var stub = new StubModel
         {
            Conditions = new StubConditionsModel
            {
               JsonPath = null
            }
         };

         // act
         var result = _checker.Validate(stub);

         // assert
         Assert.AreEqual(ConditionValidationType.NotExecuted, result);
      }

      [TestMethod]
      public void JsonPathConditionChecker_ValidateAsync_StubsFound_AllXPathConditionsIncorrect_ShouldReturnInvalid()
      {
         // arrange
         string body = @"{
  ""firstName"": ""John"",
  ""lastName"" : ""doe"",
  ""age""      : 26,
  ""address""  : {
    ""streetAddress"": ""naist street"",
    ""city""         : ""Nara"",
    ""postalCode""   : ""630-0192""
  },
  ""phoneNumbers"": [
    {
      ""type""  : ""iPhone"",
      ""number"": ""0123-4567-8888""
    },
    {
      ""type""  : ""home"",
      ""number"": ""0123-4567-8910""
    }
  ]
}";
         var stub = new StubModel
         {
            Conditions = new StubConditionsModel
            {
               JsonPath = new[]
               {
                  "$.phoneNumbers[?(@.type=='Android')]",
                  "$.phoneNumbers[?(@.type=='Office')]"
               }
            }
         };

         _httpContextServiceMock
            .Setup(m => m.GetBody())
            .Returns(body);

         // act
         var result = _checker.Validate(stub);

         // assert
         Assert.AreEqual(ConditionValidationType.Invalid, result);
      }

      [TestMethod]
      public void JsonPathConditionChecker_ValidateAsync_StubsFound_OnlyOneJsonPathConditionCorrect_ShouldReturnInvalid()
      {
         // arrange
         string body = @"{
  ""firstName"": ""John"",
  ""lastName"" : ""doe"",
  ""age""      : 26,
  ""address""  : {
    ""streetAddress"": ""naist street"",
    ""city""         : ""Nara"",
    ""postalCode""   : ""630-0192""
  },
  ""phoneNumbers"": [
    {
      ""type""  : ""iPhone"",
      ""number"": ""0123-4567-8888""
    },
    {
      ""type""  : ""home"",
      ""number"": ""0123-4567-8910""
    }
  ]
}";
         var stub = new StubModel
         {
            Conditions = new StubConditionsModel
            {
               JsonPath = new[]
               {
                  "$.phoneNumbers[?(@.type=='iPhone')]",
                  "$.phoneNumbers[?(@.type=='Office')]"
               }
            }
         };

         _httpContextServiceMock
            .Setup(m => m.GetBody())
            .Returns(body);

         // act
         var result = _checker.Validate(stub);

         // assert
         Assert.AreEqual(ConditionValidationType.Invalid, result);
      }

      [TestMethod]
      public void JsonPathConditionChecker_ValidateAsync_StubsFound_HappyFlow_WithNamespaces()
      {
         // arrange
         string body = @"{
  ""firstName"": ""John"",
  ""lastName"" : ""doe"",
  ""age""      : 26,
  ""address""  : {
    ""streetAddress"": ""naist street"",
    ""city""         : ""Nara"",
    ""postalCode""   : ""630-0192""
  },
  ""phoneNumbers"": [
    {
      ""type""  : ""iPhone"",
      ""number"": ""0123-4567-8888""
    },
    {
      ""type""  : ""home"",
      ""number"": ""0123-4567-8910""
    }
  ]
}";
         var stub = new StubModel
         {
            Conditions = new StubConditionsModel
            {
               JsonPath = new[]
               {
                  "$.phoneNumbers[?(@.type=='iPhone')]",
                  "$.phoneNumbers[?(@.type=='home')]"
               }
            }
         };

         _httpContextServiceMock
            .Setup(m => m.GetBody())
            .Returns(body);

         // act
         var result = _checker.Validate(stub);

         // assert
         Assert.AreEqual(ConditionValidationType.Valid, result);
      }
   }
}