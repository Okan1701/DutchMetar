export type AirportSummary = {
    icao: string;
    lastIssuedMetarDate: string;
    isAuto: boolean;
    isCavok: boolean;
    isCorrected: boolean;
    windDirection?: number;
    windSpeedKnots?: number;
};
