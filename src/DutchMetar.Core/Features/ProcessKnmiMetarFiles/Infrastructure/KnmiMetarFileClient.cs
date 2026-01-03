using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DutchMetar.Core.Features.ProcessKnmiMetarFiles.Infrastructure.Contracts;
using DutchMetar.Core.Features.ProcessKnmiMetarFiles.Infrastructure.Interfaces;
using DutchMetar.Core.Features.SyncKnmiMetarFileList.Infrastructure;
using Microsoft.Extensions.Options;

namespace DutchMetar.Core.Features.ProcessKnmiMetarFiles.Infrastructure;

public class KnmiMetarFileClient : IKnmiMetarFileClient
{
    private readonly HttpClient _httpClient;
    private readonly KnmiMetarApiOptions _options;

    private const string Url =
        "https://api.dataplatform.knmi.nl/open-data/v1/datasets/metar/versions/1.0/files/{0}/url";

    public KnmiMetarFileClient(HttpClient httpClient, IOptions<KnmiMetarApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GetKnmiMetarFileContentAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var url = string.Format(Url, fileName);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AuthorizationToken);
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        // Assumption: KNMI API returns 429 code on rate limit reached
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new MaxRequestLimitReachedException();
        }
        response.EnsureSuccessStatusCode();
        
        var fileDownload = await  response.Content.ReadFromJsonAsync<KnmiFileDownload>(cancellationToken);

        if (fileDownload == null)
        {
            throw new NullReferenceException("Failed to deserialize KNMI file download response");
        }
        
        var content = await _httpClient.GetStringAsync(fileDownload.TemporaryDownloadUrl, cancellationToken);
        return content;
    }
}