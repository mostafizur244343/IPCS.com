import { Injectable } from '@angular/core';
import { ApiService } from './api';
import { BehaviorSubject, Observable, tap } from 'rxjs';

/**
 * PermissionService
 * Manages user permissions and access control.
 * Stores permissions in memory and local storage for quick checking.
 */
@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private permissionsSubject = new BehaviorSubject<string[]>([]);
  public permissions$ = this.permissionsSubject.asObservable();

  constructor(private api: ApiService) {
    this.loadFromStorage();
  }

  /**
   * Loads permissions from localStorage on application start
   */
  private loadFromStorage() {
    const saved = localStorage.getItem('user_permissions');
    if (saved) {
      this.permissionsSubject.next(JSON.parse(saved));
    }
  }

  /**
   * Fetches latest permissions from the API for the logged-in user
   */
  fetchMyPermissions(): Observable<string[]> {
    return this.api.get<string[]>('Permission/my-permissions').pipe(
      tap(perms => {
        this.permissionsSubject.next(perms);
        localStorage.setItem('user_permissions', JSON.stringify(perms));
      })
    );
  }

  /**
   * Checks if the user has a specific permission
   */
  hasPermission(permission: string): boolean {
    const current = this.permissionsSubject.value;
    return current.includes(permission) || current.includes('SuperAdmin');
  }

  /**
   * Checks if user has any of the provided permissions
   */
  hasAnyPermission(permissions: string[]): boolean {
    const current = this.permissionsSubject.value;
    if (current.includes('SuperAdmin')) return true;
    return permissions.some(p => current.includes(p));
  }

  /**
   * Clears permissions on logout
   */
  clear() {
    this.permissionsSubject.next([]);
    localStorage.removeItem('user_permissions');
  }
}
