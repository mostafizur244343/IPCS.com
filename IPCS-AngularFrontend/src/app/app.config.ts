import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth-interceptor';

/**
 * Application Configuration
 * Configures global services including Routing and HttpClient with interceptors.
 * i18n: ngx-translate loads ./assets/i18n/<lang>.json (public/assets/i18n).
 * Default language is English; LanguageService restores the saved choice.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // Providing HttpClient with the global authentication and error interceptors
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    // Translation files are served from public/assets/i18n/.
    // NOTE: the loader MUST be passed inside provideTranslateService —
    // a separate provideTranslateHttpLoader() entry is overridden by the
    // service's own default (NoOp) loader registration.
    // useHttpBackend: the translation fetch must bypass the interceptor
    // chain — the auth interceptor injects NotificationService, which
    // injects TranslateService, which would be a circular dependency
    // (NG0200) while the service is still loading its first language.
    provideTranslateService({
      loader: provideTranslateHttpLoader({
        prefix: './assets/i18n/',
        suffix: '.json',
        useHttpBackend: true
      }),
      fallbackLang: 'en',
      lang: 'en'
    })
  ]
};
