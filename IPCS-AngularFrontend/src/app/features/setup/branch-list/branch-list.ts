import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';
import { ImageUploadComponent } from '../../../shared/image-upload/image-upload';

/**
 * BranchListComponent
 * Manages the different physical pharmacy locations (branches).
 * Includes branch name, address, and contact information.
 */
@Component({
  selector: 'app-branch-list',
  standalone: true,
  imports: [CommonModule, FormsModule, ImageUploadComponent],
  templateUrl: './branch-list.html'
})
export class BranchListComponent implements OnInit {
  branches: any[] = [];
  filteredBranches: any[] = [];
  newBranch = { branchName: '', address: '', contactNumber: '', email: '', managerName: '', picturePath: '', isActive: true };
  isLoading = false;
  isUploading = false;
  showForm = false;
  searchTerm = '';

  // Editing state
  editingBranchId: number | null = null;
  editBranch: any = {};

  // Details state
  selectedBranchForDetails: any = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadBranches();
  }

  showDetails(branch: any) {
    this.selectedBranchForDetails = branch;
  }

  closeDetails() {
    this.selectedBranchForDetails = null;
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  onSearch() {
    const term = this.searchTerm.toLowerCase();
    this.filteredBranches = this.branches.filter(b => 
      (b.branchName && b.branchName.toLowerCase().includes(term)) ||
      (b.address && b.address.toLowerCase().includes(term))
    );
  }

  /**
   * Fetches the list of all active branches
   */
  loadBranches() {
    this.isLoading = true;
    this.api.get<any[]>('Branch').subscribe({
      next: (data) => {
        this.branches = data;
        this.filteredBranches = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  async onImageSelected(file: File, target: 'new' | 'edit' = 'new') {
    this.isUploading = true;
    try {
      this.api.uploadFile(file).subscribe({
        next: (response: any) => {
          const filePath = response.filePath || response.FilePath;
          if (target === 'new') {
            this.newBranch.picturePath = filePath;
          } else {
            this.editBranch.picturePath = filePath;
          }
          this.isUploading = false;
        },
        error: (err) => {
          console.error('Upload failed:', err);
          alert('Image upload failed: ' + (err.error?.Message || err.message));
          this.isUploading = false;
        }
      });
    } catch (err) {
      console.error('Upload error:', err);
      alert('Image upload failed');
      this.isUploading = false;
    }
  }

  onImageRemoved(target: 'new' | 'edit' = 'new') {
    if (target === 'new') {
      this.newBranch.picturePath = '';
    } else {
      this.editBranch.picturePath = '';
    }
  }

  /**
   * Adds a new branch to the system
   */
  addBranch() {
    if (!this.newBranch.branchName) return;
    this.isLoading = true;
    this.api.post('Branch', this.newBranch).subscribe({
      next: () => {
        this.newBranch = { branchName: '', address: '', contactNumber: '', email: '', managerName: '', picturePath: '', isActive: true };
        this.showForm = false;
        this.loadBranches();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Starts editing a branch
   */
  startEdit(branch: any) {
    this.editingBranchId = branch.branchId;
    this.editBranch = { ...branch };
  }

  /**
   * Cancels editing
   */
  cancelEdit() {
    this.editingBranchId = null;
    this.editBranch = {};
  }

  /**
   * Saves the updated branch
   */
  updateBranch() {
    if (!this.editBranch.branchName || !this.editingBranchId) return;
    this.isLoading = true;
    this.api.put(`Branch/${this.editingBranchId}`, this.editBranch).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadBranches();
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Deletes a branch (soft delete)
   */
  deleteBranch(id: number) {
    if (confirm('Delete this branch?')) {
      this.api.delete(`Branch/${id}`).subscribe(() => this.loadBranches());
    }
  }

  getImageUrl(path: string): string {
    if (!path) return '';
    if (path.startsWith('http') || path.startsWith('data:')) {
      return path;
    }
    return `https://localhost:7054${path}`;
  }
}

