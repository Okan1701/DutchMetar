export type TafHistoryRequest = {
    icao: string;
    startDate?: string;
    endDate?: string;
    pageSize?: number;
    page: number;
};
