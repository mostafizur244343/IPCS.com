import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

/**
 * CustomerLedgerComponent
 * Displays the transaction history (Khata) for a specific customer.
 * Shows all sales and payments made by the customer.
 */
@Component({
  selector: 'app-customer-ledger',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-ledger.html'
})
export class CustomerLedgerComponent implements OnInit {
  customerId: number = 0;
  customer: any = null;
  salesHistory: any[] = [];
  isLoading = false;

  constructor(private api: ApiService, private route: ActivatedRoute) {}

  ngOnInit() {
    this.customerId = this.route.snapshot.params['id'];
    this.loadHistory();
  }

  /**
   * Fetches the customer profile and their sales history from the API
   */
  loadHistory() {
    this.isLoading = true;
    this.api.get<any>(`Customer/history/${this.customerId}`).subscribe({
      next: (data) => {
        this.customer = data.customer;
        this.salesHistory = data.salesHistory;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Utility to calculate total due from the history
   */
  getTotalDue(): number {
    return this.salesHistory?.reduce((sum, s) => sum + (s.dueAmount || 0), 0) || 0;
  }
}
