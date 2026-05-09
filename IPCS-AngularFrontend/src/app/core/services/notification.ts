import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

/**
 * NotificationService
 * Provides a global system for displaying toast notifications (Success, Error, Info).
 * Replaces generic browser alerts with a premium UI experience.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private toastsSubject = new BehaviorSubject<any[]>([]);
  public toasts$ = this.toastsSubject.asObservable();

  /**
   * Displays a success toast
   */
  success(message: string) {
    this.show(message, 'success');
  }

  /**
   * Displays an error toast
   */
  error(message: string) {
    this.show(message, 'error');
  }

  /**
   * Displays an informational toast
   */
  info(message: string) {
    this.show(message, 'info');
  }

  private show(message: string, type: 'success' | 'error' | 'info') {
    const id = Date.now();
    const current = this.toastsSubject.value;
    this.toastsSubject.next([...current, { id, message, type }]);

    // Auto-remove toast after 4 seconds
    setTimeout(() => {
      this.remove(id);
    }, 4000);
  }

  remove(id: number) {
    const current = this.toastsSubject.value.filter(t => t.id !== id);
    this.toastsSubject.next(current);
  }
}
