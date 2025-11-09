using System.Text.RegularExpressions;
using DutchMetar.Core.Features.LoadDutchMetars.Exceptions;
using DutchMetar.Core.Features.LoadDutchMetars.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.LoadDutchMetars;

public class KnmiMetarRepository : IKnmiMetarRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KnmiMetarRepository> _logger;
    private const string MetarUrl = "https://www.knmi.nl/nederland-nu/luchtvaart/vliegveldwaarnemingen";
    private const string MetarBlockXPath = "/html/body/main/div[2]/div[2]/div/pre";

    public KnmiMetarRepository(HttpClient httpClient, ILogger<KnmiMetarRepository> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ICollection<string>> GetKnmiRawMetarsAsync()
    {
        var httpResponse = await _httpClient.GetAsync(MetarUrl);
        httpResponse.EnsureSuccessStatusCode();

        var httpBodyStream = await httpResponse.Content.ReadAsStreamAsync();
        var htmlDocument = new HtmlDocument();
        htmlDocument.Load(httpBodyStream);

        if (htmlDocument.ParseErrors.Any())
        {
            _logger.LogWarning("Parse error occured while loading HTML document");
        }

        var metarNode = htmlDocument.DocumentNode.SelectSingleNode(MetarBlockXPath);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        // Documentation states that SelectSingleNode can return null
        if (metarNode == null)
        {
            throw new MetarParseException("HTML node containing METAR could not be found");
        }

        var metarTextBlocks = metarNode.InnerText.Split("ZCZC");
        var pattern = new Regex("METAR[\\s\\S]*");
        return metarTextBlocks.Select(metarBlock =>
        {
            var match = pattern.Match(metarBlock);
            return match.Value.Trim();
        }).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    }
}