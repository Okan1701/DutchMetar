using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.SyncKnmiMetar.Exceptions;

namespace DutchMetar.Core.Features.SyncKnmiMetar.Interfaces;

/// <summary>
/// Processes raw METAR into <see cref="Metar"/> and <see cref="Airport"/> entities.
/// </summary>
public interface IMetarProcessor
{
    /// <summary>
    /// Processes the given raw METAR string by decoding it and creating a <see cref="Metar"/> object.
    /// If no parent <see cref="Airport"/> entity exists, a new one is created
    /// </summary>
    /// <param name="metar">The raw megtar string to process.</param>
    /// <param name="airportName">(Optional) name of the associated airport</param>
    /// <param name="createdAt">The date that the METAR was created.</param>
    /// <param name="cancellationToken">(Optional) cancellation token</param>
    /// <exception cref="MetarParseException"></exception>
    Task ProcessRawMetarAsync(string metar, string? airportName, DateTimeOffset createdAt, CancellationToken cancellationToken);
}