import { AirportDayHistorySnapshot } from './airport-day-history-snapshot';

export type AirportDayHistory = {
    icao: string;
    isMissingData: boolean;
    history: AirportDayHistorySnapshot[];
}