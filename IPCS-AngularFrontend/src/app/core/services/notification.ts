import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import Swal from 'sweetalert2';

/**
 * NotificationService
 * Provides a global system for displaying dialog notifications (Success, Error, Info, Warning).
 * Replaces generic browser alerts and banner popups with premium SweetAlert2 popups.
 * Dialog titles and button labels are translated via ngx-translate; the message
 * itself is translated by the caller (or is a server-provided message).
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  // Maintained for backward compatibility, unused now
  private toastsSubject = new BehaviorSubject<any[]>([]);
  public toasts$ = this.toastsSubject.asObservable();

  constructor(private translate: TranslateService) {}

  private fire(title: string, message: string, icon: 'success' | 'error' | 'info' | 'warning') {
    Swal.fire({
      title: title,
      text: message,
      icon: icon,
      width: '360px', // More compact, elegant width
      confirmButtonText: this.translate.instant('notify.ok'),
      confirmButtonColor: '#0f766e', // Theme primary Teal color
      background: '#ffffff',
      color: '#1e293b',
      heightAuto: false // Prevents body shift/scrollbar jump issues
    });
  }

  /**
   * Displays a success dialog popup
   */
  success(message: string, title?: string) {
    this.fire(title ?? this.translate.instant('notify.success'), message, 'success');
  }

  /**
   * Displays an error dialog popup
   */
  error(message: string, title?: string) {
    this.fire(title ?? this.translate.instant('notify.error'), message, 'error');
  }

  /**
   * Displays an informational dialog popup
   */
  info(message: string, title?: string) {
    this.fire(title ?? this.translate.instant('notify.info'), message, 'info');
  }

  /**
   * Displays a warning dialog popup
   */
  warning(message: string, title?: string) {
    this.fire(title ?? this.translate.instant('notify.warning'), message, 'warning');
  }

  /**
   * Displays a confirmation dialog and returns a promise resolving to boolean
   */
  confirm(
    message: string,
    title?: string,
    confirmButtonText?: string,
    cancelButtonText?: string,
    isDestructive: boolean = true
  ): Promise<boolean> {
    return Swal.fire({
      title: title ?? this.translate.instant('notify.confirmTitle'),
      text: message,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: confirmButtonText ?? this.translate.instant('notify.yes'),
      cancelButtonText: cancelButtonText ?? this.translate.instant('notify.cancel'),
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
