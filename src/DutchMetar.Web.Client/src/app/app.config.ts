import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideNativeDateAdapter } from '@angular/material/core';
import { registerLocaleData } from '@angular/common';
import localeNl from '@angular/common/locales/nl';

registerLocaleData(localeNl);

export const appConfig: ApplicationConfig = {
    providers: [
        provideNativeDateAdapter(),
        provideBrowserGlobalErrorListeners(),
        provideRouter(routes),
        { provide: LOCALE_ID, useValue: 'nl-NL' },
    ],
};
