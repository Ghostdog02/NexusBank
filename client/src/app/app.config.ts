import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
  provideZonelessChangeDetection
} from '@angular/core';

import { provideRouter, withDebugTracing } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';

import { authInterceptor } from './auth/auth.interceptor';
import { authErrorInterceptor } from './auth-error/auth-error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withDebugTracing()),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-bottom-right',
      preventDuplicates: true
    }),
  ],
};
