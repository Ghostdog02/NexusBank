// error.service.ts
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthErrorService {
  private apiUrl = 'http://localhost:3000/auth';
  private httpClient = inject(HttpClient);

  handleError(error: HttpErrorResponse) {
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
        default:
          errorMessage = `An unexpected error occurred. Status code: ${error.status}`;
          break;
      }
    }

    return errorMessage;
  }
}
