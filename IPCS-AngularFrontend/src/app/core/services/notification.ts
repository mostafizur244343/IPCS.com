import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import Swal from 'sweetalert2';

/**
 * NotificationService
 * Provides a global system for displaying dialog notifications (Success, Error, Info, Warning).
 * Replaces generic browser alerts and banner popups with premium SweetAlert2 popups.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  // Maintained for backward compatibility, unused now
  private toastsSubject = new BehaviorSubject<any[]>([]);
  public toasts$ = this.toastsSubject.asObservable();

  private fire(title: string, message: string, icon: 'success' | 'error' | 'info' | 'warning') {
    Swal.fire({
      title: title,
      text: message,
      icon: icon,
      width: '360px', // More compact, elegant width
      confirmButtonText: 'OK',
      confirmButtonColor: '#0f766e', // Theme primary Teal color
      background: '#ffffff',
      color: '#1e293b',
      heightAuto: false // Prevents body shift/scrollbar jump issues
    });
  }

  /**
   * Displays a success dialog popup
   */
  success(message: string, title: string = 'Success') {
    this.fire(title, message, 'success');
  }

  /**
   * Displays an error dialog popup
   */
  error(message: string, title: string = 'Error') {
    this.fire(title, message, 'error');
  }

  /**
   * Displays an informational dialog popup
   */
  info(message: string, title: string = 'Info') {
    this.fire(title, message, 'info');
  }

  /**
   * Displays a warning dialog popup
   */
  warning(message: string, title: string = 'Warning') {
    this.fire(title, message, 'warning');
  }

  /**
   * Displays a confirmation dialog and returns a promise resolving to boolean
   */
  confirm(
    message: string,
    title: string = 'Are you sure?',
    confirmButtonText: string = 'Yes',
    cancelButtonText: string = 'Cancel',
    isDestructive: boolean = true
  ): Promise<boolean> {
    return Swal.fire({
      title: title,
      text: message,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: confirmButtonText,
      cancelButtonText: cancelButtonText,
      confirmButtonColor: isDestructive ? '#ef4444' : '#0f766e',
      cancelButtonColor: '#64748b',
      background: '#ffffff',
      color: '#1e293b',
      width: '360px',
      heightAuto: false
    }).then(result => !!result.isConfirmed);
  }

  // Maintained for backward compatibility, unused now
  remove(id: number) {
    // No-op
  }
}
