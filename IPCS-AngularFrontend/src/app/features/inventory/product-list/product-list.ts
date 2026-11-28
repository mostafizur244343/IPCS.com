import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PermissionService } from '../../../core/services/permission';
import { FormsModule } from '@angular/forms';
import { BaseFeatureComponent } from '../../../shared/components/base-feature';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { PagedResult } from '../../../core/models/paged-result';
import { environment } from '../../../../environments/environment';
import { TranslatePipe } from '@ngx-translate/core';

/**
 * ProductListComponent
 * Displays a list of inventory products with RBAC-protected action buttons.
 * Server-side paging: only one page is fetched at a time; search is sent
 * to the server (debounced) instead of filtering a full-table download.
 */
@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PaginationComponent, TranslatePipe],
  templateUrl: './product-list.html'
})
export class ProductListComponent extends BaseFeatureComponent implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm: string = '';

  // Server-side paging state
  page = 1;
  pageSize = 20;
  totalCount = 0;
  private searchTimer: any;
  
  public permService = inject(PermissionService);

  // Details state
  selectedProductForDetails: any = null;

  override ngOnInit() {
    this.loadProducts();
  }

  showDetails(product: any) {
    this.isLoading = true;
    this.api.get<any>(`Product/${product.productId}`).subscribe({
      next: (fullProduct) => {
        this.selectedProductForDetails = fullProduct;
        this.isLoading = false;
      },
      error: (err) => {
        this.handleError(err);
        this.selectedProductForDetails = product;
      }
    });
  }

  closeDetails() {
    this.selectedProductForDetails = null;
  }

  loadProducts() {
    this.isLoading = true;
    const search = this.searchTerm.trim();
    const query = `Product?page=${this.page}&pageSize=${this.pageSize}` + (search ? `&search=${encodeURIComponent(search)}` : '');
    this.api.get<PagedResult<any>>(query).subscribe({
      next: (res) => {
        this.products = res.items;
        this.filteredProducts = res.items;
        this.totalCount = res.totalCount;
        // If the last item of a page was deleted, step back a page.
        if (res.items.length === 0 && this.page > 1) {
          this.page--;
          this.loadProducts();
          return;
        }
        this.isLoading = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  onSearch() {
    // Debounced server-side search: reload page 1 from the server.
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.page = 1;
      this.loadProducts();
    }, 400);
  }

  onPageChange(page: number) {
    this.page = page;
    this.loadProducts();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.page = 1;
    this.loadProducts();
  }

  async deleteProduct(id: number) {
    const confirmed = await this.confirmDelete(this.translate.instant('inventory.products.deleteConfirmTitle'), this.translate.instant('inventory.products.deleteConfirmText'));
    if (confirmed) {
      this.isLoading = true;
      this.api.delete(`Product/${id}`).subscribe({
        next: () => {
          this.notification.success(this.translate.instant('inventory.products.deleted'));
          this.loadProducts();
        },
        error: (err) => this.handleError(err)
      });
    }
  }

  getImageUrl(path: string): string {
    if (!path) return '';
    if (path.startsWith('http') || path.startsWith('data:')) {
      return path;
    }
    return `${environment.fileServerUrl}${path}`;
  }
}
