import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ImageUploadComponent } from '../../../shared/image-upload/image-upload';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { PagedResult } from '../../../core/models/paged-result';

/**
 * CustomerListComponent
 * Manages pharmacy customers and their credit profiles.
 * Server-side paging: only one page is fetched at a time; search is sent
 * to the server (debounced) instead of filtering a full-table download.
 */
@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ImageUploadComponent, PaginationComponent, TranslatePipe],
  templateUrl: './customer-list.html'
})
export class CustomerListComponent implements OnInit {
  customers: any[] = [];
  filteredCustomers: any[] = [];
  newCustomer = { 
    customerId: 0,
    customerName: '', 
    mobile: '', 
    address: '', 
    picturePath: '',
    openingBalance: 0, 
    advanceBalance: 0,
    isDue: false,
    isActive: true
  };
  isLoading = false;
  isUploading = false;
  isEditing = false;
  showForm = false;
  searchTerm = '';

  // Server-side paging state
  page = 1;
  pageSize = 20;
  totalCount = 0;
  private searchTimer: any;

  // Details state
  selectedCustomerForDetails: any = null;

  constructor(private api: ApiService, private translate: TranslateService) {}

  showDetails(customer: any) {
    this.selectedCustomerForDetails = customer;
  }

  closeDetails() {
    this.selectedCustomerForDetails = null;
  }

  ngOnInit() {
    this.loadCustomers();
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (!this.showForm && this.isEditing) {
      this.resetForm();
    }
  }

  onSearch() {
    // Debounced server-side search: reload page 1 from the server.
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.page = 1;
      this.loadCustomers();
    }, 400);
  }

  onPageChange(page: number) {
    this.page = page;
    this.loadCustomers();
  }

  onPageSizeChange(size: number) {
    this.pageSize = size;
    this.page = 1;
    this.loadCustomers();
  }

  loadCustomers() {
    this.isLoading = true;
    const search = this.searchTerm.trim();
    const query = `Customer/active?page=${this.page}&pageSize=${this.pageSize}` + (search ? `&search=${encodeURIComponent(search)}` : '');
    this.api.get<PagedResult<any>>(query).subscribe({
      next: (res) => {
        this.customers = res.items;
        this.filteredCustomers = res.items;
        this.totalCount = res.totalCount;
        // If the last item of a page was deleted, step back a page.
        if (res.items.length === 0 && this.page > 1) {
          this.page--;
          this.loadCustomers();
          return;
        }
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  async onImageSelected(file: File) {
    this.isUploading = true;
    try {
      this.api.uploadFile(file).subscribe({
        next: (response: any) => {
          const filePath = response.filePath || response.FilePath;
          this.newCustomer.picturePath = filePath;
          this.isUploading = false;
        },
        error: (err) => {
          console.error('Upload failed:', err);
          alert(this.translate.instant('setup.imageUploadFailed') + ': ' + (err.error?.Message || err.message));
          this.isUploading = false;
        }
      });
    } catch (err) {
      console.error('Upload error:', err);
      alert(this.translate.instant('setup.imageUploadFailed'));
      this.isUploading = false;
    }
  }

  onImageRemoved() {
    this.newCustomer.picturePath = '';
  }

  addCustomer() {
    if (!this.newCustomer.customerName || !this.newCustomer.mobile) return;
    
    if (this.isEditing) {
      const customerData = {
        customerId: this.newCustomer.customerId,
        customerName: this.newCustomer.customerName,
        mobile: this.newCustomer.mobile,
        address: this.newCustomer.address,
        picturePath: this.newCustomer.picturePath,
        openingBalance: this.newCustomer.openingBalance,
        advanceBalance: this.newCustomer.openingBalance,
        isDue: this.newCustomer.isDue,
        isActive: this.newCustomer.isActive
      };
      
      this.isLoading = true;
      this.api.put(`Customer/${this.newCustomer.customerId}`, customerData).subscribe({
        next: () => {
          this.resetForm();
          this.showForm = false;
          this.loadCustomers();
        },
        error: () => this.isLoading = false
      });
    } else {
      const customerData = {
        customerName: this.newCustomer.customerName,
        mobile: this.newCustomer.mobile,
        address: this.newCustomer.address,
        picturePath: this.newCustomer.picturePath,
        openingBalance: this.newCustomer.openingBalance,
        advanceBalance: this.newCustomer.openingBalance,
        isDue: this.newCustomer.isDue,
        isActive: this.newCustomer.isActive
      };

      this.isLoading = true;
      this.api.post('Customer', customerData).subscribe({
        next: () => {
          this.resetForm();
          this.showForm = false;
          this.loadCustomers();
        },
        error: () => this.isLoading = false
      });
    }
  }

  editCustomer(customer: any) {
    this.newCustomer = { 
      customerId: customer.customerId,
      customerName: customer.customerName,
      mobile: customer.mobile,
      address: customer.address,
      picturePath: customer.picturePath || '',
      openingBalance: customer.openingBalance,
      advanceBalance: customer.advanceBalance,
      isDue: customer.isDue,
      isActive: customer.isActive
    };
    this.isEditing = true;
    this.showForm = true;
  }

  resetForm() {
    this.newCustomer = { 
      customerId: 0,
      customerName: '', 
      mobile: '', 
      address: '', 
      picturePath: '',
      openingBalance: 0, 
      advanceBalance: 0,
      isDue: false,
      isActive: true
    };
    this.isEditing = false;
  }

  deleteCustomer(id: number) {
    if (confirm(this.translate.instant('setup.customers.deleteConfirm'))) {
      this.api.delete(`Customer/${id}`).subscribe(() => this.loadCustomers());
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

