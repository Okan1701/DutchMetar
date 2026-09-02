import { MeteoCondition } from '../../types/meteo-condition';

export type AirportSummary = {
    icao: string;
    lastIssuedMetarDate: string;
    isAuto: boolean;
    isCavok: boolean;
    isCorrected: boolean;
    meteoCondition: MeteoCondition
    windDirection?: number;
    windSpeedKnots?: number;
};
