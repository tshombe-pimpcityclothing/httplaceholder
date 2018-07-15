﻿using System.Threading.Tasks;
using HttPlaceholder.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HttPlaceholder.BusinessLogic.Implementations.ResponseWriters;
using HttPlaceholder.BusinessLogic.Tests.Utilities;
using HttPlaceholder.Models;

namespace HttPlaceholder.BusinessLogic.Tests.Implementations.ResponseWriters
{
   [TestClass]
   public class ExtraDurationResponseWriterFacts
   {
      private Mock<IAsyncService> _asyncServiceMock;
      private ExtraDurationResponseWriter _writer;

      [TestInitialize]
      public void Initialize()
      {
         _asyncServiceMock = new Mock<IAsyncService>();
         _writer = new ExtraDurationResponseWriter(
            _asyncServiceMock.Object);
      }

      [TestCleanup]
      public void Cleanup()
      {
         _asyncServiceMock.VerifyAll();
      }

      [TestMethod]
      public async Task ExtraDurationResponseWriter_WriteToResponseAsync_HappyFlow_NoValueSetInStub()
      {
         // arrange
         var stub = new StubModel
         {
            Response = new StubResponseModel
            {
               ExtraDuration = null
            }
         };

         var response = new ResponseModel();

         // act
         bool result = await _writer.WriteToResponseAsync(stub, response);

         // assert
         Assert.IsFalse(result);
         _asyncServiceMock.Verify(m => m.DelayAsync(It.IsAny<int>()), Times.Never);
      }

      [TestMethod]
      public async Task ExtraDurationResponseWriter_WriteToResponseAsync_HappyFlow()
      {
         // arrange
         var stub = new StubModel
         {
            Response = new StubResponseModel
            {
               ExtraDuration = 10
            }
         };

         var response = new ResponseModel();

         // act
         bool result = await _writer.WriteToResponseAsync(stub, response);

         // assert
         Assert.IsTrue(result);
         _asyncServiceMock.Verify(m => m.DelayAsync(stub.Response.ExtraDuration.Value), Times.Once);
      }
   }
}