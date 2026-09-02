import { MetarHistoryReport } from './metar-history-report';

export type MetarHistory = {
    icao: string;
    airportName?: string;
    currentPage: number;
    maxPages: number;
    totalItems: number;
    metarReports: MetarHistoryReport[];
}