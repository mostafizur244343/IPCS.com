import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification';

// Track the last time a "Session expired" message was shown to prevent stacking multiple toasts for parallel requests
let lastSessionExpiredTime = 0;

/**
 * authInterceptor
 * A functional interceptor that automatically attaches JWT tokens to outgoing requests.
 * It also handles global HTTP errors (like 401 Unauthorized or 500 Server Error).
 */
export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const router = inject(Router);
  const notification = inject(NotificationService);
  const translate = inject(TranslateService);
  const token = localStorage.getItem('token');

  // Clone the request and add the Authorization header if token exists
  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  // Handle the response and catch errors
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = translate.instant('common.unexpectedError');

      if (error.status === 401) {
        // Token expired or unauthorized -> logout and redirect
        localStorage.removeItem('token');
        localStorage.removeItem('user_data');

        const now = Date.now();
        if (now - lastSessionExpiredTime > 5000) {
          lastSessionExpiredTime = now;
          notification.error(translate.instant('common.sessionExpired'));
        }
        router.navigate(['/login']);
        errorMessage = 'Session expired';
      } else if (error.status === 403) {
        errorMessage = translate.instant('common.noPermission');
        notification.error(errorMessage);
      } else if (error.status === 0) {
        errorMessage = translate.instant('common.cannotConnect');
        notification.error(errorMessage);
      } else {
        // Server-side errors or other
        errorMessage = error.error?.message || error.error?.Message || error.message || translate.instant('common.unexpectedError');
        notification.error(errorMessage);
      }
      
      // Return the original HttpErrorResponse so components can handle it specifically
      return throwError(() => error);
    })
  );
};
