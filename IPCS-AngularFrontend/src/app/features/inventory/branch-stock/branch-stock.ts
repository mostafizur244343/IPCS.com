import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../../core/services/api';
import { PermissionService } from '../../../core/services/permission';

@Component({
  selector: 'app-branch-stock',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container animate-fade-in">
      <!-- Page Header -->
      <div class="page-header">
        <div>
          <h1>Branch Stock Inventory</h1>
          <p>Real-time physical stock levels across all pharmacy branches.</p>
        </div>
      </div>

      <!-- Filter/Search Card -->
      <div class="card filter-card">
        <div class="search-box">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8"></circle>
            <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
          </svg>
          <input 
            type="text" 
            placeholder="Search by branch, product or batch..." 
            [(ngModel)]="searchTerm"
            (input)="onSearch()"
          >
        </div>
      </div>

      <!-- Table Card -->
      <div class="card table-card">
        <div *ngIf="isLoading" class="loading-state">
          <div class="spinner-blue"></div>
          <p>Loading Branch Stock...</p>
        </div>

        <table *ngIf="!isLoading" class="data-table">
          <thead>
            <tr>
              <th>Branch</th>
              <th>Product Name</th>
              <th>Batch / Lot No</th>
              <th>Current Stock</th>
              <th>Damaged Stock</th>
              <th>Last Updated</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let stock of filteredStocks">
              <td>
                <span class="branch-badge">{{ stock.branchName || 'N/A' }}</span>
              </td>
              <td><strong>{{ stock.productName || 'N/A' }}</strong></td>
              <td>
                <span class="batch-badge">{{ stock.lotNumber || 'N/A' }}</span>
              </td>
              <td>
                <span class="stock-badge" [class.low]="stock.currentStock <= 5">
                  {{ stock.currentStock }}
                </span>
              </td>
              <td>
                <span class="damaged-badge" [class.has-damaged]="stock.damagedStock > 0">
                  {{ stock.damagedStock }}
                </span>
              </td>
              <td>{{ stock.lastUpdated | date:'medium' }}</td>
              <td>
                <span class="status-indicator" [class.active]="stock.currentStock > 0">
                  {{ stock.currentStock > 0 ? 'In Stock' : 'Out of Stock' }}
                </span>
              </td>
            </tr>
            <tr *ngIf="filteredStocks.length === 0">
              <td colspan="7" class="no-data">No branch stock records found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <style>
      .container { padding-bottom: 40px; }
      .page-header { margin-bottom: 24px; }
      .page-header h1 { font-size: 28px; font-weight: 700; color: #1e293b; margin: 0 0 4px 0; }
      .page-header p { margin: 0; color: #64748b; font-size: 14px; }

      .filter-card { margin-bottom: 24px; padding: 16px 24px; }
      .search-box { display: flex; align-items: center; background: #f1f5f9; padding: 10px 16px; border-radius: 10px; gap: 12px; }
      .search-box input { background: none; border: none; outline: none; width: 100%; font-size: 14px; color: #1e293b; }

      .branch-badge {
        background-color: #e0f2fe;
        color: #0369a1;
        padding: 4px 10px;
        border-radius: 6px;
        font-weight: 600;
        font-size: 13px;
        border: 1px solid #bae6fd;
      }

      .batch-badge {
        background-color: #fef9c3;
        color: #854d0e;
        padding: 4px 10px;
        border-radius: 6px;
        font-weight: 600;
        font-size: 13px;
        border: 1px solid #fef08a;
      }

      .stock-badge {
        padding: 4px 10px;
        border-radius: 6px;
        background: #f1f5f9;
        font-weight: 500;
      }
      .stock-badge.low {
        background: #fee2e2;
        color: #ef4444;
        font-weight: 600;
      }

      .damaged-badge {
        padding: 4px 10px;
        border-radius: 6px;
        background: #f1f5f9;
        color: #64748b;
      }
      .damaged-badge.has-damaged {
        background: #ffedd5;
        color: #ea580c;
        font-weight: 600;
      }

      .status-indicator { display: inline-flex; align-items: center; gap: 6px; font-size: 13px; }
      .status-indicator::before { content: ''; width: 8px; height: 8px; border-radius: 50%; background: #cbd5e1; }
      .status-indicator.active::before { background: var(--success, #10b981); }

      .loading-state {
        padding: 40px;
        text-align: center;
        color: #64748b;
      }
      .spinner-blue {
        width: 32px;
        height: 32px;
        border: 3px solid rgba(15, 118, 110, 0.1);
        border-top-color: var(--primary, #0f766e);
        border-radius: 50%;
        animation: spin 1s linear infinite;
        margin: 0 auto 12px;
      }
      @keyframes spin { to { transform: rotate(360deg); } }

      .no-data {
        text-align: center;
        padding: 30px;
        color: #64748b;
        font-style: italic;
      }
    </style>
  `
})
export class BranchStockComponent implements OnInit {
  stocks: any[] = [];
  filteredStocks: any[] = [];
  searchTerm = '';
  isLoading = false;

  constructor(private api: ApiService, public permService: PermissionService) {}

  ngOnInit() {
    this.loadStocks();
  }

  loadStocks() {
    this.isLoading = true;
    this.api.get<any[]>('BranchLotStock/all').subscribe({
      next: (data) => {
        this.stocks = data;
        this.filteredStocks = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading branch stock', err);
        this.isLoading = false;
      }
    });
  }

  onSearch() {
    if (!this.searchTerm.trim()) {
      this.filteredStocks = this.stocks;
      return;
    }
    const term = this.searchTerm.toLowerCase().trim();
    this.filteredStocks = this.stocks.filter(s => 
      (s.branchName && s.branchName.toLowerCase().includes(term)) ||
      (s.productName && s.productName.toLowerCase().includes(term)) ||
      (s.lotNumber && s.lotNumber.toLowerCase().includes(term))
    );
  }
}
