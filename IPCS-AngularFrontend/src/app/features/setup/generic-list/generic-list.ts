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
  newGeneric = { genericName: '', indications: '' };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadGenerics();
  }

  /**
   * Fetches the list of all generic chemical names
   */
  loadGenerics() {
    this.isLoading = true;
    this.api.get<any[]>('GenericInfo').subscribe({
      next: (data) => {
        this.generics = data;
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
        this.newGeneric = { genericName: '', indications: '' };
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
