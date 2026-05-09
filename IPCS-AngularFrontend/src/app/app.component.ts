import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NotificationService } from './core/services/notification';

/**
 * Root AppComponent
 * Includes the main router outlet and the global notification (toast) container.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  template: `
    <!-- Global Notification (Toast) Container -->
    <div class="toast-container">
      <div *ngFor="let toast of notification.toasts$ | async" 
           class="toast" 
           [class.success]="toast.type === 'success'"
           [class.error]="toast.type === 'error'"
           [class.info]="toast.type === 'info'"
           (click)="notification.remove(toast.id)">
        <div class="toast-icon">
          <span *ngIf="toast.type === 'success'">✓</span>
          <span *ngIf="toast.type === 'error'">✕</span>
          <span *ngIf="toast.type === 'info'">ℹ</span>
        </div>
        <div class="toast-content">{{ toast.message }}</div>
      </div>
    </div>

    <!-- Main View -->
    <router-outlet></router-outlet>
  `,
  styles: [`
    .toast-container {
      position: fixed; top: 24px; right: 24px; z-index: 9999;
      display: flex; flex-direction: column; gap: 12px;
    }
    .toast {
      min-width: 300px; padding: 16px; border-radius: 12px;
      background: white; box-shadow: 0 10px 15px -3px rgba(0,0,0,0.1);
      display: flex; align-items: center; gap: 12px;
      cursor: pointer; animation: slideIn 0.3s ease-out;
      border-left: 6px solid #cbd5e1;
    }
    .toast.success { border-left-color: #10b981; }
    .toast.error { border-left-color: #ef4444; }
    .toast.info { border-left-color: #3b82f6; }
    
    .toast-icon {
      width: 24px; height: 24px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      font-size: 14px; font-weight: bold; color: white;
    }
    .toast.success .toast-icon { background: #10b981; }
    .toast.error .toast-icon { background: #ef4444; }
    .toast.info .toast-icon { background: #3b82f6; }

    .toast-content { font-size: 14px; color: #1e293b; font-weight: 500; }

    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
  `]
})
export class AppComponent {
  constructor(public notification: NotificationService) {}
}
