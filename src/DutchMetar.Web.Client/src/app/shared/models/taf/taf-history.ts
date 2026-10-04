import { TafHistoryReport } from './taf-history-report';

export type TafHistory = {
    icao: string;
    airportName?: string;
    currentPage: number;
    maxPages: number;
    totalItems: number;
    tafReports: TafHistoryReport[];
};
