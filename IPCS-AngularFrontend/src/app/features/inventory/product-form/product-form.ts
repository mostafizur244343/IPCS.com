import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * ProductFormComponent
 * This component is used for adding new products or editing existing ones.
 */
@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './product-form.html'
})
export class ProductFormComponent implements OnInit {
  // Product Data Model
  product: any = {
    productName: '',
    sku: '',
    strength: '',
    mrp: 0,
    salesPrice: 0,
    baseUomId: null,
    categoryId: null,
    brandId: null,
    uomId: null,
    genericId: null,
    locationId: null,
    openingQuantity: 0,
    openingCostPrice: 0,
    reorderLevel: 5,
    minOrderQuantity: 1,
    isService: false,
    isActive: true
  };

  // Lists for dropdown options (Lookup Data)
  categories: any[] = [];
  manufacturers: any[] = [];
  units: any[] = [];
  generics: any[] = [];
  locations: any[] = [];

  isEditMode = false;
  isLoading = false;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadLookups(); // Loading all lookup data for dropdowns
    
    // Check if an ID exists in the URL to determine if we're in edit mode
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.loadProduct(id);
    }
  }

  /**
   * Function to fetch all lookup lists from the API
   */
  loadLookups() {
    this.api.get<any[]>('Category').subscribe(data => this.categories = data);
    this.api.get<any[]>('Manufacturer').subscribe(data => this.manufacturers = data);
    this.api.get<any[]>('UOM').subscribe(data => this.units = data);
    this.api.get<any[]>('GenericInfo').subscribe(data => this.generics = data);
    this.api.get<any[]>('StoreLocation').subscribe(data => this.locations = data);
  }

  /**
   * Loads existing product data for editing
   */
  loadProduct(id: number) {
    this.isLoading = true;
    this.api.get<any>(`Product/${id}`).subscribe(data => {
      this.product = data;
      this.isLoading = false;
    });
  }

  /**
   * Function called on form submission
   */
  onSubmit() {
    this.isLoading = true;
    if (this.isEditMode) {
      // Request to update an existing product
      this.api.put(`Product/${this.product.productId}`, this.product).subscribe({
        next: () => this.router.navigate(['/dashboard/inventory']),
        error: (err) => { alert(err.error?.message || 'Update failed'); this.isLoading = false; }
      });
    } else {
      // Request to create a new product
      this.api.post('Product', this.product).subscribe({
        next: () => this.router.navigate(['/dashboard/inventory']),
        error: (err) => { alert(err.error?.message || 'Create failed'); this.isLoading = false; }
      });
    }
  }
}
