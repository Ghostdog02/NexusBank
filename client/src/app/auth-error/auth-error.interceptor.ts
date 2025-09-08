import { HttpErrorResponse, HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';

import { AuthErrorComponent } from './auth-error.component';

import { AuthErrorService } from './auth-error.service';

import { ToastrModule } from 'ngx-toastr';

export function authErrorInterceptor(
  this: any,
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  
  let authErrorService = inject(AuthErrorService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unknown error occurred';

      errorMessage = authErrorService.handleError(error.error);

      dialog.open(AuthErrorComponent, {
        data: { message: errorMessage },
        width: '400px',
        disableClose: false,
      });
      // console.log(error);
      // alert(error.error.message);
      return throwError(() => {
        return error;
      });
    })
  );
}
