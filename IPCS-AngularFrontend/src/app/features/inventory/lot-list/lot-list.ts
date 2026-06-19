import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { PermissionService } from '../../../core/services/permission';

@Component({
  selector: 'app-lot-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container animate-fade-in">
      <div class="page-header">
        <div>
          <h1>Batch / Lot Information</h1>
          <p>View and manage product batches, manufacturing dates, and expiry tracking.</p>
        </div>
      </div>

      <div class="card table-card">
        <div *ngIf="isLoading" class="loading-state">
          <div class="spinner-blue"></div>
          <p>Loading Batches/Lots...</p>
        </div>
        
        <table *ngIf="!isLoading" class="data-table">
          <thead>
            <tr>
              <th>Batch No</th>
              <th>Product</th>
              <th>Mfg Date</th>
              <th>Expiry Date</th>
              <th>Purchase Price</th>
              <th>Sales Price</th>
              <th>Status</th>
              <th class="text-right">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let lot of lots">
              <td><span class="batch-no-badge">{{ lot.lotNumber }}</span></td>
              <td><strong>{{ lot.product?.productName || 'N/A' }}</strong></td>
              <td>{{ (lot.manufacturingDate | date:'mediumDate') || 'N/A' }}</td>
              <td>{{ lot.expiryDate | date:'mediumDate' }}</td>
              <td>৳{{ lot.purchasePrice | number:'1.2-2' }}</td>
              <td>৳{{ lot.product?.salesPrice | number:'1.2-2' }}</td>
              <td>
                <span class="status-badge" [class.expired]="isExpired(lot.expiryDate)">
                  {{ isExpired(lot.expiryDate) ? 'Expired' : 'Active' }}
                </span>
              </td>
              <td class="text-right">
                <button class="icon-btn info" (click)="showDetails(lot)" title="Details">
                  <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>
                </button>
              </td>
            </tr>
            <tr *ngIf="lots.length === 0">
              <td colspan="8" class="no-data">No batch/lot records found.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Details Modal -->
    <div class="modal-backdrop" *ngIf="selectedLotForDetails" (click)="closeDetails()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>Batch / Lot Details</h2>
          <button class="modal-close-btn" (click)="closeDetails()">×</button>
        </div>
        <div class="modal-body">
          <div class="detail-grid">
            <div class="detail-label">Lot ID</div>
            <div class="detail-value">#{{ selectedLotForDetails.lotId }}</div>

            <div class="detail-label">Batch / Lot No</div>
            <div class="detail-value"><span class="batch-no-badge">{{ selectedLotForDetails.lotNumber }}</span></div>

            <div class="detail-label">Product Name</div>
            <div class="detail-value"><strong>{{ selectedLotForDetails.product?.productName || 'N/A' }}</strong></div>

            <div class="detail-label">SKU</div>
            <div class="detail-value">{{ selectedLotForDetails.product?.sku || selectedLotForDetails.product?.sKU || 'N/A' }}</div>

            <div class="detail-label">Manufacturing Date</div>
            <div class="detail-value">{{ (selectedLotForDetails.manufacturingDate | date:'longDate') || 'N/A' }}</div>

            <div class="detail-label">Expiry Date</div>
            <div class="detail-value">{{ selectedLotForDetails.expiryDate | date:'longDate' }}</div>

            <div class="detail-label">Original Price</div>
            <div class="detail-value">৳{{ selectedLotForDetails.purchasePrice | number:'1.2-2' }}</div>

            <div class="detail-label">Sales Price</div>
            <div class="detail-value">৳{{ (selectedLotForDetails.product?.salesPrice | number:'1.2-2') || '0.00' }}</div>

            <div class="detail-label">Status</div>
            <div class="detail-value">
              <span class="status-badge" [class.expired]="isExpired(selectedLotForDetails.expiryDate)">
                {{ isExpired(selectedLotForDetails.expiryDate) ? 'Expired' : 'Active' }}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <style>
      .container { padding-bottom: 40px; }
      .page-header { margin-bottom: 24px; }
      .page-header h1 { font-size: 28px; font-weight: 700; color: #1e293b; margin: 0 0 4px 0; }
      .page-header p { margin: 0; color: #64748b; font-size: 14px; }
      
      .batch-no-badge {
        background-color: #fef9c3;
        color: #854d0e;
        padding: 4px 10px;
        border-radius: 6px;
        font-weight: 600;
        font-size: 13px;
        border: 1px solid #fef08a;
      }
      
      .status-badge {
        padding: 4px 10px;
        border-radius: 6px;
        background: #dcfce7;
        color: #15803d;
        font-weight: 500;
        font-size: 13px;
      }
      .status-badge.expired {
        background: #fee2e2;
        color: #ef4444;
      }
      
      .text-right { text-align: right; }
      
      .icon-btn {
        width: 32px;
        height: 32px;
        border-radius: 8px;
        border: 1px solid #e2e8f0;
        background: white;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        transition: all 0.2s;
      }
      .icon-btn.info:hover {
        color: var(--primary, #0f766e);
        background: #f0fdfa;
        border-color: var(--primary, #0f766e);
      }
      
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
export class LotListComponent implements OnInit {
  lots: any[] = [];
  isLoading = false;
  selectedLotForDetails: any = null;

  constructor(private api: ApiService, public permService: PermissionService) {}

  ngOnInit() {
    this.loadLots();
  }

  loadLots() {
    this.isLoading = true;
    this.api.get<any[]>('LotInfo').subscribe(data => {
      this.lots = data;
      this.isLoading = false;
    });
  }

  isExpired(date: string): boolean {
    return new Date(date) < new Date();
  }

  showDetails(lot: any) {
    this.selectedLotForDetails = lot;
  }

  closeDetails() {
    this.selectedLotForDetails = null;
  }
}
