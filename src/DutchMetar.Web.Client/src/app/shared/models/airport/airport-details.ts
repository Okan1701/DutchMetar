import { AirportCurrentMetar } from './airport-current-metar';
import { MeteoCondition } from '../../types/meteo-condition';
import { AirportForecast } from './airport-forecast';

export type AirportDetails = {
    icao: string;
    name?: string;
    meteoCondition: MeteoCondition;
    lastUpdated: Date;
    latestWeather?: AirportCurrentMetar
    latestForecast?: AirportForecast
}
