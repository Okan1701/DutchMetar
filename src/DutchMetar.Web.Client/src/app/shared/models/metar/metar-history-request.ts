export type MetarHistoryRequest = {
    icao: string;
    startDate?: string;
    endDate?: string;
    pageSize?: number;
    page: number;
}