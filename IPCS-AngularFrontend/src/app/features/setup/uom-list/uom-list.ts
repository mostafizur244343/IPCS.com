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
  templateUrl: './uom-list.html'
})
export class UomListComponent implements OnInit {
  uoms: any[] = [];
  newUom = { uomName: '', uomAbbreviation: '' };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadUoms();
  }

  /**
   * Fetches the list of all defined units of measurement
   */
  loadUoms() {
    this.isLoading = true;
    this.api.get<any[]>('UOM').subscribe({
      next: (data) => {
        this.uoms = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
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
        this.newUom = { uomName: '', uomAbbreviation: '' };
        this.loadUoms();
      },
      error: () => this.isLoading = false
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
