using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HttPlaceholder.Application.Configuration.Models;
using HttPlaceholder.Common;
using HttPlaceholder.Common.Utilities;
using HttPlaceholder.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HttPlaceholder.Persistence.StubSources;

/// <summary>
///     A stub source that is used to store and read data on the file system.
/// </summary>
internal class FileSystemStubSource(
    IFileService fileService,
    IOptionsMonitor<SettingsModel> options,
    IDateTime dateTime)
    : BaseMemoryStubSource(options)
{
    /// <inheritdoc />
    public override async Task AddRequestResultAsync(RequestResultModel requestResult, ResponseModel responseModel,
        string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureDirectoriesExist(distributionKey, cancellationToken);

        var requestsFolder = GetRequestsFolder(distributionKey);
        var responsesFolder = GetResponsesFolder(distributionKey);
        await StoreResponseAsync(requestResult, responseModel, responsesFolder, cancellationToken);

        var requestFilePath = Path.Combine(requestsFolder, ConstructRequestFilename(requestResult.CorrelationId));
        var requestContents = JsonConvert.SerializeObject(requestResult);
        await fileService.WriteAllTextAsync(requestFilePath, requestContents, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task AddStubAsync(StubModel stub, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        // TODO
        // - Als je een stub toevoegt, moet de cache worden bijgewerkt en moet er een nieuw stub tracking ID worden ingesteld.
        // - Als de stub is bijgewerkt buiten de API om en de stub tracking ID is gewijzigd, moet de cache opnieuw opgebouwd worden. Dit gebeurt wanneer je de "get stubs" methode aanroept.
        await EnsureDirectoriesExist(distributionKey, cancellationToken);

        var path = GetStubsFolder(distributionKey);
        var filePath = Path.Combine(path, ConstructStubFilename(stub.Id));
        var contents = JsonConvert.SerializeObject(stub);
        await fileService.WriteAllTextAsync(filePath, contents, cancellationToken);
        await base.AddStubAsync(stub, distributionKey, cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<RequestResultModel> GetRequestAsync(string correlationId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        var requestFilePath = await FindRequestFilenameAsync(correlationId, distributionKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(requestFilePath))
        {
            return null;
        }

        var contents = await fileService.ReadAllTextAsync(requestFilePath, cancellationToken);
        return JsonConvert.DeserializeObject<RequestResultModel>(contents);
    }

    /// <inheritdoc />
    public override async Task<ResponseModel> GetResponseAsync(string correlationId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        var path = GetResponsesFolder(distributionKey);
        var filePath = Path.Combine(path, ConstructResponseFilename(correlationId));
        if (!await fileService.FileExistsAsync(filePath, cancellationToken))
        {
            return null;
        }

        var contents = await fileService.ReadAllTextAsync(filePath, cancellationToken);
        return JsonConvert.DeserializeObject<ResponseModel>(contents);
    }

    /// <inheritdoc />
    public override async Task DeleteAllRequestResultsAsync(string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        var requestsPath = GetRequestsFolder(distributionKey);
        var files = await fileService.GetFilesAsync(requestsPath, "*.json", cancellationToken);
        foreach (var filePath in files)
        {
            await fileService.DeleteFileAsync(filePath, cancellationToken);
            await DeleteResponseAsync(filePath, distributionKey, cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async Task<bool> DeleteRequestAsync(string correlationId, string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        var requestFilePath = await FindRequestFilenameAsync(correlationId, distributionKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(requestFilePath))
        {
            return false;
        }

        await DeleteResponseAsync(ConstructResponseFilename(correlationId), distributionKey, cancellationToken);
        await fileService.DeleteFileAsync(requestFilePath, cancellationToken);
        return true;
    }

    protected override Task SaveStubTrackingMetadataAsync(string distributionKey, string stubUpdateTrackingId,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    // /// <inheritdoc />
    // public override async Task<bool> DeleteStubAsync(string stubId, string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     var path = GetStubsFolder(distributionKey);
    //     var filePath = Path.Combine(path, ConstructStubFilename(stubId));
    //     if (!await fileService.FileExistsAsync(filePath, cancellationToken))
    //     {
    //         return false;
    //     }
    //
    //     await fileService.DeleteFileAsync(filePath, cancellationToken);
    //     // if (string.IsNullOrWhiteSpace(distributionKey))
    //     // {
    //     //     fileSystemStubCache.DeleteStub(stubId);
    //     // }
    //     // TODO update cache
    //
    //     return true;
    // }
    //
    // /// <inheritdoc />
    // public override async Task<IEnumerable<RequestResultModel>> GetRequestResultsAsync(PagingModel pagingModel,
    //     string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     var path = GetRequestsFolder(distributionKey);
    //     var files = (await fileService.GetFilesAsync(path, "*.json", cancellationToken))
    //         .OrderByDescending(f => f)
    //         .ToArray();
    //     if (pagingModel != null)
    //     {
    //         IEnumerable<string> filesQuery = files;
    //         if (!string.IsNullOrWhiteSpace(pagingModel.FromIdentifier))
    //         {
    //             var index = files
    //                 .Select((file, index) => new { file, index })
    //                 .Where(f => f.file.Contains(pagingModel.FromIdentifier))
    //                 .Select(f => f.index)
    //                 .FirstOrDefault();
    //             filesQuery = files
    //                 .Skip(index);
    //         }
    //
    //         if (pagingModel.ItemsPerPage.HasValue)
    //         {
    //             filesQuery = filesQuery.Take(pagingModel.ItemsPerPage.Value);
    //         }
    //
    //         files = filesQuery.ToArray();
    //     }
    //
    //     var result = files
    //         .Select(filePath => fileService
    //             .ReadAllTextAsync(filePath, cancellationToken))
    //         .ToArray();
    //     var results = await Task.WhenAll(result);
    //     return results.Select(JsonConvert.DeserializeObject<RequestResultModel>);
    // }
    //
    // /// <inheritdoc />
    // public override async Task<IEnumerable<(StubModel Stub, Dictionary<string, string> Metadata)>> GetStubsAsync(
    //     string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     await EnsureDirectoriesExist(distributionKey, cancellationToken);
    //     var path = GetStubsFolder(distributionKey);
    //     var files = await fileService.GetFilesAsync(path, "*.json", cancellationToken);
    //     var result = (await Task.WhenAll(files
    //             .Select(filePath => fileService
    //                 .ReadAllTextAsync(filePath, cancellationToken))))
    //         .Select(JsonConvert.DeserializeObject<StubModel>);
    //     return result.Select(s => (s, new Dictionary<string, string>()));
    //     // TODO read / initialize cache
    //     // var result = await fileSystemStubCache.GetOrUpdateStubCacheAsync(distributionKey,
    //     //     cancellationToken);
    //     // return result.Select(s => (s, new Dictionary<string, string>()));
    // }
    //
    // /// <inheritdoc />
    // public override async Task<(StubModel Stub, Dictionary<string, string> Metadata)?> GetStubAsync(
    //     string stubId,
    //     string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     await EnsureDirectoriesExist(distributionKey, cancellationToken);
    //     var stubs = await GetStubsAsync(distributionKey, cancellationToken);
    //     var result = stubs.FirstOrDefault(s => s.Stub.Id == stubId);
    //     return result.Stub != null ? (result.Stub, new Dictionary<string, string>()) : null;
    // }
    //
    // /// <inheritdoc />
    // public override async Task<IEnumerable<(StubOverviewModel Stub, Dictionary<string, string> Metadata)>>
    //     GetStubsOverviewAsync(string distributionKey = null,
    //         CancellationToken cancellationToken = default) =>
    //     (await GetStubsAsync(distributionKey, cancellationToken))
    //     .Select(s => (new StubOverviewModel { Id = s.Stub.Id, Tenant = s.Stub.Tenant, Enabled = s.Stub.Enabled },
    //         s.Metadata))
    //     .ToArray();
    //
    // /// <inheritdoc />
    // public override async Task CleanOldRequestResultsAsync(CancellationToken cancellationToken = default)
    // {
    //     var path = GetRequestsFolder();
    //     var folders = await fileService.GetDirectoriesAsync(path, cancellationToken);
    //     await HandleCleaningOfOldRequests(path, null, cancellationToken);
    //     foreach (var folder in folders)
    //     {
    //         await HandleCleaningOfOldRequests(folder, new DirectoryInfo(folder).Name, cancellationToken);
    //     }
    // }
    //
    // /// <inheritdoc />
    // public override async Task<ScenarioStateModel> GetScenarioAsync(string scenario, string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     var path = GetScenariosFolder(distributionKey);
    //     var scenarioPath = Path.Combine(path, ConstructScenarioFilename(scenario));
    //     if (!await fileService.FileExistsAsync(scenarioPath, cancellationToken))
    //     {
    //         return null;
    //     }
    //
    //     var contents = await fileService.ReadAllTextAsync(scenarioPath, cancellationToken);
    //     return JsonConvert.DeserializeObject<ScenarioStateModel>(contents);
    // }
    //
    // /// <inheritdoc />
    // public override async Task<ScenarioStateModel> AddScenarioAsync(string scenario,
    //     ScenarioStateModel scenarioStateModel,
    //     string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     await EnsureDirectoriesExist(distributionKey, cancellationToken);
    //     var path = GetScenariosFolder(distributionKey);
    //     var scenarioPath = Path.Combine(path, ConstructScenarioFilename(scenario));
    //     if (await fileService.FileExistsAsync(scenarioPath, cancellationToken))
    //     {
    //         throw new InvalidOperationException($"Scenario state with key '{scenario}' already exists.");
    //     }
    //
    //     await fileService.WriteAllTextAsync(scenarioPath, JsonConvert.SerializeObject(scenarioStateModel),
    //         cancellationToken);
    //     return scenarioStateModel;
    // }
    //
    // /// <inheritdoc />
    // public override async Task UpdateScenarioAsync(string scenario, ScenarioStateModel scenarioStateModel,
    //     string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     var path = GetScenariosFolder(distributionKey);
    //     var scenarioPath = Path.Combine(path, ConstructScenarioFilename(scenario));
    //     if (!await fileService.FileExistsAsync(scenarioPath, cancellationToken))
    //     {
    //         throw new InvalidOperationException($"Scenario state with key '{scenario}' not found.");
    //     }
    //
    //     await fileService.WriteAllTextAsync(scenarioPath, JsonConvert.SerializeObject(scenarioStateModel),
    //         cancellationToken);
    // }
    //
    // /// <inheritdoc />
    // public override async Task<IEnumerable<ScenarioStateModel>> GetAllScenariosAsync(string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     var path = GetScenariosFolder(distributionKey);
    //     var files = await fileService.GetFilesAsync(path, "*.json", cancellationToken);
    //     var result = new List<ScenarioStateModel>();
    //     foreach (var file in files)
    //     {
    //         var contents = await fileService.ReadAllTextAsync(file, cancellationToken);
    //         result.Add(JsonConvert.DeserializeObject<ScenarioStateModel>(contents));
    //     }
    //
    //     return result;
    // }
    //
    // /// <inheritdoc />
    // public override async Task<bool> DeleteScenarioAsync(string scenario, string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     if (string.IsNullOrWhiteSpace(scenario))
    //     {
    //         return false;
    //     }
    //
    //     var path = GetScenariosFolder(distributionKey);
    //     var scenarioPath = Path.Combine(path, ConstructScenarioFilename(scenario));
    //     if (!await fileService.FileExistsAsync(scenarioPath, cancellationToken))
    //     {
    //         return false;
    //     }
    //
    //     await fileService.DeleteFileAsync(scenarioPath, cancellationToken);
    //     return true;
    // }
    //
    // /// <inheritdoc />
    // public override async Task DeleteAllScenariosAsync(string distributionKey = null,
    //     CancellationToken cancellationToken = default)
    // {
    //     var path = GetScenariosFolder(distributionKey);
    //     var files = await fileService.GetFilesAsync(path, "*.json", cancellationToken);
    //     foreach (var file in files)
    //     {
    //         await fileService.DeleteFileAsync(file, cancellationToken);
    //     }
    // }

    private async Task HandleCleaningOfOldRequests(string path, string distributionKey,
        CancellationToken cancellationToken)
    {
        var maxLength = Options.CurrentValue.Storage?.OldRequestsQueueLength ?? 40;
        var filePaths = await fileService.GetFilesAsync(path, "*.json", cancellationToken);
        var filePathsAndDates = filePaths
            .Select(p => (path: p, lastWriteTime: fileService.GetLastWriteTime(p)))
            .OrderByDescending(t => t.lastWriteTime)
            .Skip(maxLength);
        foreach (var filePath in filePathsAndDates)
        {
            await fileService.DeleteFileAsync(filePath.path, cancellationToken);
            await DeleteResponseAsync(filePath.path, distributionKey, cancellationToken);
        }
    }
    //
    // /// <inheritdoc />
    // public override async Task PrepareStubSourceAsync(CancellationToken cancellationToken)
    // {
    //     await CreateDirectoryIfNotExistsAsync(GetRootFolder(), cancellationToken);
    //     await EnsureDirectoriesExist(null, cancellationToken);
    //     // await fileSystemStubCache.GetOrUpdateStubCacheAsync(null, cancellationToken);
    //     await GetStubsAsync(null, cancellationToken);
    // }
    //
    private async Task EnsureDirectoriesExist(string distributionKey = null,
        CancellationToken cancellationToken = default)
    {
        await CreateDirectoryIfNotExistsAsync(GetRequestsFolder(distributionKey), cancellationToken);
        await CreateDirectoryIfNotExistsAsync(GetResponsesFolder(distributionKey), cancellationToken);
        await CreateDirectoryIfNotExistsAsync(GetStubsFolder(distributionKey), cancellationToken);
        await CreateDirectoryIfNotExistsAsync(GetScenariosFolder(distributionKey), cancellationToken);
    }


    private async Task DeleteResponseAsync(string filePath, string distributionKey,
        CancellationToken cancellationToken = default)
    {
        var responsesPath = GetResponsesFolder(distributionKey);
        var responseFileName = Path.GetFileName(filePath);
        var responseFilePath = Path.Combine(responsesPath, responseFileName);
        if (await fileService.FileExistsAsync(responseFilePath, cancellationToken))
        {
            await fileService.DeleteFileAsync(responseFilePath, cancellationToken);
        }
    }

    private async Task StoreResponseAsync(
        RequestResultModel requestResult,
        ResponseModel responseModel,
        string responsesFolder,
        CancellationToken cancellationToken)
    {
        if (responseModel != null)
        {
            requestResult.HasResponse = true;
            var responseFilePath =
                Path.Combine(responsesFolder, ConstructResponseFilename(requestResult.CorrelationId));
            var responseContents = JsonConvert.SerializeObject(responseModel);
            await fileService.WriteAllTextAsync(responseFilePath, responseContents, cancellationToken);
        }
    }

    private async Task CreateDirectoryIfNotExistsAsync(string path, CancellationToken cancellationToken)
    {
        if (!await fileService.DirectoryExistsAsync(path, cancellationToken))
        {
            await fileService.CreateDirectoryAsync(path, cancellationToken);
        }
    }

    private static string ConstructStubFilename(string stubId) => PathUtilities.CleanPath($"{stubId}.json");

    private static string ConstructScenarioFilename(string scenario) =>
        PathUtilities.CleanPath($"scenario-{scenario.ToLower()}.json");

    private static string ConstructOldRequestFilename(string correlationId) => $"{correlationId}.json";

    private string ConstructRequestFilename(string correlationId) => $"{dateTime.UtcNowUnix}-{correlationId}.json";

    internal async Task<string> FindRequestFilenameAsync(string correlationId, string distributionKey,
        CancellationToken cancellationToken)
    {
        var requestsFolder = GetRequestsFolder(distributionKey);
        var oldRequestFilename = ConstructOldRequestFilename(correlationId);
        var oldRequestPath = Path.Join(requestsFolder, oldRequestFilename);
        if (await fileService.FileExistsAsync(oldRequestPath, cancellationToken))
        {
            return oldRequestPath;
        }

        var files = await fileService.GetFilesAsync(requestsFolder, $"*-{correlationId}.json", cancellationToken);
        return files.Length == 0 ? null : files[0];
    }

    private static string ConstructResponseFilename(string correlationId) => $"{correlationId}.json";

    private string GetRootFolder()
    {
        var folder = Options.CurrentValue.Storage?.FileStorageLocation;
        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new InvalidOperationException("File storage location unexpectedly not set.");
        }

        return folder;
    }

    private string GetStubsFolder(string distributionKey = null) =>
        GetFolderPath(distributionKey, FileNames.StubsFolderName);

    private string GetRequestsFolder(string distributionKey = null) =>
        GetFolderPath(distributionKey, FileNames.RequestsFolderName);

    private string GetResponsesFolder(string distributionKey = null) =>
        GetFolderPath(distributionKey, FileNames.ResponsesFolderName);

    private string GetScenariosFolder(string distributionKey = null) =>
        GetFolderPath(distributionKey, FileNames.ScenariosFolderName);

    private string GetFolderPath(string distributionKey, string folderName) =>
        !string.IsNullOrWhiteSpace(distributionKey)
            ? Path.Combine(GetRootFolder(), distributionKey, folderName)
            : Path.Combine(GetRootFolder(), folderName);
}
