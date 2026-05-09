import { Injectable } from '@angular/core';
import { ApiService } from './api';
import { PermissionService } from './permission';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

/**
 * AuthService
 * Handles user authentication, token management, and logout.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(
    private api: ApiService,
    private permService: PermissionService,
    private router: Router
  ) {}

  /**
   * Logs in a user and fetches their permissions
   */
  login(credentials: any) {
    return this.api.post<any>('Account/login', credentials).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('user_data', JSON.stringify(res.user));
        // After login, fetch permissions
        this.permService.fetchMyPermissions().subscribe();
      })
    );
  }

  /**
   * Logs out the current user and clears all session data
   */
  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user_data');
    this.permService.clear();
    this.router.navigate(['/login']);
  }

  /**
   * Checks if the user is currently authenticated
   */
  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }
}
