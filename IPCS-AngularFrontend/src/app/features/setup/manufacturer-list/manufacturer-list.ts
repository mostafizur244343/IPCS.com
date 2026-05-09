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
  newManufacturer = { manufacturerName: '', contactPerson: '', phone: '', email: '' };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadManufacturers();
  }

  /**
   * Fetches the list of all active manufacturers
   */
  loadManufacturers() {
    this.isLoading = true;
    this.api.get<any[]>('Manufacturer').subscribe({
      next: (data) => {
        this.manufacturers = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Adds a new manufacturer to the system
   */
  addManufacturer() {
    if (!this.newManufacturer.manufacturerName) return;
    this.isLoading = true;
    this.api.post('Manufacturer', this.newManufacturer).subscribe({
      next: () => {
        this.newManufacturer = { manufacturerName: '', contactPerson: '', phone: '', email: '' };
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
