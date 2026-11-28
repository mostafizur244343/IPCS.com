import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

export type SupportedLang = 'en' | 'bn';

/** localStorage key for the persisted language choice. */
export const LANG_STORAGE_KEY = 'ipcs-lang';

/**
 * LanguageService
 * Owns the current UI language: 'en' (default) or 'bn' (বাংলা).
 * The choice is persisted in localStorage and restored at startup,
 * and switching updates every translated string immediately (no reload).
 */
@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private translate = inject(TranslateService);

  /** Current language as a signal, so switchers can highlight the active one. */
  readonly currentLang = signal<SupportedLang>('en');

  constructor() {
    this.translate.addLangs(['en', 'bn']);
    const saved = localStorage.getItem(LANG_STORAGE_KEY);
    const lang: SupportedLang = saved === 'bn' ? 'bn' : 'en';
    this.currentLang.set(lang);
  }

  /**
   * Load the startup language file BEFORE the app renders (called by
   * APP_INITIALIZER). Without this, components render while the JSON is
   * still loading and raw keys (e.g. "auth.headline1") flash on screen.
   * A failed fetch never blocks startup — the app still starts (and
   * falls back to showing keys, same as before the fix).
   */
  init(): Promise<void> {
    return firstValueFrom(
      this.translate.use(this.currentLang()).pipe(catchError(() => of({})))
    ).then(() => undefined);
  }

  setLang(lang: SupportedLang): void {
    if (lang === this.currentLang()) return;
    localStorage.setItem(LANG_STORAGE_KEY, lang);
    this.currentLang.set(lang);
    this.translate.use(lang);
  }
}
