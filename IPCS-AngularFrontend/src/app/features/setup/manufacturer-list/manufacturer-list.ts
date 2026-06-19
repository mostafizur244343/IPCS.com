import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * ManufacturerListComponent
 * Manages pharmaceutical companies and suppliers.
 * Includes contact information and basic CRUD operations.
 */
@Component({
  selector: 'app-manufacturer-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manufacturer-list.html'
})
export class ManufacturerListComponent implements OnInit {
  manufacturers: any[] = [];
  filteredManufacturers: any[] = [];
  newManufacturer = { brandName: '', origin: '', contactPerson: '', phoneNumber: '' };
  isLoading = false;
  showForm = false;
  searchTerm = '';

  // Editing state
  editingBrandId: number | null = null;
  editManufacturer: any = {};

  // Details state
  selectedManufacturerForDetails: any = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadManufacturers();
  }

  showDetails(mfg: any) {
    this.selectedManufacturerForDetails = mfg;
  }

  closeDetails() {
    this.selectedManufacturerForDetails = null;
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredManufacturers = this.manufacturers.filter(m => 
      (m.brandName && m.brandName.toLowerCase().includes(term)) ||
      (m.contactPerson && m.contactPerson.toLowerCase().includes(term)) ||
      (m.origin && m.origin.toLowerCase().includes(term))
    );
  }

  /**
   * Fetches the list of all active manufacturers
   */
  loadManufacturers() {
    this.isLoading = true;
    this.api.get<any[]>('Manufacturer').subscribe({
      next: (data) => {
        this.manufacturers = data;
        this.filteredManufacturers = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Adds a new manufacturer to the system
   */
  addManufacturer() {
    if (!this.newManufacturer.brandName) return;
    this.isLoading = true;
    this.api.post('Manufacturer', this.newManufacturer).subscribe({
      next: () => {
        this.newManufacturer = { brandName: '', origin: '', contactPerson: '', phoneNumber: '' };
        this.showForm = false;
        this.loadManufacturers();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Starts editing a manufacturer
   */
  startEdit(mfg: any) {
    this.editingBrandId = mfg.brandId;
    this.editManufacturer = { ...mfg };
  }

  /**
   * Cancels editing
   */
  cancelEdit() {
    this.editingBrandId = null;
    this.editManufacturer = {};
  }

  /**
   * Saves the updated manufacturer
   */
  updateManufacturer() {
    if (!this.editManufacturer.brandName || !this.editingBrandId) return;
    this.isLoading = true;
    this.api.put(`Manufacturer/${this.editingBrandId}`, this.editManufacturer).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadManufacturers();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Removes a manufacturer from the system
   */
  deleteManufacturer(id: number) {
    if (confirm('Delete this manufacturer?')) {
      this.api.delete(`Manufacturer/${id}`).subscribe(() => this.loadManufacturers());
    }
  }
}

