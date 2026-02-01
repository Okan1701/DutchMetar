using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;

namespace DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Infrastructure;

public class KnmiMetarApiClient : IKnmiMetarApiClient
{
    private readonly HttpClient _httpClient;
    private readonly KnmiMetarApiOptions _options;
    private const string FileListUrl = "https://api.dataplatform.knmi.nl/open-data/v1/datasets/metar/versions/1.0/files";
    private const string FileDownloadUrl =
        "https://api.dataplatform.knmi.nl/open-data/v1/datasets/metar/versions/1.0/files/{0}/url";

    public KnmiMetarApiClient(HttpClient httpClient, IOptions<KnmiMetarApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<KnmiListFilesResponse> GetMetarFileSummaries(KnmiFilesParameters parameters, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AuthorizationToken);
        var url = GetUrlWithQueryParameters(FileListUrl, parameters);
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        // KNMI API returns 429 code on rate limit reached
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new KnmiRateLimitReachedException();
        }
        
        await HandleStatusCodeAsync(response);
        
        var data =  await response.Content.ReadFromJsonAsync<KnmiListFilesResponse>(cancellationToken);
        
        return data ?? throw new NullReferenceException("Failed to deserialize response");
    }
    
    public async Task<string> GetKnmiMetarFileContentAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var url = string.Format(FileDownloadUrl, fileName);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AuthorizationToken);
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        // Assumption: KNMI API returns 429 code on rate limit reached
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new KnmiRateLimitReachedException();
        }
        await HandleStatusCodeAsync(response);
        
        var fileDownload = await  response.Content.ReadFromJsonAsync<KnmiFileDownload>(cancellationToken);

        if (fileDownload == null)
        {
            throw new NullReferenceException("Failed to deserialize KNMI file download response");
        }
        
        // URL already contains auth value, additional authorization options result in HTTP 400
        _httpClient.DefaultRequestHeaders.Authorization = null;
        var fileDownloadResponse = await _httpClient.GetAsync(fileDownload.TemporaryDownloadUrl, cancellationToken);
        await HandleStatusCodeAsync(fileDownloadResponse);
        var content = await fileDownloadResponse.Content.ReadAsStringAsync(cancellationToken);
        return content;
    }

    private string GetUrlWithQueryParameters(string baseUrl, KnmiFilesParameters parameters)
    {
        var url = baseUrl + "?";

        if (parameters.Begin.HasValue)
        {
            url += $"begin={parameters.Begin.Value.ToString("o", CultureInfo.InvariantCulture)}&";
        }
        
        if (parameters.End.HasValue)
        {
            url += $"end={parameters.End.Value.ToString("o", CultureInfo.InvariantCulture)}&";
        }
        
        if (parameters.MaxKeys.HasValue)
        {
            url += $"maxKeys={parameters.MaxKeys.Value}&";
        }

        if (!string.IsNullOrEmpty(parameters.NextPageToken))
        {
            url += $"nextPageToken={parameters.NextPageToken}&";
        }

        if (!string.IsNullOrEmpty(parameters.Sorting))
        {
            url += $"sorting={parameters.Sorting}&";
        }
        
        if (!string.IsNullOrEmpty(parameters.OrderBy))
        {
            url += $"orderBy={parameters.OrderBy}&";
        }

        if (url.EndsWith('&'))
        {
            url = url.Remove(url.Length - 1);
        }
        
        return url;
    }
    
    /// <summary>
    /// Check the response status and throw an appropriate exception
    /// </summary>
    private async Task HandleStatusCodeAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        
        // Error responses are not documented, so it's trial and error...
        var responseContent = await response.Content.ReadAsStringAsync();
        KnmiError? errorResponse = await response.Content.ReadFromJsonAsync<KnmiError>();

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new KnmiRateLimitReachedException();
        }
        
        // Their documentation states that they return 429 when rate limit is reached
        // But in practice their seem to also use 403 error when the quote is exceeded...
        if (response.StatusCode == HttpStatusCode.Forbidden && errorResponse?.Error == "Quota exceeded")
        {
            throw new KnmiRateLimitReachedException(response.StatusCode, errorResponse.Error);
        }
        
        // General HTTP errors are wrapped in this exception
        throw new KnmiApiException(response.StatusCode, errorResponse?.Error ?? responseContent);
    }
}