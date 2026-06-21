export type AirportCurrentMetar = {
    rawMetar: string;
    issuedAt: string;
    isAuto: boolean;
    isCavok: boolean;
    isCorrected: boolean;
    windDirection?: number;
    windSpeedKnots?: number;
    windSpeedGustsKnots?: number;
    noCloudsDetected: boolean;
    visibilityMeters?: number;
    temperatureCelsius?: number;
    dewpointCelsius?: number;
    altimeterValue?: number;
    remarks?: string;
};