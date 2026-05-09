import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

/**
 * StockAdjustmentComponent
 * Handles manual stock corrections for Damage, Loss, or Discrepancies.
 * Integrates with StockLedger to maintain an accurate audit trail.
 */
@Component({
  selector: 'app-stock-adjustment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-adjustment.html'
})
export class StockAdjustmentComponent implements OnInit {
  // Adjustment model
  adjustment: any = {
    productId: null,
    quantity: 0,
    adjustmentType: 'Damage', // Damage, Loss, Correction
    remarks: '',
    branchId: 1
  };

  products: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.api.get<any[]>('Product').subscribe(data => this.products = data);
  }

  /**
   * Submits the adjustment as a ledger entry
   */
  onSubmit() {
    if (!this.adjustment.productId || this.adjustment.quantity <= 0) {
      alert('Please select a product and enter a valid quantity.');
      return;
    }

    this.isLoading = true;
    
    // Creating the ledger payload
    const payload = {
      productId: this.adjustment.productId,
      branchId: this.adjustment.branchId,
      quantityIn: 0,
      quantityOut: this.adjustment.quantity, // Adjustments usually reduce stock
      transactionType: `Adjustment (${this.adjustment.adjustmentType})`,
      remarks: this.adjustment.remarks
    };

    this.api.post('StockLedger/entry', payload).subscribe({
      next: (res: any) => {
        alert(res.message);
        this.router.navigate(['/dashboard/inventory/ledger']);
      },
      error: () => {
        alert('Failed to save adjustment');
        this.isLoading = false;
      }
    });
  }
}
