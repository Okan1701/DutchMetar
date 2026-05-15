import { Routes } from '@angular/router';
import { Home } from "./features/home/home";
import { Airport } from './features/airport/airport';

export const routes: Routes = [
    {
        path: '',
        component: Home,
    },
    {
        path: 'airport/:icao',
        component: Airport,
    },
];
