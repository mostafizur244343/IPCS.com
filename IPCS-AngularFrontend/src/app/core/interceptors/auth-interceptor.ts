import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
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
      let errorMessage = 'An unexpected error occurred.';
      
      if (error.status === 401) {
        // Token expired or unauthorized -> logout and redirect
        localStorage.removeItem('token');
        localStorage.removeItem('user_data');
        
        const now = Date.now();
        if (now - lastSessionExpiredTime > 5000) {
          lastSessionExpiredTime = now;
          notification.error('Your session has expired. Please login again.');
        }
        router.navigate(['/login']);
        errorMessage = 'Session expired';
      } else if (error.status === 403) {
        errorMessage = 'You do not have permission to perform this action.';
        notification.error(errorMessage);
      } else if (error.status === 0) {
        errorMessage = 'Cannot connect to the server. Please check your connection.';
        notification.error(errorMessage);
      } else {
        // Server-side errors or other
        errorMessage = error.error?.message || error.error?.Message || error.message || 'An unexpected error occurred.';
        notification.error(errorMessage);
      }
      
      // Return the original HttpErrorResponse so components can handle it specifically
      return throwError(() => error);
    })
  );
};
