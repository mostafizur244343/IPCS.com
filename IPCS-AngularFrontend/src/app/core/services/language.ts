import { Injectable, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

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
    this.translate.use(lang);
  }

  setLang(lang: SupportedLang): void {
    if (lang === this.currentLang()) return;
    localStorage.setItem(LANG_STORAGE_KEY, lang);
    this.currentLang.set(lang);
    this.translate.use(lang);
  }
}
