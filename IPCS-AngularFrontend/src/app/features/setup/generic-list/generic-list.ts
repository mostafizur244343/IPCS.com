import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * GenericListComponent
 * Manages generic chemical names for medicines.
 * Used for suggesting alternatives and grouping products.
 */
@Component({
  selector: 'app-generic-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './generic-list.html'
})
export class GenericListComponent implements OnInit {
  generics: any[] = [];
  filteredGenerics: any[] = [];
  newGeneric = { genericName: '', indication: '' };
  isLoading = false;
  showForm = false;
  searchTerm = '';

  // Editing state
  editingGenericId: number | null = null;
  editGeneric: any = {};

  // Details state
  selectedGenericForDetails: any = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadGenerics();
  }

  showDetails(generic: any) {
    this.selectedGenericForDetails = generic;
  }

  closeDetails() {
    this.selectedGenericForDetails = null;
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredGenerics = this.generics.filter(g => 
      (g.genericName && g.genericName.toLowerCase().includes(term)) ||
      (g.indication && g.indication.toLowerCase().includes(term))
    );
  }

  /**
   * Fetches the list of all generic chemical names
   */
  loadGenerics() {
    this.isLoading = true;
    this.api.get<any[]>('GenericInfo').subscribe({
      next: (data) => {
        this.generics = data;
        this.filteredGenerics = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Adds a new generic name to the setup
   */
  addGeneric() {
    if (!this.newGeneric.genericName) return;
    this.isLoading = true;
    this.api.post('GenericInfo', this.newGeneric).subscribe({
      next: () => {
        this.newGeneric = { genericName: '', indication: '' };
        this.showForm = false;
        this.loadGenerics();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Starts editing a generic entry
   */
  startEdit(generic: any) {
    this.editingGenericId = generic.genericId;
    this.editGeneric = { ...generic };
  }

  /**
   * Cancels editing
   */
  cancelEdit() {
    this.editingGenericId = null;
    this.editGeneric = {};
  }

  /**
   * Saves the updated generic entry
   */
  updateGeneric() {
    if (!this.editGeneric.genericName || !this.editingGenericId) return;
    this.isLoading = true;
    this.api.put(`GenericInfo/${this.editingGenericId}`, this.editGeneric).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadGenerics();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Removes a generic name entry
   */
  deleteGeneric(id: number) {
    if (confirm('Delete this generic entry?')) {
      this.api.delete(`GenericInfo/${id}`).subscribe(() => this.loadGenerics());
    }
  }
}

