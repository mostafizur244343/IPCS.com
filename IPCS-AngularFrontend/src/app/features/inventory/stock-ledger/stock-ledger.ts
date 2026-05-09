import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * StockLedgerComponent
 * Provides a comprehensive history of all stock movements.
 * Includes Purchases, Sales, Transfers, and Adjustments.
 */
@Component({
  selector: 'app-stock-ledger',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './stock-ledger.html'
})
export class StockLedgerComponent implements OnInit {
  ledgerEntries: any[] = [];
  filteredEntries: any[] = [];
  searchTerm: string = '';
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadLedger();
  }

  /**
   * Fetches the entire stock movement history for the branch
   */
  loadLedger() {
    this.isLoading = true;
    const branchId = 1; // Default branch
    this.api.get<any[]>(`StockLedger/branch/${branchId}`).subscribe({
      next: (data) => {
        this.ledgerEntries = data.sort((a, b) => new Date(b.createdDate).getTime() - new Date(a.createdDate).getTime());
        this.filteredEntries = this.ledgerEntries;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Filters the ledger by medicine name or transaction type
   */
  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredEntries = this.ledgerEntries.filter(e => 
      e.productName?.toLowerCase().includes(term) || 
      e.transactionType?.toLowerCase().includes(term)
    );
  }
}
