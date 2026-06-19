import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../../../core/services/notification';

/**
 * TransferListComponent
 * Manages the movement of stock between different pharmacy branches.
 * Displays all transfer requests and their current statuses.
 */
@Component({
  selector: 'app-transfer-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './transfer-list.html'
})
export class TransferListComponent implements OnInit {
  transfers: any[] = []; // List of all stock transfers
  isLoading = false;

  // Details state
  selectedTransferForDetails: any = null;

  constructor(
    private api: ApiService,
    private notifications: NotificationService
  ) {}

  showDetails(transfer: any) {
    this.api.get<any>(`Transfer/${transfer.transferId}`).subscribe({
      next: (fullData) => {
        this.selectedTransferForDetails = fullData;
      },
      error: (err) => {
        console.error('Error loading transfer details', err);
        this.selectedTransferForDetails = transfer;
      }
    });
  }

  closeDetails() {
    this.selectedTransferForDetails = null;
  }

  ngOnInit() {
    this.loadTransfers();
  }

  /**
   * Fetches all transfer records from the API
   */
  loadTransfers() {
    this.isLoading = true;
    this.api.get<any[]>('Transfer').subscribe({
      next: (data) => {
        this.transfers = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading transfers', err);
        this.isLoading = false;
      }
    });
  }

  /**
   * Confirms the receipt of goods at the destination branch
   */
  confirmReceive(transferId: number) {
    this.notifications.confirm(
      'Are you sure you have received these items? This will update local stock levels.',
      'Confirm Goods Received',
      'Yes, Received',
      'No, Cancel',
      false
    ).then(confirmed => {
      if (confirmed) {
        this.api.post('Transfer/confirm-receive', { transferId }).subscribe({
          next: (res: any) => {
            this.notifications.success(res.message || 'Goods received confirmed successfully.');
            this.loadTransfers();
          },
          error: (err) => {
            this.notifications.error(err.error?.message || 'Failed to confirm goods receipt.');
          }
        });
      }
    });
  }

  /**
   * Cancels a pending transfer
   */
  cancelTransfer(id: number) {
    this.notifications.confirm(
      'Are you sure you want to cancel this transfer?',
      'Cancel Transfer',
      'Yes, Cancel',
      'No, Keep it',
      true
    ).then(confirmed => {
      if (confirmed) {
        this.api.post(`Transfer/cancel/${id}`, {}).subscribe({
          next: (res: any) => {
            this.notifications.success(res.message || 'Transfer canceled successfully.');
            this.loadTransfers();
          },
          error: (err) => {
            this.notifications.error(err.error?.message || 'Failed to cancel transfer.');
          }
        });
      }
    });
  }
}
