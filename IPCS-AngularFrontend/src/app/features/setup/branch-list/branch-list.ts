import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../../core/services/api';
import { FormsModule } from '@angular/forms';

/**
 * BranchListComponent
 * Manages the different physical pharmacy locations (branches).
 * Includes branch name, address, and contact information.
 */
@Component({
  selector: 'app-branch-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './branch-list.html'
})
export class BranchListComponent implements OnInit {
  branches: any[] = [];
  newBranch = { branchName: '', address: '', phone: '' };
  isLoading = false;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.loadBranches();
  }

  /**
   * Fetches the list of all active branches
   */
  loadBranches() {
    this.isLoading = true;
    this.api.get<any[]>('Branch').subscribe({
      next: (data) => {
        this.branches = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Adds a new branch to the system
   */
  addBranch() {
    if (!this.newBranch.branchName) return;
    this.isLoading = true;
    this.api.post('Branch', this.newBranch).subscribe({
      next: () => {
        this.newBranch = { branchName: '', address: '', phone: '' };
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
}
