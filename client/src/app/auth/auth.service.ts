import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";

import { environment } from "../../../environments/environment";
import { AuthData } from "./auth-data.model";
import { BehaviorSubject, Subject } from "rxjs";

const BACKEND_URL = environment.apiUrl + '/auth' 

@Injectable({ providedIn: 'root' })
export class AuthService {
  private httpClient = inject(HttpClient);
  private authStatusListener = new BehaviorSubject<boolean>(false);

  //isLoggedIn = false;
  token = '';

  logIn(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient
      .post<{ token: string; expiresIn: number; userId: string }>(BACKEND_URL + '/login', authData)
      .subscribe({
        next: (response) => {
          this.token = response.token;
          const duration = response.expiresIn;
          this.authStatusListener.next(true);
        },
      });
  }

  signUp(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient.post(BACKEND_URL + '/signup', authData).subscribe({
      next: (response) => {
        console.log(response);
      },
    });
  }

  getAuthStatusListener() {
    return this.authStatusListener.asObservable();
  }
}