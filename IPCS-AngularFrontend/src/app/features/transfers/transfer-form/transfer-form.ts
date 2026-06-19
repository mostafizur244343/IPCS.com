import { Component, ElementRef, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';
import { NotificationService } from '../../../core/services/notification';

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
  branchStocks: any[] = [];
  searchTerm: string = '';
  searchResults: any[] = [];
  isLoading = false;

  constructor(
    private api: ApiService, 
    private router: Router,
    private notifications: NotificationService,
    private elementRef: ElementRef
  ) {}

  ngOnInit() {
    this.loadBranches();
    this.loadBranchStocks();
  }

  /**
   * Loads the list of available branches for transfer
   */
  loadBranches() {
    this.api.get<any[]>('Branch').subscribe(data => {
      this.branches = data || [];

      const hasSelectedBranch = this.branches.some(
        b => Number(b.branchId) === Number(this.transfer.fromBranchId)
      );

      if (!hasSelectedBranch && this.branches.length > 0) {
        this.transfer.fromBranchId = this.branches[0].branchId;
      }

      this.searchProduct();
    });
  }

  /**
   * Loads all active branch stocks for validation and correct lot selection
   */
  loadBranchStocks() {
    this.api.get<any[]>('BranchLotStock/all').subscribe({
      next: (data) => {
        this.branchStocks = data || [];
        this.searchProduct();
      },
      error: (err) => {
        console.error('Failed to load branch stocks', err);
        this.branchStocks = [];
        this.searchProduct();
      }
    });
  }

  /**
   * Clears current details list if from branch changes, since stock balances will be different
   */
  onFromBranchChange() {
    if (this.transfer.details.length > 0) {
      this.notifications.warning('Source branch changed. Cleared previously added items to prevent stock conflicts.');
      this.transfer.details = [];
    }
    this.searchResults = [];
    this.searchTerm = '';
    this.searchProduct();
  }

  isDropdownOpen: boolean = false;

  private normalizeSearchText(value: any): string {
    return String(value || '').toLowerCase().replace(/\s+/g, ' ').trim();
  }

  private getAvailableBranchStocks() {
    const selectedBranchId = Number(this.transfer.fromBranchId);
    const selectedBranchName = this.normalizeSearchText(
      this.branches.find(b => Number(b.branchId) === selectedBranchId)?.branchName
    );

    return this.branchStocks
      .filter(s =>
        (
          Number(s.branchId) === selectedBranchId ||
          this.normalizeSearchText(s.branchName) === selectedBranchName
        ) &&
        Number(s.currentStock) > 0
      )
      .filter(s => !this.transfer.details.some(
        (d: any) => Number(d.productId) === Number(s.productId) && Number(d.lotId) === Number(s.lotId)
      ))
      .sort((a, b) =>
        this.normalizeSearchText(a.productName).localeCompare(this.normalizeSearchText(b.productName))
      );
  }

  /**
   * Searches for products with available stock in the source branch
   */
  searchProduct() {
    const availableStocks = this.getAvailableBranchStocks();
    const term = this.normalizeSearchText(this.searchTerm);

    if (!availableStocks.length) {
      this.searchResults = [];
      this.isDropdownOpen = true;
      return;
    }

    this.searchResults = !term
      ? availableStocks
      : availableStocks.filter(s => {
          const productName = this.normalizeSearchText(s.productName);
          const sku = this.normalizeSearchText(s.sku);
          const lotNumber = this.normalizeSearchText(s.lotNumber);
          return productName.includes(term) || sku.includes(term) || lotNumber.includes(term);
        });

    this.isDropdownOpen = true;
  }

  /**
   * Closes the dropdown
   */
  closeDropdown() {
    setTimeout(() => this.isDropdownOpen = false, 200); // delay to allow click on item
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const clickedInside = this.elementRef.nativeElement.contains(event.target);
    if (!clickedInside) {
      this.isDropdownOpen = false;
    }
  }

  /**
   * Gets complete URL for product images
   */
  getImageUrl(path: string): string {
    if (!path) return '';
    if (path.startsWith('http') || path.startsWith('data:')) {
      return path;
    }
    return `https://localhost:7054${path}`;
  }

  /**
   * Adds a product to the transfer items list
   */
  addProduct(stockItem: any) {
    const existingItem = this.transfer.details.find(
      (d: any) => Number(d.productId) === Number(stockItem.productId) && Number(d.lotId) === Number(stockItem.lotId)
    );

    if (existingItem) {
      if (Number(existingItem.transferQty) < Number(existingItem.maxQty)) {
        existingItem.transferQty = Number(existingItem.transferQty) + 1;
        this.notifications.success(`Batch ${stockItem.lotNumber} quantity increased in the transfer list.`);
      } else {
        this.notifications.warning(`Batch ${stockItem.lotNumber} already reached max available stock.`);
      }
      this.searchTerm = '';
      this.searchProduct();
      return;
    }

    this.transfer.details.push({
      productId: stockItem.productId,
      productName: stockItem.productName,
      sku: stockItem.sku,
      lotId: stockItem.lotId,
      lotNumber: stockItem.lotNumber,
      transferQty: 1,
      maxQty: stockItem.currentStock, // Set stock limit boundary
      uomId: stockItem.uomId,
      uomName: stockItem.uomName,
      unitPrice: stockItem.purchasePrice || stockItem.unitPrice || 0
    });
    this.searchTerm = '';
    this.searchProduct();
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
    if (Number(this.transfer.fromBranchId) === Number(this.transfer.toBranchId)) {
      this.notifications.error('Source and destination branches cannot be the same.');
      return;
    }
    if (this.transfer.details.length === 0) {
      this.notifications.error('Please add at least one item to transfer.');
      return;
    }

    // Validate quantities against physical available stocks
    for (const item of this.transfer.details) {
      if (item.transferQty <= 0) {
        this.notifications.error(`Invalid quantity for ${item.productName}. Please enter a positive amount.`);
        return;
      }
      if (item.transferQty > item.maxQty) {
        this.notifications.error(
          `Insufficient stock for ${item.productName} (Batch: ${item.lotNumber}). Available stock: ${item.maxQty}, Requested: ${item.transferQty}`
        );
        return;
      }
    }

    this.isLoading = true;
    this.api.post('Transfer/initiate', this.transfer).subscribe({
      next: (res: any) => {
        this.notifications.success(res.message || 'Transfer initiated successfully.');
        this.router.navigate(['/dashboard/transfers']);
      },
      error: (err) => {
        this.notifications.error(err.error?.details || err.error?.message || 'Transfer initiation failed.');
        this.isLoading = false;
      }
    });
  }
}
