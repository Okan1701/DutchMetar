import { Routes } from '@angular/router';
import { Home } from "./features/home/home";
import { Airport } from './features/airport/airport';
import { Metar } from './features/metar/metar';

export const routes: Routes = [
    {
        path: '',
        component: Home,
    },
    {
        path: 'airport/:icao',
        component: Airport,
    },
    {
        path: 'metar/:icao',
        component: Metar,
    },
];
