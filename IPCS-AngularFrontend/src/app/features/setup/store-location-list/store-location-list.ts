import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * StoreLocationListComponent
 * Manages physical storage locations within the pharmacy.
 * Helps staff locate medicines quickly (e.g., Shelf 4, Fridge 2).
 */
@Component({
  selector: 'app-store-location-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './store-location-list.html'
})
export class StoreLocationListComponent implements OnInit {
  locations: any[] = [];
  filteredLocations: any[] = [];
  newLocation = { locationName: '', description: '' };
  isLoading = false;
  showForm = false;
  searchTerm = '';

  // Details state
  selectedLocationForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(loc: any) {
    this.selectedLocationForDetails = loc;
  }

  closeDetails() {
    this.selectedLocationForDetails = null;
  }

  ngOnInit() {
    this.loadLocations();
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredLocations = this.locations.filter(l => 
      (l.locationName && l.locationName.toLowerCase().includes(term)) ||
      (l.description && l.description.toLowerCase().includes(term))
    );
  }

  /**
   * Fetches the list of all storage locations
   */
  loadLocations() {
    this.isLoading = true;
    this.api.get<any[]>('StoreLocation').subscribe({
      next: (data) => {
        this.locations = data;
        this.filteredLocations = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Adds a new physical storage location
   */
  addLocation() {
    if (!this.newLocation.locationName) return;
    this.isLoading = true;
    this.api.post('StoreLocation', this.newLocation).subscribe({
      next: () => {
        this.newLocation = { locationName: '', description: '' };
        this.showForm = false;
        this.loadLocations();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Deletes a storage location
   */
  deleteLocation(id: number) {
    if (confirm('Delete this location?')) {
      this.api.delete(`StoreLocation/${id}`).subscribe(() => this.loadLocations());
    }
  }
}

