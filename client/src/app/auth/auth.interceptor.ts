import { HttpEvent, HttpHandlerFn, HttpRequest } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuthService } from "./auth.service";
import { inject } from "@angular/core";

export function authInterceptor(this: any, 
    req: HttpRequest<unknown>,
    next: HttpHandlerFn
) : Observable<HttpEvent<unknown>> {
  const authService = inject(AuthService);
  const authToken = authService.getToken();
  const authRequest = req.clone({
      headers: req.headers.set("Authorization", "Bearer " + authToken)
  });
    
  return next(authRequest);
}