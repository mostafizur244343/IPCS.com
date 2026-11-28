import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';

/**
 * PaginationComponent
 * Shared pager for server-side paged lists: page numbers with a sliding
 * window, prev/next buttons, a "Showing X–Y of Z" label and a page-size
 * selector. The parent owns the data; this component only emits the
 * requested page / page size.
 */
@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  template: `
    <div class="pagination-bar" *ngIf="totalCount > 0">
      <div class="pagination-info">
        {{ 'pagination.showing' | translate:{start: startItem, end: endItem, total: totalCount} }}
      </div>
      <div class="pagination-controls">
        <button class="page-btn" [disabled]="page <= 1" (click)="goTo(page - 1)" [attr.title]="'pagination.prevPage' | translate">
          &lsaquo; {{ 'pagination.prev' | translate }}
        </button>
        <ng-container *ngFor="let p of pages">
          <button *ngIf="p !== '...'" class="page-btn" [class.active]="p === page" (click)="goTo(p)">{{ p }}</button>
          <span *ngIf="p === '...'" class="page-ellipsis">&hellip;</span>
        </ng-container>
        <button class="page-btn" [disabled]="page >= totalPages" (click)="goTo(page + 1)" [attr.title]="'pagination.nextPage' | translate">
          {{ 'pagination.next' | translate }} &rsaquo;
        </button>
      </div>
      <div class="pagination-size">
        <label for="page-size-select">{{ 'pagination.perPage' | translate }}</label>
        <select id="page-size-select" [ngModel]="pageSize" (ngModelChange)="onPageSizeChange($event)">
          <option *ngFor="let size of pageSizeOptions" [value]="size">{{ size }}</option>
        </select>
      </div>
    </div>
  `,
  styles: [`
    .pagination-bar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 12px;
      padding: 14px 4px 4px;
    }
    .pagination-info { font-size: 13px; color: var(--text-muted, #64748b); }
    .pagination-controls { display: flex; align-items: center; gap: 6px; }
    .page-btn {
      min-width: 34px;
      height: 34px;
      padding: 0 10px;
      border-radius: 8px;
      border: 1px solid #e2e8f0;
      background: #fff;
      color: #334155;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.18s ease;
    }
    .page-btn:hover:not(:disabled) { border-color: var(--primary, #0f766e); color: var(--primary, #0f766e); transform: translateY(-1px); box-shadow: 0 4px 10px rgba(15, 118, 110, 0.12); }
    .page-btn.active { background: var(--primary, #0f766e); border-color: var(--primary, #0f766e); color: #fff; box-shadow: 0 4px 10px rgba(15, 118, 110, 0.25); }
    .page-btn:active:not(:disabled) { transform: scale(0.94); }
    .page-btn:disabled { opacity: 0.45; cursor: not-allowed; }
    .page-ellipsis { color: #94a3b8; padding: 0 2px; }
    .pagination-size { display: flex; align-items: center; gap: 8px; font-size: 13px; color: var(--text-muted, #64748b); }
    .pagination-size select {
      height: 34px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      background: #fff;
      padding: 0 8px;
      font-size: 13px;
      cursor: pointer;
    }
  `]
})
export class PaginationComponent {
  @Input() page = 1;
  @Input() pageSize = 20;
  @Input() totalCount = 0;
  @Input() pageSizeOptions: number[] = [10, 20, 50, 100];

  @Output() pageChange = new EventEmitter<number>();
  @Output() pageSizeChange = new EventEmitter<number>();

  get totalPages(): number {
    return this.pageSize > 0 ? Math.ceil(this.totalCount / this.pageSize) : 0;
  }

  get startItem(): number {
    return this.totalCount === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get endItem(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }

  /** Sliding window of page numbers: all pages up to 7, otherwise 1 … p-1 p p+1 … last. */
  get pages(): (number | '...')[] {
    const total = this.totalPages;
    const current = this.page;
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }
    const window: (number | '...')[] = [1];
    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);
    if (start > 2) window.push('...');
    for (let p = start; p <= end; p++) window.push(p);
    if (end < total - 1) window.push('...');
    window.push(total);
    return window;
  }

  goTo(p: number): void {
    if (p >= 1 && p <= this.totalPages && p !== this.page) {
      this.pageChange.emit(p);
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSizeChange.emit(Number(size));
  }
}
