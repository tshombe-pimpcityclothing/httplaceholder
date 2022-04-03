﻿using System;
using System.Threading;
using System.Threading.Tasks;
using HttPlaceholder.Application.Exceptions;
using HttPlaceholder.Application.Requests.Queries.GetResponse;
using HttPlaceholder.Application.StubExecution;
using HttPlaceholder.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;

namespace HttPlaceholder.Application.Tests.Requests.Queries;

[TestClass]
public class GetResponseQueryHandlerFacts
{
    private readonly AutoMocker _mocker = new();

    [TestCleanup]
    public void Cleanup() => _mocker.VerifyAll();

    [TestMethod]
    public async Task Handle_ResponseNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var mockStubContext = _mocker.GetMock<IStubContext>();
        var handler = _mocker.CreateInstance<GetResponseQueryHandler>();

        var correlationId = Guid.NewGuid().ToString();
        var query = new GetResponseQuery(correlationId);

        mockStubContext
            .Setup(m => m.GetResponseAsync(correlationId))
            .ReturnsAsync((ResponseModel)null);

        // Act / Assert
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }

    [TestMethod]
    public async Task Handle_ResponseFound_ShouldReturnResponse()
    {
        // Arrange
        var mockStubContext = _mocker.GetMock<IStubContext>();
        var handler = _mocker.CreateInstance<GetResponseQueryHandler>();

        var correlationId = Guid.NewGuid().ToString();
        var query = new GetResponseQuery(correlationId);

        var expectedResult = new ResponseModel();
        mockStubContext
            .Setup(m => m.GetResponseAsync(correlationId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.AreEqual(expectedResult, result);
    }
}
