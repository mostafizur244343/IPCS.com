import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * PaymentMethodListComponent
 * Configures available payment options (e.g., Cash, Card, Mobile Banking).
 */
@Component({
  selector: 'app-payment-method-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-method-list.html'
})
export class PaymentMethodListComponent implements OnInit {
  methods: any[] = [];
  filteredMethods: any[] = [];
  newMethod = { 
    methodId: 0, 
    MethodName: '', 
    Description: '', 
    IsDigital: false, 
    ExtraChargePercentage: 0, 
    AccountNumber: '', 
    IconPath: '', 
    QRCodePath: '', 
    MinimumAmount: 0, 
    IsActive: true 
  };
  isLoading = false;
  isEditing = false;
  showForm = false;
  searchTerm = '';

  // Details state
  selectedMethodForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(method: any) {
    this.selectedMethodForDetails = method;
  }

  closeDetails() {
    this.selectedMethodForDetails = null;
  }

  ngOnInit() {
    this.loadMethods();
  }

  toggleForm() {
    this.showForm = !this.showForm;
    if (!this.showForm && this.isEditing) {
      this.resetForm();
    }
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredMethods = this.methods.filter(m => 
      (m.methodName && m.methodName.toLowerCase().includes(term)) ||
      (m.description && m.description.toLowerCase().includes(term)) ||
      (m.accountNumber && m.accountNumber.toLowerCase().includes(term))
    );
  }

  loadMethods() {
    this.isLoading = true;
    this.api.get<any[]>('PaymentMethod').subscribe({
      next: (data) => {
        this.methods = data;
        this.filteredMethods = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  addMethod() {
    if (!this.newMethod.MethodName) return;
    
    if (this.isEditing) {
      this.isLoading = true;
      this.api.put(`PaymentMethod/${this.newMethod.methodId}`, this.newMethod).subscribe({
        next: () => {
          this.resetForm();
          this.showForm = false;
          this.loadMethods();
        },
        error: () => this.isLoading = false
      });
    } else {
      this.isLoading = true;
      this.api.post('PaymentMethod', this.newMethod).subscribe({
        next: () => {
          this.resetForm();
          this.showForm = false;
          this.loadMethods();
        },
        error: () => this.isLoading = false
      });
    }
  }

  editMethod(method: any) {
    this.newMethod = {
      methodId: method.methodId,
      MethodName: method.methodName,
      Description: method.description,
      IsDigital: method.isDigital,
      ExtraChargePercentage: method.extraChargePercentage,
      AccountNumber: method.accountNumber,
      IconPath: method.iconPath,
      QRCodePath: method.qrCodePath,
      MinimumAmount: method.minimumAmount,
      IsActive: method.isActive
    };
    this.isEditing = true;
    this.showForm = true;
  }

  resetForm() {
    this.newMethod = { 
      methodId: 0, 
      MethodName: '', 
      Description: '', 
      IsDigital: false, 
      ExtraChargePercentage: 0, 
      AccountNumber: '', 
      IconPath: '', 
      QRCodePath: '', 
      MinimumAmount: 0, 
      IsActive: true 
    };
    this.isEditing = false;
  }

  deleteMethod(id: number) {
    if (confirm('Delete this payment method?')) {
      this.api.delete(`PaymentMethod/${id}`).subscribe(() => this.loadMethods());
    }
  }
}

