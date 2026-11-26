import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

let logoInstance = 0;

/**
 * LogoComponent — PharmaCare brand mark.
 * Inline SVG (mortar & pestle with a care cross) on a teal gradient tile.
 * No external image, no network fetch — safe to use on the login page
 * and in the dashboard sidebar. The gradient id is unique per instance
 * so several logos can live on one page.
 */
@Component({
  selector: 'app-logo',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="pc-logo" [style.width.px]="size" [style.height.px]="size" role="img" aria-label="PharmaCare logo">
      <svg [attr.width]="size" [attr.height]="size" viewBox="0 0 48 48" fill="none" aria-hidden="true">
        <defs>
          <linearGradient [id]="gradId" x1="0" y1="0" x2="48" y2="48" gradientUnits="userSpaceOnUse">
            <stop stop-color="#14b8a6" />
            <stop offset="0.55" stop-color="#0d9488" />
            <stop offset="1" stop-color="#0f766e" />
          </linearGradient>
        </defs>
        <rect width="48" height="48" rx="12" [attr.fill]="'url(#' + gradId + ')'" />
        <!-- care cross -->
        <path d="M15 9.5v6M12 12.5h6" stroke="#99f6e4" stroke-width="2.4" stroke-linecap="round" />
        <!-- pestle -->
        <path d="M30.5 8.5 37 15" stroke="#ffffff" stroke-width="3.4" stroke-linecap="round" />
        <!-- mortar bowl -->
        <path d="M11 24h26l-3.2 8.6a5 5 0 0 1-4.7 3.4H18.9a5 5 0 0 1-4.7-3.4L11 24Z" fill="#ffffff" />
        <!-- mortar base -->
        <path d="M19 40.5h10" stroke="#99f6e4" stroke-width="2.6" stroke-linecap="round" />
      </svg>
    </span>
  `,
  styles: [`
    .pc-logo {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      flex: none;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 6px 16px rgba(15, 118, 110, 0.28);
    }
    .pc-logo svg { display: block; }
  `],
})
export class LogoComponent {
  @Input() size = 40;
  gradId = `pcLogoGrad${++logoInstance}`;
}
