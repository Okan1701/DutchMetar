using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Core.Features.Web.TafHistory;

public class GetTafHistoryFeature : IGetTafHistoryFeature
{
    public const int DefaultPageSize = 50;

    private readonly DutchMetarContext _context;
    private readonly ILogger<GetTafHistoryFeature> _logger;

    public GetTafHistoryFeature(ILogger<GetTafHistoryFeature> logger, DutchMetarContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<GetTafHistoryResult> GetHistoryAsync(
        GetTafHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving TAF history for Airport {ICAO}", request.Icao);
        Validate(request);

        var normalizedIcao = request.Icao.ToUpperInvariant();
        var airport = await _context.Airports
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Icao == normalizedIcao, cancellationToken);

        if (airport == null)
        {
            throw new EntityNotFoundException(nameof(Airport), normalizedIcao);
        }

        var query = _context.Tafs
            .AsNoTracking()
            .Where(x => x.AirportId == airport.Id);

        if (request.StartDate.HasValue)
        {
            var startDate = new DateTimeOffset(
                request.StartDate.Value.Year,
                request.StartDate.Value.Month,
                request.StartDate.Value.Day,
                0,
                0,
                0,
                request.StartDate.Value.Offset);
            query = query.Where(x => x.IssuedAt >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDateExclusive = new DateTimeOffset(
                request.EndDate.Value.Year,
                request.EndDate.Value.Month,
                request.EndDate.Value.Day,
                0,
                0,
                0,
                request.EndDate.Value.Offset);
            endDateExclusive = endDateExclusive.AddDays(1);
            query = query.Where(x => x.IssuedAt < endDateExclusive);
        }

        var pageSize = request.PageSize ?? DefaultPageSize;
        var totalData = await query.CountAsync(cancellationToken);
        var tafData = await query
            .OrderByDescending(x => x.IssuedAt)
            .ThenByDescending(x => x.Id)
            .Skip(request.Page * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new GetTafHistoryResult
        {
            Icao = airport.Icao,
            AirportName = airport.Name,
            CurrentPage = request.Page,
            MaxPages = totalData == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
            TotalItems = totalData,
            TafReports =
            [
                .. tafData.Select(x => new GetTafHistoryResultReport
                {
                    TafId = x.Id,
                    RawTaf = x.RawTaf,
                    IssuedAt = x.IssuedAt
                })
            ]
        };
    }

    private static void Validate(GetTafHistoryRequest request)
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
