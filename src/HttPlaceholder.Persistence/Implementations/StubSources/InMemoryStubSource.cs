﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HttPlaceholder.Application.Configuration;
using HttPlaceholder.Application.StubExecution.Models;
using HttPlaceholder.Domain;
using Microsoft.Extensions.Options;

namespace HttPlaceholder.Persistence.Implementations.StubSources;

/// <summary>
///     A stub source that is used to store and read data from memory.
/// </summary>
internal class InMemoryStubSource : BaseWritableStubSource
{
    private static readonly object _lock = new();
    private readonly IOptionsMonitor<SettingsModel> _options;
    internal readonly ConcurrentDictionary<string, StubRequestCollectionItem> CollectionItems = new();

    public InMemoryStubSource(IOptionsMonitor<SettingsModel> options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public override Task AddRequestResultAsync(RequestResultModel requestResult, ResponseModel responseModel,
        string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = GetCollection(distributionKey);
            if (responseModel != null)
            {
                requestResult.HasResponse = true;
                item.StubResponses.Add(responseModel);
                item.RequestResponseMap.Add(requestResult, responseModel);
            }

            item.RequestResultModels.Add(requestResult);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public override Task AddStubAsync(StubModel stub, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = GetCollection(distributionKey);
            item.StubModels.Add(stub);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public override Task<RequestResultModel> GetRequestAsync(string correlationId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(GetRequest(correlationId, distributionKey));
        }
    }

    /// <inheritdoc />
    public override Task<ResponseModel> GetResponseAsync(string correlationId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var nullValue = Task.FromResult((ResponseModel)null);
            var request = GetRequest(correlationId, distributionKey);
            if (request == null)
            {
                return nullValue;
            }

            var item = GetCollection(distributionKey);
            return !item.RequestResponseMap.ContainsKey(request)
                ? nullValue
                : Task.FromResult(item.RequestResponseMap[request]);
        }
    }

    /// <inheritdoc />
    public override Task DeleteAllRequestResultsAsync(string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = GetCollection(distributionKey);
            item.RequestResultModels.Clear();
            item.StubResponses.Clear();
            item.RequestResponseMap.Clear();
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public override Task<bool> DeleteRequestAsync(string correlationId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = GetCollection(distributionKey);
            var request = item.RequestResultModels.FirstOrDefault(r => r.CorrelationId == correlationId);
            if (request == null)
            {
                return Task.FromResult(false);
            }

            item.RequestResultModels.Remove(request);
            RemoveResponse(request, distributionKey);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public override Task<bool> DeleteStubAsync(string stubId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = GetCollection(distributionKey);
            var stub = item.StubModels.FirstOrDefault(s => s.Id == stubId);
            if (stub == null)
            {
                return Task.FromResult(false);
            }

            item.StubModels.Remove(stub);
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public override Task<IEnumerable<RequestResultModel>> GetRequestResultsAsync(PagingModel pagingModel,
        string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = GetCollection(distributionKey);
            var result = item.RequestResultModels.OrderByDescending(r => r.RequestBeginTime).ToArray();
            if (pagingModel != null)
            {
                IEnumerable<RequestResultModel> resultQuery = result;
                if (!string.IsNullOrWhiteSpace(pagingModel.FromIdentifier))
                {
                    var index = result
                        .Select((request, index) => new {request, index})
                        .Where(f => f.request.CorrelationId.Equals(pagingModel.FromIdentifier))
                        .Select(f => f.index)
                        .FirstOrDefault();
                    resultQuery = result
                        .Skip(index);
                }

                if (pagingModel.ItemsPerPage.HasValue)
                {
                    resultQuery = resultQuery.Take(pagingModel.ItemsPerPage.Value);
                }

                result = resultQuery.ToArray();
            }

            return Task.FromResult(result.AsEnumerable());
        }
    }

    /// <inheritdoc />
    public override Task<IEnumerable<StubModel>> GetStubsAsync(string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // We need to convert the list to an array here, or else we can get errors when deleting the stubs.
            var item = GetCollection(distributionKey);
            return Task.FromResult(item.StubModels.ToArray().AsEnumerable());
        }
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<StubOverviewModel>> GetStubsOverviewAsync(string distributionKey = null,
        CancellationToken cancellationToken = default) =>
        (await GetStubsAsync(distributionKey, cancellationToken))
        .Select(s => new StubOverviewModel {Id = s.Id, Tenant = s.Tenant, Enabled = s.Enabled})
        .ToArray();

    /// <inheritdoc />
    public override Task<StubModel> GetStubAsync(string stubId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        var item = GetCollection(distributionKey);
        return Task.FromResult(item.StubModels.FirstOrDefault(s => s.Id == stubId));
    }

    /// <inheritdoc />
    public override Task CleanOldRequestResultsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            foreach (var item in CollectionItems)
            {
                var maxLength = _options.CurrentValue.Storage?.OldRequestsQueueLength ?? 40;
                var requests = item.Value.RequestResultModels
                    .OrderByDescending(r => r.RequestEndTime)
                    .Skip(maxLength);
                foreach (var request in requests)
                {
                    item.Value.RequestResultModels.Remove(request);
                    RemoveResponse(request, null);
                }
            }

            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public override Task PrepareStubSourceAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void RemoveResponse(RequestResultModel request, string distributionKey)
    {
        var item = GetCollection(distributionKey);
        if (!item.RequestResponseMap.ContainsKey(request))
        {
            return;
        }

        var response = item.RequestResponseMap[request];
        item.StubResponses.Remove(response);
        item.RequestResponseMap.Remove(request);
    }

    private RequestResultModel GetRequest(string correlationId, string distributionKey)
    {
        var item = GetCollection(distributionKey);
        return item.RequestResultModels.FirstOrDefault(r => r.CorrelationId == correlationId);
    }

    internal StubRequestCollectionItem GetCollection(string distributionKey) =>
        CollectionItems.GetOrAdd(distributionKey ?? string.Empty,
            key => new StubRequestCollectionItem(key));
}

internal class StubRequestCollectionItem
{
    internal StubRequestCollectionItem(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public readonly IDictionary<RequestResultModel, ResponseModel> RequestResponseMap =
        new Dictionary<RequestResultModel, ResponseModel>();

    public readonly IList<RequestResultModel> RequestResultModels = new List<RequestResultModel>();
    public readonly IList<StubModel> StubModels = new List<StubModel>();
    public readonly IList<ResponseModel> StubResponses = new List<ResponseModel>();
}
