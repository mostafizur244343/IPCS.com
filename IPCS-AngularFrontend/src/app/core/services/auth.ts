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
        const token = res.token || res.Token;
        const user = res.user || res.User;
        if (token) localStorage.setItem('token', token);
        if (user) localStorage.setItem('user_data', JSON.stringify(user));
        
        // Load permissions immediately
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
   * Checks if the user is currently authenticated and the token is not expired
   */
  isAuthenticated(): boolean {
    const token = localStorage.getItem('token');
    if (!token) return false;

    try {
      // Decode JWT payload (middle part) to check expiration
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp;
      const now = Math.floor(Date.now() / 1000);
      
      if (expiry && now >= expiry) {
        // Token has expired
        this.logout();
        return false;
      }
      return true;
    } catch (e) {
      // Invalid token format
      this.logout();
      return false;
    }
  }
}
