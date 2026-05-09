import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { PermissionService } from '../../../core/services/permission';
import { FormsModule } from '@angular/forms';

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
export class ProductListComponent implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  searchTerm: string = '';
  isLoading = false;

  constructor(
    private api: ApiService,
    public permService: PermissionService // Injected to check permissions in HTML
  ) {}

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.isLoading = true;
    this.api.get<any[]>('Product').subscribe({
      next: (data) => {
        this.products = data;
        this.filteredProducts = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading products', err);
        this.isLoading = false;
      }
    });
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredProducts = this.products.filter(p => 
      p.productName.toLowerCase().includes(term) || 
      p.sku.toLowerCase().includes(term)
    );
  }

  deleteProduct(id: number) {
    if (confirm('Are you sure you want to delete this product?')) {
      this.api.delete(`Product/${id}`).subscribe(() => {
        this.loadProducts();
      });
    }
  }
}
