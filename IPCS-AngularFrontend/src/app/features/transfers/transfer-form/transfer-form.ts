import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * TransferFormComponent
 * Handles the initiation of stock transfers from one branch to another.
 * Requires selecting products and specific lots for accurate inventory deduction.
 */
@Component({
  selector: 'app-transfer-form',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './transfer-form.html'
})
export class TransferFormComponent implements OnInit {
  // Main Transfer Object
  transfer: any = {
    transferDate: new Date().toISOString().split('T')[0],
    fromBranchId: 1, // Default from branch
    toBranchId: null,
    remarks: '',
    shippingCharge: 0,
    details: []
  };

  // UI Lookups
  branches: any[] = [];
  searchTerm: string = '';
  searchResults: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.loadBranches();
  }

  /**
   * Loads the list of available branches for transfer
   */
  loadBranches() {
    this.api.get<any[]>('Branch').subscribe(data => this.branches = data);
  }

  /**
   * Searches for products with available stock in the source branch
   */
  searchProduct() {
    if (this.searchTerm.length < 2) {
      this.searchResults = [];
      return;
    }
    // Note: In a real scenario, this would filter by the selected FromBranchId
    this.api.get<any[]>(`Product?search=${this.searchTerm}`).subscribe(data => {
      this.searchResults = data;
    });
  }

  /**
   * Adds a product to the transfer items list
   */
  addProduct(product: any) {
    this.transfer.details.push({
      productId: product.productId,
      productName: product.productName,
      sku: product.sku,
      lotId: product.lotId || 1, // Defaulting to first lot if multiple not handled yet
      transferQty: 1,
      uomId: product.uomId,
      uomName: product.uomName,
      unitPrice: product.salesPrice
    });
    this.searchTerm = '';
    this.searchResults = [];
  }

  /**
   * Removes an item from the transfer list
   */
  removeItem(index: number) {
    this.transfer.details.splice(index, 1);
  }

  /**
   * Submits the transfer request to the API
   */
  onSubmit() {
    if (this.transfer.fromBranchId === this.transfer.toBranchId) {
      alert('Source and destination branches cannot be the same');
      return;
    }
    if (this.transfer.details.length === 0) {
      alert('Please add at least one item to transfer');
      return;
    }

    this.isLoading = true;
    this.api.post('Transfer/initiate', this.transfer).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.router.navigate(['/dashboard/transfers']);
      },
      error: (err) => {
        alert(err.error?.details || 'Transfer initiation failed');
        this.isLoading = false;
      }
    });
  }
}
