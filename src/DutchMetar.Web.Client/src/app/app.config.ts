import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
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
        provideHttpClient(),
        provideRouter(routes),
        { provide: LOCALE_ID, useValue: 'nl-NL' },
    ],
};
