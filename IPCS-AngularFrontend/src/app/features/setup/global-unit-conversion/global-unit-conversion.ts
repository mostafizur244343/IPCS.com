import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api';

/**
 * GlobalUnitConversionComponent
 * Manages standard universal conversions (e.g., 1 KG = 1000 Grams).
 * These conversions apply universally, independent of specific products.
 */
@Component({
  selector: 'app-global-unit-conversion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './global-unit-conversion.html'
})
export class GlobalUnitConversionComponent implements OnInit {
  conversions: any[] = [];
  filteredConversions: any[] = [];
  units: any[] = [];
  
  newConversion: any = { 
    fromUnitId: null, 
    toUnitId: null, 
    conversionFactor: null 
  };
  
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
    this.loadLookups();
    this.loadConversions();
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredConversions = this.conversions.filter(c => 
      (c.fromUnitName && c.fromUnitName.toLowerCase().includes(term)) ||
      (c.toUnitName && c.toUnitName.toLowerCase().includes(term))
    );
  }

  loadLookups() {
    this.api.get<any[]>('UOM').subscribe(data => this.units = data);
  }

  loadConversions() {
    this.isLoading = true;
    this.api.get<any[]>('GlobalUnitConversion').subscribe({
      next: (data) => { 
        this.conversions = data; 
        this.filteredConversions = data;
        this.isLoading = false; 
      },
      error: () => this.isLoading = false
    });
  }

  addConversion() {
    if (!this.newConversion.fromUnitId || !this.newConversion.toUnitId || !this.newConversion.conversionFactor) {
      alert("Please fill all required fields.");
      return;
    }
    
    this.isLoading = true;
    this.api.post('GlobalUnitConversion', this.newConversion).subscribe({
      next: () => {
        this.newConversion = { fromUnitId: null, toUnitId: null, conversionFactor: null };
        this.showForm = false;
        this.loadConversions();
      },
      error: (err) => { 
        alert(err.error?.message || "Failed to save conversion rule"); 
        this.isLoading = false; 
      }
    });
  }

  deleteConversion(id: number) {
    if (confirm("Delete this global conversion rule?")) {
      this.api.delete(`GlobalUnitConversion/${id}`).subscribe(() => this.loadConversions());
    }
  }
}

