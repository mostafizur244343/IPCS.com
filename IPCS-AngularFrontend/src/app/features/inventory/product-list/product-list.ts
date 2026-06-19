import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PermissionService } from '../../../core/services/permission';
import { FormsModule } from '@angular/forms';
import { BaseFeatureComponent } from '../../../shared/components/base-feature';
import { environment } from '../../../../environments/environment';

/**
 * ProductListComponent
 * Displays a list of inventory products with RBAC-protected action buttons.
 */
@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-list.html'
})
export class ProductListComponent extends BaseFeatureComponent implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm: string = '';
  
  public permService = inject(PermissionService);

  // Details state
  selectedProductForDetails: any = null;

  override ngOnInit() {
    this.api.get<any[]>('Product').subscribe({
      next: (data) => {
        this.products = data;
        this.filteredProducts = data;
      },
      error: (err) => this.handleError(err)
    });
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
    this.api.get<any[]>('Product').subscribe({
      next: (data) => {
        this.products = data;
        this.filteredProducts = data;
        this.isLoading = false;
      },
      error: (err) => this.handleError(err)
    });
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredProducts = this.products.filter(p => 
      p.productName.toLowerCase().includes(term) || 
      (p.sku && p.sku.toLowerCase().includes(term))
    );
  }

  async deleteProduct(id: number) {
    const confirmed = await this.confirmDelete('Delete Product?', 'This product will be permanently removed.');
    if (confirmed) {
      this.isLoading = true;
      this.api.delete(`Product/${id}`).subscribe({
        next: () => {
          this.notification.success('Product deleted successfully.');
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
