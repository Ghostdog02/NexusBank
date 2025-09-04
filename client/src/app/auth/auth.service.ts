import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";

import { environment } from "../../../environments/environment";
import { AuthData } from "./auth-data.model";

const BACKEND_URL = environment.apiUrl + '/auth' 

@Injectable({ providedIn: 'root' })
export class AuthService {
  private httpClient = inject(HttpClient);

  isLoggedIn = false;

  logIn(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient
      .post<{ token: string; expiresIn: number; userId: string }>(BACKEND_URL + '/login', authData)
      .subscribe({
        next: (response) => {
          // const token = response.token;
          // const duration = response.expiresIn;
          this.isLoggedIn = true;
          console.log(this.isLoggedIn);
        },
      });
  }

  signUp(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient
      .post(BACKEND_URL + '/signup', authData)
      .subscribe({
        next: (response) => {
          console.log(response);
        }
      });
  }
}