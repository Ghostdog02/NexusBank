import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import { AuthErrorService } from './auth-error.service';
import { AuthErrorComponent } from './auth-error.component';

export function authErrorInterceptor(
  this: any,
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  
  let authErrorComponent = inject(AuthErrorComponent);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      authErrorComponent.handleError(error.error);

      return throwError(() => {
        return error;
      });
    })
  );
}
