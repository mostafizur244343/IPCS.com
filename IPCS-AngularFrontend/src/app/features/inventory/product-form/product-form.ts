import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { NotificationService } from '../../../core/services/notification';
import { ImageUploadComponent } from '../../../shared/image-upload/image-upload';
import { lastValueFrom } from 'rxjs';

/**
 * ProductFormComponent
 * This component is used for adding new products or editing existing ones.
 */
@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ImageUploadComponent],
  templateUrl: './product-form.html'
})
export class ProductFormComponent implements OnInit {
  // Product Data Model
  product: any = {
    productName: '',
    sku: '',
    strength: '',
    picturePath: '',
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
  isSalesPriceSameAsMRP = false;

  // Lists for dropdown options (Lookup Data)
  categories: any[] = [];
  manufacturers: any[] = [];
  units: any[] = [];
  generics: any[] = [];
  locations: any[] = [];

  isEditMode = false;
  isLoading = false;
  isUploading = false;

  // Toggle states for inline creation
  isNewGeneric = false;
  isNewCategory = false;
  isNewManufacturer = false;
  isNewLocation = false;
  isNewUom = false;

  // New item values
  newGenericName = '';
  newCategoryName = '';
  newManufacturerName = '';
  newLocationName = '';
  newUomName = '';

  toggleNewGeneric() {
    this.isNewGeneric = !this.isNewGeneric;
    if (this.isNewGeneric) {
      this.product.genericId = null;
    }
  }
  toggleNewCategory() {
    this.isNewCategory = !this.isNewCategory;
    if (this.isNewCategory) {
      this.product.categoryId = null;
    }
  }
  toggleNewManufacturer() {
    this.isNewManufacturer = !this.isNewManufacturer;
    if (this.isNewManufacturer) {
      this.product.brandId = null;
    }
  }
  toggleNewLocation() {
    this.isNewLocation = !this.isNewLocation;
    if (this.isNewLocation) {
      this.product.locationId = null;
    }
  }
  toggleNewUom() {
    this.isNewUom = !this.isNewUom;
    if (this.isNewUom) {
      this.product.uomId = null;
    }
  }

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private notification: NotificationService
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

  async onImageSelected(file: File) {
    this.isUploading = true;
    try {
      this.api.uploadFile(file).subscribe({
        next: (response: any) => {
          const filePath = response.filePath || response.FilePath;
          this.product.picturePath = filePath;
          this.isUploading = false;
          this.notification.success('Image uploaded successfully');
        },
        error: (err) => {
          console.error('Upload failed:', err);
          this.notification.error('Image upload failed: ' + (err.error?.Message || err.message));
          this.isUploading = false;
        }
      });
    } catch (err) {
      console.error('Upload error:', err);
      this.notification.error('Image upload failed');
      this.isUploading = false;
    }
  }

  onImageRemoved() {
    this.product.picturePath = '';
  }

  /**
   * Syncs Sales Price with MRP if checkbox is checked
   */
  onPriceSync() {
    if (this.isSalesPriceSameAsMRP) {
      this.product.salesPrice = this.product.mrp;
    }
  }

  onMRPInput() {
    if (this.isSalesPriceSameAsMRP) {
      this.product.salesPrice = this.product.mrp;
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

  onSubmit() {
    // Model validation checks
    const missing: string[] = [];
    let firstFocusElement: HTMLElement | null = null;

    const checkField = (value: any, isNew: boolean, newName: string, label: string, selector: string) => {
      const isEmpty = (val: any) => val === undefined || val === null || val === '' || val === 'null' || val === 0;
      if (isNew) {
        if (!newName || !newName.trim()) {
          missing.push(label);
          if (!firstFocusElement) firstFocusElement = document.querySelector(selector);
        }
      } else {
        if (isEmpty(value)) {
          missing.push(label);
          if (!firstFocusElement) firstFocusElement = document.querySelector(selector);
        }
      }
    };

    // 1. Product Name validation
    if (!this.product.productName || !this.product.productName.trim()) {
      missing.push('Product Name');
      if (!firstFocusElement) firstFocusElement = document.querySelector('input[name="productName"]');
    }
    // 2. Generic Name validation
    checkField(this.product.genericId, this.isNewGeneric, this.newGenericName, 'Generic Name', this.isNewGeneric ? 'input[name="newGenericName"]' : 'select[name="genericId"]');
    // 3. Category validation
    checkField(this.product.categoryId, this.isNewCategory, this.newCategoryName, 'Category', this.isNewCategory ? 'input[name="newCategoryName"]' : 'select[name="categoryId"]');
    // 4. Manufacturer/Brand validation
    checkField(this.product.brandId, this.isNewManufacturer, this.newManufacturerName, 'Manufacturer / Brand', this.isNewManufacturer ? 'input[name="newManufacturerName"]' : 'select[name="brandId"]');
    // 5. Base Unit validation
    checkField(this.product.uomId, this.isNewUom, this.newUomName, 'Base Unit (UOM)', this.isNewUom ? 'input[name="newUomName"]' : 'select[name="uomId"]');
    // 6. Sales Price validation
    if (this.product.salesPrice === undefined || this.product.salesPrice === null || this.product.salesPrice === '') {
      missing.push('Sales Price');
      if (!firstFocusElement) firstFocusElement = document.querySelector('input[name="salesPrice"]');
    }

    if (missing.length > 0) {
      this.notification.error(`Please fill in the required fields: ${missing.join(', ')}`);
      
      // Highlight invalid inputs dynamically
      const formElement = document.querySelector('form');
      if (formElement) {
        const requiredElements = formElement.querySelectorAll('input[required], select[required]');
        requiredElements.forEach(el => {
          const field = el as HTMLInputElement | HTMLSelectElement;
          const val = field.value;
          if (!val || val.trim() === '' || val === 'null' || val.includes('null')) {
            field.classList.add('ng-touched');
            field.classList.add('ng-invalid');
          }
        });
      }
      
      if (firstFocusElement) {
        (firstFocusElement as HTMLElement).focus();
      }
      return;
    }

    this.isLoading = true;

    // Map inline creation names directly into the product model
    this.product.newGenericName = this.isNewGeneric && this.newGenericName.trim() ? this.newGenericName.trim() : null;
    this.product.newCategoryName = this.isNewCategory && this.newCategoryName.trim() ? this.newCategoryName.trim() : null;
    this.product.newBrandName = this.isNewManufacturer && this.newManufacturerName.trim() ? this.newManufacturerName.trim() : null;
    this.product.newLocationName = this.isNewLocation && this.newLocationName.trim() ? this.newLocationName.trim() : null;
    this.product.newUomName = this.isNewUom && this.newUomName.trim() ? this.newUomName.trim() : null;
    this.product.newUOMName = this.isNewUom && this.newUomName.trim() ? this.newUomName.trim() : null;

    // Base unit conversion setup (will be overridden on server if newUOMName is provided)
    this.product.baseUomId = this.product.uomId || 0;
    this.product.selectedPurchaseUnitId = this.product.uomId || 0;

    if (this.isEditMode) {
      this.api.put(`Product/${this.product.productId}`, this.product).subscribe({
        next: () => {
          this.notification.success('Product updated successfully');
          this.router.navigate(['/dashboard/inventory']);
        },
        error: (err) => { 
          const msg = err.error?.message || (typeof err.error === 'string' ? err.error : null) || 'Update failed';
          this.notification.error(msg); 
          this.isLoading = false; 
        }
      });
    } else {
      this.api.post('Product', this.product).subscribe({
        next: () => {
          this.notification.success('Product created successfully');
          this.router.navigate(['/dashboard/inventory']);
        },
        error: (err) => { 
          const msg = err.error?.message || (typeof err.error === 'string' ? err.error : null) || 'Create failed';
          this.notification.error(msg); 
          this.isLoading = false; 
        }
      });
    }
  }

  getImageUrl(path: string): string {
    if (!path) return '';
    if (path.startsWith('http') || path.startsWith('data:')) {
      return path;
    }
    return `${this.api.getFileServerUrl()}${path}`;
  }
}
