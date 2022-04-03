﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttPlaceholder.Application.Configuration;
using HttPlaceholder.Application.Interfaces.Persistence;
using HttPlaceholder.Domain;
using Microsoft.Extensions.Options;

namespace HttPlaceholder.Persistence.Implementations.StubSources;

/// <summary>
/// A stub source that is used to store and read data from memory.
/// </summary>
internal class InMemoryStubSource : IWritableStubSource
{
    private static readonly object _lock = new();

    private readonly SettingsModel _settings;

    internal readonly IList<RequestResultModel> RequestResultModels = new List<RequestResultModel>();
    internal readonly IList<StubModel> StubModels = new List<StubModel>();
    internal readonly IList<ResponseModel> StubResponses = new List<ResponseModel>();

    internal readonly IDictionary<RequestResultModel, ResponseModel> RequestResponseMap =
        new Dictionary<RequestResultModel, ResponseModel>();

    public InMemoryStubSource(IOptions<SettingsModel> options)
    {
        _settings = options.Value;
    }

    /// <inheritdoc />
    public Task AddRequestResultAsync(RequestResultModel requestResult, ResponseModel responseModel)
    {
        lock (_lock)
        {
            if (responseModel != null)
            {
                requestResult.HasResponse = true;
                StubResponses.Add(responseModel);
                RequestResponseMap.Add(requestResult, responseModel);
            }

            RequestResultModels.Add(requestResult);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task AddStubAsync(StubModel stub)
    {
        lock (_lock)
        {
            StubModels.Add(stub);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RequestOverviewModel>> GetRequestResultsOverviewAsync()
    {
        var requests = await GetRequestResultsAsync();
        return requests.Select(r => new RequestOverviewModel
        {
            Method = r.RequestParameters?.Method,
            Url = r.RequestParameters?.Url,
            CorrelationId = r.CorrelationId,
            StubTenant = r.StubTenant,
            ExecutingStubId = r.ExecutingStubId,
            RequestBeginTime = r.RequestBeginTime,
            RequestEndTime = r.RequestEndTime,
            HasResponse = r.HasResponse
        }).ToArray();
    }

    /// <inheritdoc />
    public Task<RequestResultModel> GetRequestAsync(string correlationId)
    {
        lock (_lock)
        {
            return Task.FromResult(GetRequest(correlationId));
        }
    }

    /// <inheritdoc />
    public Task<ResponseModel> GetResponseAsync(string correlationId)
    {
        lock (_lock)
        {
            var nullValue = Task.FromResult((ResponseModel)null);
            var request = GetRequest(correlationId);
            if (request == null)
            {
                return nullValue;
            }

            return !RequestResponseMap.ContainsKey(request) ? nullValue : Task.FromResult(RequestResponseMap[request]);
        }
    }

    /// <inheritdoc />
    public Task DeleteAllRequestResultsAsync()
    {
        lock (_lock)
        {
            RequestResultModels.Clear();
            StubResponses.Clear();
            RequestResponseMap.Clear();
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteRequestAsync(string correlationId)
    {
        lock (_lock)
        {
            var request = RequestResultModels.FirstOrDefault(r => r.CorrelationId == correlationId);
            if (request == null)
            {
                return Task.FromResult(false);
            }

            RequestResultModels.Remove(request);
            RemoveResponse(request);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteStubAsync(string stubId)
    {
        lock (_lock)
        {
            var stub = StubModels.FirstOrDefault(s => s.Id == stubId);
            if (stub == null)
            {
                return Task.FromResult(false);
            }

            StubModels.Remove(stub);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<RequestResultModel>> GetRequestResultsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(RequestResultModels.AsEnumerable());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<StubModel>> GetStubsAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(StubModels.AsEnumerable());
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<StubOverviewModel>> GetStubsOverviewAsync() =>
        (await GetStubsAsync())
        .Select(s => new StubOverviewModel {Id = s.Id, Tenant = s.Tenant, Enabled = s.Enabled})
        .ToArray();

    /// <inheritdoc />
    public Task<StubModel> GetStubAsync(string stubId) =>
        Task.FromResult(StubModels.FirstOrDefault(s => s.Id == stubId));

    /// <inheritdoc />
    public Task CleanOldRequestResultsAsync()
    {
        lock (_lock)
        {
            var maxLength = _settings.Storage?.OldRequestsQueueLength ?? 40;
            var requests = RequestResultModels
                .OrderByDescending(r => r.RequestEndTime)
                .Skip(maxLength);
            foreach (var request in requests)
            {
                RequestResultModels.Remove(request);
                RemoveResponse(request);
            }

            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task PrepareStubSourceAsync() => Task.CompletedTask;

    private void RemoveResponse(RequestResultModel request)
    {
        if (RequestResponseMap.ContainsKey(request))
        {
            var response = RequestResponseMap[request];
            StubResponses.Remove(response);
            RequestResponseMap.Remove(request);
        }
    }

    private RequestResultModel GetRequest(string correlationId) =>
        RequestResultModels.FirstOrDefault(r => r.CorrelationId == correlationId);
}
