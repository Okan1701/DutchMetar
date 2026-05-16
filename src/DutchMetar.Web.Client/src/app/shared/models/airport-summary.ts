export type AirportSummary = {
    icao: string;
    lastIssuedMetarDate: string;
    isAuto: boolean;
    isCavok: boolean;
    windDirection?: number;
    windSpeedKnots?: number;
};
