import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { PagedResult } from '../../../core/models/paged-result';
import { TranslatePipe } from '@ngx-translate/core';

/**
 * StockLedgerComponent
 * Provides a comprehensive history of all stock movements.
 * Includes Purchases, Sales, Transfers, and Adjustments.
 * Server-side paging: only one page is fetched at a time (the ledger is
 * the fastest-growing table); search is sent to the server (debounced).
 */
@Component({
  selector: 'app-stock-ledger',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, TranslatePipe],
  templateUrl: './stock-ledger.html'
})
export class StockLedgerComponent implements OnInit {
  ledgerEntries: any[] = [];
  filteredEntries: any[] = [];
  searchTerm: string = '';
  isLoading = false;

  // Server-side paging state
  page = 1;
  pageSize = 20;
  totalCount = 0;
  private searchTimer: any;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadLedger();
  }

  /**
   * Fetches one page of the stock movement history for the branch
   * (server orders by LedgerId desc = newest first).
   */
  loadLedger() {
    this.isLoading = true;
    const branchId = 1; // Default branch
    const search = this.searchTerm.trim();
    const query = `StockLedger/branch/${branchId}?page=${this.page}&pageSize=${this.pageSize}` + (search ? `&search=${encodeURIComponent(search)}` : '');
    this.api.get<PagedResult<any>>(query).subscribe({
      next: (res) => {
        this.ledgerEntries = res.items;
        this.filteredEntries = res.items;
        this.totalCount = res.totalCount;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Server-side search by medicine name, batch/lot, reference number
   * or transaction type (debounced).
   */
  onSearch() {
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.page = 1;
      this.loadLedger();
    }, 400);
  }

  onPageChange(page: number) {
    this.page = page;
    this.loadLedger();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.page = 1;
    this.loadLedger();
  }
}
