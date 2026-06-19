import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { ImageUploadComponent } from '../../../shared/image-upload/image-upload';

/**
 * SupplierListComponent
 * Manages pharmaceutical distributors and supply chain partners.
 */
@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ImageUploadComponent],
  templateUrl: './supplier-list.html'
})
export class SupplierListComponent implements OnInit {
  suppliers: any[] = [];
  filteredSuppliers: any[] = [];
  newSupplier = { supplierName: '', mobile: '', openingBalance: 0, isDue: true, picturePath: '' };
  isLoading = false;
  isUploading = false;
  showForm = false;
  searchTerm = '';

  // Editing state
  editingSupplierId: number | null = null;
  editSupplier: any = {};

  // Details state
  selectedSupplierForDetails: any = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadSuppliers();
  }

  showDetails(supplier: any) {
    this.selectedSupplierForDetails = supplier;
  }

  closeDetails() {
    this.selectedSupplierForDetails = null;
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredSuppliers = this.suppliers.filter(s => 
      (s.supplierName && s.supplierName.toLowerCase().includes(term)) ||
      (s.mobile && s.mobile.toLowerCase().includes(term)) ||
      (s.supplierCode && s.supplierCode.toLowerCase().includes(term))
    );
  }

  loadSuppliers() {
    this.isLoading = true;
    this.api.get<any[]>('Supplier/active').subscribe({
      next: (data) => {
        this.suppliers = data;
        this.filteredSuppliers = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  async onImageSelected(file: File, target: 'new' | 'edit' = 'new') {
    console.log('File selected:', file.name, file.size, file.type);
    this.isUploading = true;
    
    this.api.uploadFile(file).subscribe({
      next: (response: any) => {
        console.log('Upload response:', response);
        const filePath = response.filePath || response.FilePath;
        if (target === 'new') {
          this.newSupplier.picturePath = filePath;
        } else {
          this.editSupplier.picturePath = filePath;
        }
        this.isUploading = false;
        alert('Image uploaded successfully!');
      },
      error: (err) => {
        console.error('Upload failed full error:', err);
        const errorMsg = err.error?.error || err.error?.message || err.message || 'Unknown error';
        alert('Image upload failed: ' + errorMsg);
        this.isUploading = false;
      }
    });
  }

  onImageRemoved(target: 'new' | 'edit' = 'new') {
    if (target === 'new') {
      this.newSupplier.picturePath = '';
    } else {
      this.editSupplier.picturePath = '';
    }
  }

  addSupplier() {
    if (!this.newSupplier.supplierName || !this.newSupplier.mobile) return;
    this.isLoading = true;

    // Force values to correct types
    const payload = {
      ...this.newSupplier,
      supplierName: this.newSupplier.supplierName,
      mobile: this.newSupplier.mobile,
      picturePath: this.newSupplier.picturePath,
      openingBalance: Number(this.newSupplier.openingBalance),
      isDue: this.newSupplier.isDue
    };

    this.api.post('Supplier', payload).subscribe({
      next: () => {
        this.newSupplier = { supplierName: '', mobile: '', openingBalance: 0, isDue: true, picturePath: '' };
        this.showForm = false;
        this.loadSuppliers();
      },
      error: (err) => {
        console.error('Supplier Create Error:', err);
        const errorMsg = err.error?.Error || err.error?.Msg || 'সাপ্লায়ার তৈরি করা সম্ভব হয়নি।';
        alert(errorMsg);
        this.isLoading = false;
      }
    });
  }

  startEdit(sup: any) {
    this.editingSupplierId = sup.supplierId;
    this.editSupplier = { ...sup };
  }

  cancelEdit() {
    this.editingSupplierId = null;
    this.editSupplier = {};
  }

  updateSupplier() {
    if (!this.editSupplier.supplierName || !this.editingSupplierId) return;
    this.isLoading = true;

    const payload = {
      supplierName: this.editSupplier.supplierName,
      mobile: this.editSupplier.mobile,
      picturePath: this.editSupplier.picturePath,
      openingBalance: Number(this.editSupplier.openingBalance),
      isDue: this.editSupplier.isDue
    };

    this.api.put(`Supplier/${this.editingSupplierId}`, payload).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadSuppliers();
      },
      error: (err) => {
        console.error('Supplier Update Error:', err);
        this.isLoading = false;
      }
    });
  }

  deleteSupplier(id: number) {
    if (confirm('Delete this supplier?')) {
      this.api.delete(`Supplier/${id}`).subscribe(() => this.loadSuppliers());
    }
  }

  getImageUrl(path: string): string {
    if (!path) return '';
    if (path.startsWith('http') || path.startsWith('data:')) {
      return path;
    }
    return `https://localhost:7054${path}`;
  }
}

