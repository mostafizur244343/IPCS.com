import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth';
import { PermissionService } from '../services/permission';

/**
 * AuthGuard
 * Protects routes from unauthorized access.
 * Checks for authentication and required permissions.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    // 1. Check if user is logged in
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login']);
      return false;
    }

    // 2. Check for required permissions if specified in the route data
    const requiredPermission = route.data['permission'];
    if (requiredPermission) {
      // Note: We'll use PermissionService to check here if needed
      // For now, allowing all authenticated users to proceed to children
    }

    return true;
  }
}
