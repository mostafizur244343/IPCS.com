import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

let logoInstance = 0;

/**
 * LogoComponent — PharmaCare brand mark (v2).
 * A white pill/capsule tilted diagonally on a two-tone emerald gradient
 * tile, with a small care cross at the top-right. The bright emerald
 * gradient lives ONLY here (and in the login aurora band) — the rest of
 * the app uses deep emerald, so the mark stays the one bright accent.
 * Inline SVG, no external image, no network fetch. The gradient id is
 * unique per instance so several logos can live on one page.
 */
@Component({
  selector: 'app-logo',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="pc-logo" [style.width.px]="size" [style.height.px]="size" role="img" aria-label="PharmaCare logo">
      <svg [attr.width]="size" [attr.height]="size" viewBox="0 0 48 48" fill="none" aria-hidden="true">
        <defs>
          <linearGradient [id]="gradId" x1="6" y1="4" x2="42" y2="44" gradientUnits="userSpaceOnUse">
            <stop stop-color="#34D399" />
            <stop offset="0.55" stop-color="#10B981" />
            <stop offset="1" stop-color="#047857" />
          </linearGradient>
          <radialGradient [id]="hlId" cx="0.32" cy="0.22" r="0.9" gradientUnits="objectBoundingBox">
            <stop stop-color="#ffffff" stop-opacity="0.28" />
            <stop offset="0.55" stop-color="#ffffff" stop-opacity="0" />
          </radialGradient>
        </defs>
        <rect width="48" height="48" rx="12" [attr.fill]="'url(#' + gradId + ')'" />
        <rect width="48" height="48" rx="12" [attr.fill]="'url(#' + hlId + ')'" />
        <rect x="0.75" y="0.75" width="46.5" height="46.5" rx="11.5" stroke="#ffffff" stroke-opacity="0.28" stroke-width="1.5" />
        <!-- pill: white capsule, tilted diagonally, with a divider line -->
        <g transform="rotate(-45 24 25)">
          <rect x="19.5" y="10" width="9" height="30" rx="4.5" fill="#ffffff" />
          <line x1="19.5" y1="25" x2="28.5" y2="25" stroke="#047857" stroke-width="2" stroke-linecap="round" />
        </g>
        <!-- care cross, top-right -->
        <path d="M38.5 9v5M36 11.5h5" stroke="#D1FAE5" stroke-width="2.2" stroke-linecap="round" />
      </svg>
    </span>
  `,
  styles: [`
    .pc-logo {
      display: flex;
      align-items: center;
      justify-content: center;
      flex: none;
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 6px 16px rgba(4, 120, 87, 0.25);
    }
    .pc-logo svg { display: block; }
  `],
})
export class LogoComponent {
  @Input() size = 40;
  gradId = `pcLogoGrad${++logoInstance}`;
  hlId = `pcLogoHl${logoInstance}`;
}
