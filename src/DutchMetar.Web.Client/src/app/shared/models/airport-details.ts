import { AirportCurrentMetar } from './airport-current-metar';

export type AirportDetails = {
    icao: string;
    lastUpdated: Date;
    latestWeather?: AirportCurrentMetar
}