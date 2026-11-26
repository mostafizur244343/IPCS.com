import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { PagedResult } from '../../../core/models/paged-result';

/**
 * SalesListComponent
 * Used to display a list of all sales invoices/transactions.
 * Server-side paging: only one page is fetched at a time; search is sent
 * to the server (debounced) instead of filtering a full-table download.
 */
@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './sales-list.html'
})
export class SalesListComponent implements OnInit {
  sales: any[] = []; // List of sales invoices
  filteredSales: any[] = [];
  searchTerm: string = '';
  isLoading = false;

  // Server-side paging state
  page = 1;
  pageSize = 20;
  totalCount = 0;
  private searchTimer: any;

  // Details state
  selectedSalesForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(sale: any) {
    this.api.get<any>(`Sales/${sale.salesId}`).subscribe({
      next: (fullData) => {
        this.selectedSalesForDetails = fullData;
      },
      error: (err) => {
        console.error('Error loading sales details', err);
        this.selectedSalesForDetails = sale;
      }
    });
  }

  closeDetails() {
    this.selectedSalesForDetails = null;
  }

  ngOnInit() {
    this.loadSales();
  }

  /**
   * Fetches one page of sales records from the API
   */
  loadSales() {
    this.isLoading = true;
    const search = this.searchTerm.trim();
    const query = `Sales?page=${this.page}&pageSize=${this.pageSize}` + (search ? `&search=${encodeURIComponent(search)}` : '');
    this.api.get<PagedResult<any>>(query).subscribe({
      next: (res) => {
        this.sales = res.items;
        this.filteredSales = res.items;
        this.totalCount = res.totalCount;
        // If the last item of a page was deleted, step back a page.
        if (res.items.length === 0 && this.page > 1) {
          this.page--;
          this.loadSales();
          return;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading sales', err);
        this.isLoading = false;
      }
    });
  }

  /**
   * Server-side search by Invoice Number or Customer Name (debounced).
   */
  onSearch() {
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.page = 1;
      this.loadSales();
    }, 400);
  }

  onPageChange(page: number) {
    this.page = page;
    this.loadSales();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.page = 1;
    this.loadSales();
  }

  /**
   * Deletes a sale (Soft Delete)
   */
  deleteSale(id: number) {
    if (confirm('Are you sure you want to delete this sale? This will revert stock levels.')) {
      this.api.delete(`Sales/${id}`).subscribe(() => {
        this.loadSales();
      });
    }
  }
}
