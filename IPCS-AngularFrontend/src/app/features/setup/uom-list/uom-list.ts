import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * UomListComponent
 * Manages Units of Measurement (UOM) for inventory items.
 * Supports defining base units and conversion factors.
 */
@Component({
  selector: 'app-uom-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './uom-list.html',
})
export class UomListComponent implements OnInit {
  uoms: any[] = [];
  filteredUoms: any[] = [];
  newUom = { uomName: '', description: '' };
  isLoading = false;
  showForm = false;
  searchTerm = '';

  // Editing state
  editingUomId: number | null = null;
  editUom: any = {};

  // Details state
  selectedUomForDetails: any = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadUoms();
  }

  showDetails(uom: any) {
    this.selectedUomForDetails = uom;
  }

  closeDetails() {
    this.selectedUomForDetails = null;
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredUoms = this.uoms.filter(u => 
      (u.uomName && u.uomName.toLowerCase().includes(term)) ||
      (u.description && u.description.toLowerCase().includes(term))
    );
  }

  /**
   * Fetches the list of all defined units of measurement
   */
  loadUoms() {
    this.isLoading = true;
    this.api.get<any[]>('UOM').subscribe({
      next: (data) => {
        this.uoms = data;
        this.filteredUoms = data;
        this.isLoading = false;
      },
      error: () => (this.isLoading = false),
    });
  }

  /**
   * Adds a new UOM to the system
   */
  addUom() {
    if (!this.newUom.uomName) return;
    this.isLoading = true;
    this.api.post('UOM', this.newUom).subscribe({
      next: () => {
        this.newUom = { uomName: '', description: '' };
        this.showForm = false;
        this.loadUoms();
      },
      error: () => (this.isLoading = false),
    });
  }

  /**
   * Starts editing a UOM
   */
  startEdit(uom: any) {
    this.editingUomId = uom.uomId;
    this.editUom = { ...uom };
  }

  /**
   * Cancels editing
   */
  cancelEdit() {
    this.editingUomId = null;
    this.editUom = {};
  }

  /**
   * Saves the updated UOM
   */
  updateUom() {
    if (!this.editUom.uomName || !this.editingUomId) return;
    this.isLoading = true;
    this.api.put(`UOM/${this.editingUomId}`, this.editUom).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadUoms();
      },
      error: () => (this.isLoading = false),
    });
  }

  /**
   * Deletes a UOM entry
   */
  deleteUom(id: number) {
    if (confirm('Delete this unit of measurement?')) {
      this.api.delete(`UOM/${id}`).subscribe(() => this.loadUoms());
    }
  }
}
