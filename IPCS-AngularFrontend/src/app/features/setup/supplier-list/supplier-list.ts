import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * SupplierListComponent
 * Manages pharmaceutical distributors and supply chain partners.
 */
@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './supplier-list.html'
})
export class SupplierListComponent implements OnInit {
  suppliers: any[] = [];
  newSupplier = { supplierName: '', contactPerson: '', phone: '', email: '', address: '' };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadSuppliers();
  }

  loadSuppliers() {
    this.isLoading = true;
    this.api.get<any[]>('Supplier').subscribe({
      next: (data) => {
        this.suppliers = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  addSupplier() {
    if (!this.newSupplier.supplierName) return;
    this.isLoading = true;
    this.api.post('Supplier', this.newSupplier).subscribe({
      next: () => {
        this.newSupplier = { supplierName: '', contactPerson: '', phone: '', email: '', address: '' };
        this.loadSuppliers();
      },
      error: () => this.isLoading = false
    });
  }

  deleteSupplier(id: number) {
    if (confirm('Delete this supplier?')) {
      this.api.delete(`Supplier/${id}`).subscribe(() => this.loadSuppliers());
    }
  }
}
