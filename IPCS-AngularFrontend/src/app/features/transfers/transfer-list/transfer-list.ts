import { Component, OnInit } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
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
  imports: [CommonModule, RouterModule, FormsModule, TranslatePipe],
  templateUrl: './transfer-list.html'
})
export class TransferListComponent implements OnInit {
  transfers: any[] = []; // List of all stock transfers
  isLoading = false;

  // Details state
  selectedTransferForDetails: any = null;

  constructor(
    private api: ApiService,
    private notifications: NotificationService,
    private translate: TranslateService
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
      this.translate.instant('transfers.messages.confirmReceiveText'),
      this.translate.instant('transfers.list.confirmReceived'),
      this.translate.instant('transfers.messages.yesReceived'),
      this.translate.instant('transfers.messages.noCancel'),
      false
    ).then(confirmed => {
      if (confirmed) {
        this.api.post('Transfer/confirm-receive', { transferId }).subscribe({
          next: (res: any) => {
            this.notifications.success(res.message || this.translate.instant('transfers.messages.goodsReceived'));
            this.loadTransfers();
          },
          error: (err) => {
            this.notifications.error(err.error?.message || this.translate.instant('transfers.messages.confirmReceiveFailed'));
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
      this.translate.instant('transfers.messages.cancelConfirmText'),
      this.translate.instant('transfers.list.cancelTransfer'),
      this.translate.instant('transfers.messages.yesCancel'),
      this.translate.instant('transfers.messages.noKeepIt'),
      true
    ).then(confirmed => {
      if (confirmed) {
        this.api.post(`Transfer/cancel/${id}`, {}).subscribe({
          next: (res: any) => {
            this.notifications.success(res.message || this.translate.instant('transfers.messages.canceled'));
            this.loadTransfers();
          },
          error: (err) => {
            this.notifications.error(err.error?.message || this.translate.instant('transfers.messages.cancelFailed'));
          }
        });
      }
    });
  }
}
