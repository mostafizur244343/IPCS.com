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
  newConversion: any = {
    productId: null,
    fromUomId: null,
    toUomId: null,
    conversionFactor: 1
  };

  products: any[] = [];
  units: any[] = [];
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadConversions();
    this.loadLookups();
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
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  addConversion() {
    if (!this.newConversion.productId || !this.newConversion.fromUomId || !this.newConversion.toUomId) {
      alert('Please fill all required fields');
      return;
    }

    this.isLoading = true;
    this.api.post('ProductUnitConversion', this.newConversion).subscribe({
      next: () => {
        this.newConversion = { productId: null, fromUomId: null, toUomId: null, conversionFactor: 1 };
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
