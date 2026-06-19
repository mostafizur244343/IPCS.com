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
    <!-- Main View -->
    <router-outlet></router-outlet>
  `,
  styles: [``]
})
export class AppComponent {
  constructor(public notification: NotificationService) {
    // Prevent mouse wheel from changing values on focused number inputs globally,
    // by blurring the input, ensuring page scroll is not blocked.
    document.addEventListener('wheel', () => {
      const activeEl = document.activeElement;
      if (
        activeEl &&
        activeEl.tagName === 'INPUT' &&
        (activeEl as HTMLInputElement).type === 'number'
      ) {
        (activeEl as HTMLInputElement).blur();
      }
    });
  }
}
