﻿using System;
using System.Threading;
using System.Threading.Tasks;
using HttPlaceholder.Application.Infrastructure.DependencyInjection;
using HttPlaceholder.Application.StubExecution.Utilities;
using HttPlaceholder.Common;
using HttPlaceholder.Domain;
using static HttPlaceholder.Domain.StubResponseWriterResultModel;

namespace HttPlaceholder.Application.StubExecution.ResponseWriters;

/// <summary>
///     Response writer that is used to add an extra duration to the total execution time of the request.
/// </summary>
internal class ExtraDurationResponseWriter(IAsyncService asyncService) : IResponseWriter, ISingletonService
{
    private static readonly Random _random = new();

    /// <inheritdoc />
    public int Priority => 0;

    /// <inheritdoc />
    public async Task<StubResponseWriterResultModel> WriteToResponseAsync(StubModel stub, ResponseModel response,
        CancellationToken cancellationToken)
    {
        // Simulate sluggish response here, if configured.
        if (stub.Response?.ExtraDuration == null)
        {
            return IsNotExecuted(GetType().Name);
        }

        var duration = ConversionUtilities.ParseInteger(stub.Response.ExtraDuration) ?? GetDurationInRange(stub);
        await asyncService.DelayAsync(duration, cancellationToken);
        return IsExecuted(GetType().Name);
    }

    private static int GetDurationInRange(StubModel stub)
    {
        var durationDto = ConversionUtilities.Convert<StubExtraDurationModel>(stub.Response.ExtraDuration);
        var min = durationDto?.Min ?? 0;
        var max = durationDto?.Max ?? (durationDto?.Min ?? 0) + 10000;
        return _random.Next(min, max);
    }
}
