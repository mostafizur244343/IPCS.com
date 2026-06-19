import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../../core/services/notification';
import { ApiService } from '../../core/services/api';
import Swal from 'sweetalert2';

@Component({
    template: ''
})
export abstract class BaseFeatureComponent {
    protected router = inject(Router);
    protected notification = inject(NotificationService);
    protected api = inject(ApiService);
    
    isLoading = false;

    ngOnInit(): void {}

    /**
     * Common alert for confirmation before deletion
     */
    async confirmDelete(title: string = 'Are you sure?', text: string = "You won't be able to revert this!"): Promise<boolean> {
        const result = await Swal.fire({
            title,
            text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        });
        return result.isConfirmed;
    }

    /**
     * Standard error handler for components (optional if using Interceptor)
     */
    handleError(error: any) {
        this.isLoading = false;
        console.error('An error occurred:', error);
        // ErrorInterceptor already shows SweetAlert, but we can add extra logic here
    }
}
