import { MeteoCondition } from '../../types/meteo-condition';
import { AirportCurrentMetar } from './airport-current-metar';
import { AirportForecast } from './airport-forecast';

export type AirportSummary = {
    icao: string;
    name?: string;
    lastUpdated: Date;
    latestWeather?: AirportCurrentMetar;
    latestForecast?: AirportForecast;
    meteoCondition: MeteoCondition
};
