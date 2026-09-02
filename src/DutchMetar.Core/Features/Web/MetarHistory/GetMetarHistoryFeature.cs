using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.Web.MetarHistory;

public class GetMetarHistoryFeature : IGetMetarHistoryFeature
{
    public const int DefaultPageSize = 50;
    
    private readonly DutchMetarContext _context;
    private readonly ILogger<GetMetarHistoryFeature> _logger;

    public GetMetarHistoryFeature(ILogger<GetMetarHistoryFeature> logger, DutchMetarContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<GetMetarHistoryResult> GetHistoryAsync(GetMetarHistoryRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving METAR history for Airport {ICAO}", request.Icao);
        Validate(request);

        var normalizedIcao = request.Icao.ToUpperInvariant();
        var airport = await _context.Airports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Icao == normalizedIcao, cancellationToken);

        if (airport == null)
        {
            throw new EntityNotFoundException(nameof(Airport), normalizedIcao);
        }

        var query = _context.Metars
            .AsNoTracking()
            .Where(x => x.AirportId == airport.Id);

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.IssuedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.IssuedAt <= request.EndDate.Value);
        }

        var pageSize = request.PageSize ?? DefaultPageSize;
        var totalData = await query.CountAsync(cancellationToken);
        var metarData = await query
            .OrderByDescending(x => x.IssuedAt)
            .Skip(request.Page * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new GetMetarHistoryResult
        {
            Icao = airport.Icao,
            AirportName = airport.Name,
            CurrentPage = request.Page,
            MaxPages = totalData == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
            TotalItems = totalData,
            MetarReports =
            [
                .. metarData.Select(x => new GetMetarHistoryResultReports
                {
                    MetarId = x.Id,
                    RawMetar = x.RawMetar,
                    IssuedAt = x.IssuedAt
                })
            ]
        };
    }

    private void Validate(GetMetarHistoryRequest request)
    {
        if (request.Page < 0)
        {
            throw new RequestValidationExxception("Page cannot be negative");
        }

        if (request.PageSize.GetValueOrDefault() <= 0)
        {
            throw new RequestValidationExxception("PageSize cannot be zero or negative");
        }

        if (string.IsNullOrWhiteSpace(request.Icao) || request.Icao.Length != 4)
        {
            throw new RequestValidationExxception("Invalid ICAO.");
        }
    }
}