import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { NotificationService } from '../../../core/services/notification';

/**
 * Trash column definition (passed via route `data`).
 * field: dot-path into the row object, e.g. 'supplier.supplierName'.
 * fallbackField: optional second path tried when the first is empty
 *   (e.g. the Product DTO serializes SKU as `sKU`, older rows as `sku`).
 * format: 'date' | 'money' | 'bool' | 'count' | 'text' (default 'text').
 */
export interface TrashColumn {
  field: string;
  label: string;
  fallbackField?: string;
  format?: 'date' | 'money' | 'bool' | 'count' | 'text';
  trueText?: string;
  falseText?: string;
  bold?: boolean;
}

export interface TrashConfig {
  title: string;
  subtitle: string;
  trashEndpoint: string;
  restoreEndpoint: string;
  idField: string;
  backRoute: string;
  columns: TrashColumn[];
}

/**
 * TrashListComponent
 * Generic trash view used by every module: loads the module's deleted
 * records from `<module>/trash`, shows them in the same table style as
 * the module's list page, and restores a row via
 * POST `<module>/restore/{id}`.
 *
 * Server guard errors (400 with a message, e.g. "not enough stock to
 * restore") are shown globally by the auth interceptor, so this
 * component only resets its loading state on error and never shows a
 * second, generic error popup.
 */
@Component({
  selector: 'app-trash-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './trash-list.html',
})
export class TrashListComponent implements OnInit {
  config!: TrashConfig;
  rows: any[] = [];
  filteredRows: any[] = [];
  searchTerm = '';
  isLoading = false;

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private notification: NotificationService,
  ) {}

  ngOnInit() {
    this.config = this.route.snapshot.data['trash'] as TrashConfig;
    this.loadTrash();
  }

  loadTrash() {
    this.isLoading = true;
    this.api.get<any[]>(this.config.trashEndpoint).subscribe({
      next: (data) => {
        this.rows = data || [];
        this.filteredRows = this.rows;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  onSearch() {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) {
      this.filteredRows = this.rows;
      return;
    }
    this.filteredRows = this.rows.filter((row) =>
      this.config.columns.some((col) =>
        String(this.displayValue(row, col)).toLowerCase().includes(term),
      ),
    );
  }

  /** Resolves a dot-path like 'supplier.supplierName' on a row object. */
  private resolve(row: any, path: string): any {
    return path
      .split('.')
      .reduce((obj, key) => (obj == null ? undefined : obj[key]), row);
  }

  displayValue(row: any, col: TrashColumn): string {
    let raw = this.resolve(row, col.field);
    if ((raw === undefined || raw === null || raw === '') && col.fallbackField) {
      raw = this.resolve(row, col.fallbackField);
    }
    if (raw === undefined || raw === null || raw === '') return '-';
    switch (col.format) {
      case 'date': {
        const d = new Date(raw);
        return isNaN(d.getTime())
          ? '-'
          : d.toLocaleDateString('en-GB', {
              day: '2-digit',
              month: 'short',
              year: 'numeric',
            });
      }
      case 'money':
        return Number(raw).toFixed(2);
      case 'bool':
        return raw ? (col.trueText ?? 'Yes') : (col.falseText ?? 'No');
      case 'count':
        return Array.isArray(raw) ? String(raw.length) : String(raw);
      default:
        return String(raw);
    }
  }

  /**
   * Restores a deleted record. The backend re-applies the record's
   * stock and accounting effects; if that is not possible (e.g. the
   * goods were sold on), it answers 400 with a clear message, which
   * the auth interceptor shows to the user.
   */
  async restoreRow(row: any) {
    const id = this.resolve(row, this.config.idField);
    const confirmed = await this.notification.confirm(
      'This will restore the record and re-apply its stock and accounting effects.',
      'Restore this record?',
      'Yes, restore it',
      'Cancel',
      false,
    );
    if (!confirmed) return;
    this.isLoading = true;
    this.api.post<any>(`${this.config.restoreEndpoint}/${id}`, {}).subscribe({
      next: (res) => {
        this.notification.success(res?.message || 'Restored successfully.');
        this.loadTrash();
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }
}
