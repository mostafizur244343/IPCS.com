import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth-interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

/**
 * Application Configuration
 * Configures global services including Routing and HttpClient with interceptors.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // Providing HttpClient with the global authentication and error interceptors
    provideHttpClient(
      withInterceptors([authInterceptor, errorInterceptor])
    )
  ]
};
