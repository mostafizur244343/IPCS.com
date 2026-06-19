import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api';

/**
 * UserListComponent
 * Manages the list of pharmacy staff and administrators.
 * Shows account status and assigned roles.
 */
@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-list.html'
})
export class UserListComponent implements OnInit {
  users: any[] = []; // List of users with their roles
  isLoading = false;

  // Details state
  selectedUserForDetails: any = null;

  constructor(private api: ApiService) {}

  showDetails(user: any) {
    this.selectedUserForDetails = user;
  }

  closeDetails() {
    this.selectedUserForDetails = null;
  }

  ngOnInit() {
    this.loadUsers();
  }

  /**
   * Fetches all registered users and their assigned roles
   */
  loadUsers() {
    this.isLoading = true;
    this.api.get<any[]>('Role/users-with-roles').subscribe({
      next: (data) => {
        this.users = data;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  /**
   * Deletes a user account (soft delete or Identity delete)
   */
  deleteUser(userId: string) {
    if (confirm('Are you sure you want to delete this user? Access will be revoked immediately.')) {
      this.api.delete(`Account/delete-user/${userId}`).subscribe(() => {
        this.loadUsers();
      });
    }
  }
}
