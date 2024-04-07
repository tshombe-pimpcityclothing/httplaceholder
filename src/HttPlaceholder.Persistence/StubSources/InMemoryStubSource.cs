using System.Threading;
using System.Threading.Tasks;
using HttPlaceholder.Application.Configuration.Models;
using Microsoft.Extensions.Options;

namespace HttPlaceholder.Persistence.StubSources;

/// <summary>
///     A stub source that is used to store and read data from memory.
/// </summary>
internal class InMemoryStubSource(IOptionsMonitor<SettingsModel> options) : BaseMemoryStubSource(options)
{
    protected override Task SaveStubTrackingMetadataAsync(string distributionKey, string stubUpdateTrackingId,
        CancellationToken cancellationToken) => Task.CompletedTask;
}
