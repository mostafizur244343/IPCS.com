import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ImageUploadComponent } from '../../../shared/image-upload/image-upload';

/**
 * CustomerListComponent
 * Manages pharmacy customers and their credit profiles.
 */
@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ImageUploadComponent],
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

  // Details state
  selectedCustomerForDetails: any = null;

  constructor(private api: ApiService) {}

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
    const term = this.searchTerm.toLowerCase();
    this.filteredCustomers = this.customers.filter(c => 
      (c.customerName && c.customerName.toLowerCase().includes(term)) ||
      (c.mobile && c.mobile.toLowerCase().includes(term))
    );
  }

  loadCustomers() {
    this.isLoading = true;
    this.api.get<any[]>('Customer/active').subscribe({
      next: (data) => {
        this.customers = data;
        this.filteredCustomers = data;
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
          alert('Image upload failed: ' + (err.error?.Message || err.message));
          this.isUploading = false;
        }
      });
    } catch (err) {
      console.error('Upload error:', err);
      alert('Image upload failed');
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
    if (confirm('Delete this customer?')) {
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

