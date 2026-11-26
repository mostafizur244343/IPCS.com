import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { PagedResult } from '../../../core/models/paged-result';

/**
 * PurchaseListComponent
 * Displays a list of all purchase transactions made with suppliers.
 * Server-side paging: only one page is fetched at a time; search is sent
 * to the server (debounced) instead of filtering a full-table download.
 */
@Component({
  selector: 'app-purchase-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent],
  templateUrl: './purchase-list.html'
})
export class PurchaseListComponent implements OnInit {
  purchases: any[] = []; // Full list of purchase records
  filteredPurchases: any[] = [];
  searchTerm: string = '';
  isLoading = false;

  // Server-side paging state
  page = 1;
  pageSize = 20;
  totalCount = 0;
  private searchTimer: any;

  // Details state
  selectedPurchaseForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(purchase: any) {
    this.api.get<any>(`Purchase/${purchase.purchaseId}`).subscribe({
      next: (fullData) => {
        this.selectedPurchaseForDetails = this.withSupplierName(fullData);
      },
      error: (err) => {
        console.error('Error loading purchase details', err);
        this.selectedPurchaseForDetails = purchase;
      }
    });
  }

  /**
   * The API returns PurchaseMaster entities with the supplier as a nested
   * `supplier` object ({ supplierName, ... }); the table and the details
   * modal read a flat `supplierName`. Flatten it here so the Supplier
   * column actually shows a name instead of staying blank.
   * Same for the detail rows: the modal reads flat `productName` /
   * `lotNo` / `uomName`, but the API returns nested `product` / `batchNo`
   * / `uom` — flatten those too.
   */
  private withSupplierName(p: any): any {
    const details = (p?.purchaseDetails ?? []).map((d: any) => ({
      ...d,
      productName: d?.product?.productName ?? d?.productName ?? '',
      lotNo: d?.batchNo ?? d?.lotNo ?? '',
      uomName: d?.uom?.uomName ?? d?.uom?.UOMName ?? d?.uomName ?? ''
    }));
    return { ...p, supplierName: p?.supplier?.supplierName ?? p?.supplierName ?? '', purchaseDetails: details };
  }

  closeDetails() {
    this.selectedPurchaseForDetails = null;
  }

  ngOnInit() {
    this.loadPurchases();
  }

  /**
   * Fetches one page of purchase records from the API
   */
  loadPurchases() {
    this.isLoading = true;
    const search = this.searchTerm.trim();
    const query = `Purchase?page=${this.page}&pageSize=${this.pageSize}` + (search ? `&search=${encodeURIComponent(search)}` : '');
    this.api.get<PagedResult<any>>(query).subscribe({
      next: (res) => {
        this.purchases = res.items.map((p: any) => this.withSupplierName(p));
        this.filteredPurchases = this.purchases;
        this.totalCount = res.totalCount;
        // If the last item of a page was deleted, step back a page.
        if (res.items.length === 0 && this.page > 1) {
          this.page--;
          this.loadPurchases();
          return;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading purchases', err);
        this.isLoading = false;
      }
    });
  }

  /**
   * Server-side search by Supplier Invoice or Supplier Name (debounced).
   */
  onSearch() {
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.page = 1;
      this.loadPurchases();
    }, 400);
  }

  onPageChange(page: number) {
    this.page = page;
    this.loadPurchases();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.page = 1;
    this.loadPurchases();
  }

  /**
   * Deletes a purchase (Soft Delete)
   */
  deletePurchase(id: number) {
    if (confirm('Are you sure you want to delete this purchase? This will reduce stock levels.')) {
      this.api.delete(`Purchase/${id}`).subscribe(() => {
        this.loadPurchases();
      });
    }
  }

  /**
   * Receives a shipment for a pending purchase
   */
  receiveShipment(id: number) {
    if (confirm('Mark this shipment as received? This will update the inventory.')) {
      this.api.post(`Purchase/receive-shipment/${id}`, {}).subscribe(() => {
        this.loadPurchases();
      });
    }
  }
}
