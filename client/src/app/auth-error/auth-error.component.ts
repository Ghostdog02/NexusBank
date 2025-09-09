import { HttpErrorResponse } from "@angular/common/http";
import { Component, inject } from "@angular/core";
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-auth-error',
  templateUrl: './auth-error.component.html',
})
export class AuthErrorComponent {
  private apiUrl = 'http://localhost:3000/auth';
  private toastrService = inject(ToastrService);

  handleError(error: HttpErrorResponse): void {
    let errorMessage = '';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `An error occurred: ${error.error.message}`;
    } else {
      switch (error.status) {
        case 401:
          errorMessage = 'Unauthorized. Please log in.';
          break;
        case 403:
          errorMessage = 'Forbidden. You do not have access to this resource.';
          break;
        case 404:
          errorMessage = `Resource not found: ${this.apiUrl}`;
          break;
      }
    }

    this.toastrService.error(errorMessage, 'Error');
  }
}