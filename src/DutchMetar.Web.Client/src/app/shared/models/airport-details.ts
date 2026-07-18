import { AirportCurrentMetar } from './airport-current-metar';

export type AirportDetails = {
    icao: string;
    name?: string;
    lastUpdated: Date;
    latestWeather?: AirportCurrentMetar
}