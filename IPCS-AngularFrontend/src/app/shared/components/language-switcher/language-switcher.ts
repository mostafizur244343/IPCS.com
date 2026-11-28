import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language';

/**
 * LanguageSwitcherComponent
 * Small বাংলা / English segmented control. Used in the dashboard header
 * (visible on every page after login) and on the login page topbar.
 * Switching updates all visible text immediately, without a reload.
 */
@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="lang-switcher" role="group" aria-label="Language / ভাষা">
      <button type="button" [class.active]="langSvc.currentLang() === 'bn'" (click)="langSvc.setLang('bn')">বাংলা</button>
      <button type="button" [class.active]="langSvc.currentLang() === 'en'" (click)="langSvc.setLang('en')">English</button>
    </div>
  `,
  styles: [`
    .lang-switcher {
      display: inline-flex;
      gap: 2px;
      padding: 3px;
      border-radius: 999px;
      background: rgba(15, 118, 110, 0.08);
      border: 1px solid rgba(15, 118, 110, 0.15);
    }
    .lang-switcher button {
      border: 0;
      background: transparent;
      color: #475569;
      font-size: 12px;
      font-weight: 600;
      line-height: 1;
      padding: 7px 12px;
      border-radius: 999px;
      cursor: pointer;
      white-space: nowrap;
      transition: background 0.18s, color 0.18s, box-shadow 0.18s;
    }
    .lang-switcher button:hover { color: #0f766e; }
    .lang-switcher button.active {
      background: #0f766e;
      color: #fff;
      box-shadow: 0 2px 6px rgba(15, 118, 110, 0.3);
    }
  `]
})
export class LanguageSwitcherComponent {
  langSvc = inject(LanguageService);
}
