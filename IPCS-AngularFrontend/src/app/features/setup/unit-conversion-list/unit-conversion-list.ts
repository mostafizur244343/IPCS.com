import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * UnitConversionListComponent
 * Manages conversion factors between different Units of Measurement.
 * Essential for products sold in multiple units (e.g., Box vs Piece).
 */
@Component({
  selector: 'app-unit-conversion-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './unit-conversion-list.html'
})
export class UnitConversionListComponent implements OnInit {
  conversions: any[] = [];
  filteredConversions: any[] = [];
  newConversion: any = {
    productId: null,
    fromUnitId: null,
    toUnitId: null,
    factor: 1,
    level: 1
  };

  products: any[] = [];
  units: any[] = [];
  isLoading = false;
  showForm = false;
  searchTerm = '';

  // Details state
  selectedConversionForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(conversion: any) {
    this.selectedConversionForDetails = conversion;
  }

  closeDetails() {
    this.selectedConversionForDetails = null;
  }

  ngOnInit() {
    this.loadConversions();
    this.loadLookups();
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredConversions = this.conversions.filter(c => 
      (c.product?.productName && c.product.productName.toLowerCase().includes(term)) ||
      (c.fromUnit?.uomName && c.fromUnit.uomName.toLowerCase().includes(term)) ||
      (c.toUnit?.uomName && c.toUnit.uomName.toLowerCase().includes(term))
    );
  }

  loadLookups() {
    this.api.get<any[]>('Product').subscribe(data => this.products = data);
    this.api.get<any[]>('UOM').subscribe(data => this.units = data);
  }

  loadConversions() {
    this.isLoading = true;
    this.api.get<any[]>('ProductUnitConversion').subscribe({
      next: (data) => {
        this.conversions = data;
        this.filteredConversions = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  addConversion() {
    if (!this.newConversion.productId || !this.newConversion.fromUnitId || !this.newConversion.toUnitId) {
      alert('Please fill all required fields');
      return;
    }

    this.isLoading = true;
    this.api.post('ProductUnitConversion', this.newConversion).subscribe({
      next: () => {
        this.newConversion = { productId: null, fromUnitId: null, toUnitId: null, factor: 1, level: 1 };
        this.showForm = false;
        this.loadConversions();
      },
      error: () => this.isLoading = false
    });
  }

  deleteConversion(id: number) {
    if (confirm('Delete this conversion factor?')) {
      this.api.delete(`ProductUnitConversion/${id}`).subscribe(() => this.loadConversions());
    }
  }
}

