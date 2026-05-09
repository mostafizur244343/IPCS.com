import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

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

  constructor(private api: ApiService) {}

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
    if (confirm('Are you sure you have received these items? This will update local stock.')) {
      this.api.post('Transfer/confirm-receive', { transferId }).subscribe(() => {
        this.loadTransfers();
      });
    }
  }

  /**
   * Cancels a pending transfer
   */
  cancelTransfer(id: number) {
    if (confirm('Cancel this transfer?')) {
      this.api.post(`Transfer/cancel/${id}`, {}).subscribe(() => {
        this.loadTransfers();
      });
    }
  }
}
