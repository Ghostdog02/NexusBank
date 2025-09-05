import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";

import { environment } from "../../../environments/environment";
import { AuthData } from "./auth-data.model";
import { BehaviorSubject, Subject } from "rxjs";
import { Router } from "@angular/router";
import { FormGroup } from "@angular/forms";

const BACKEND_URL = environment.apiUrl + '/auth' 

@Injectable({ providedIn: 'root' })
export class AuthService {
  private httpClient = inject(HttpClient);
  private authStatusListener = new BehaviorSubject<boolean>(false);
  private router = inject(Router);
  private tokenTimer?: ReturnType<typeof window.setTimeout>;
  private userId = '';
  private isAuth = false;
  private token = '';

  getAuthStatusListener() {
    return this.authStatusListener.asObservable();
  }

  getIsAuth() : boolean {
    return this.isAuth;
  }

  getToken() {
    return this.token;
  }

  getAuthData() {
    const token = localStorage.getItem('token');
    const expirationDate = localStorage.getItem('expiration');
    const userId = localStorage.getItem('userId');

    if (!token || !expirationDate || !userId) {
      return;
    }

    return {
      token: token,
      expirationDate: new Date(expirationDate),
      userId: userId,
    };
  }

  logIn(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient
      .post<{ token: string; expiresIn: number; userId: string }>(BACKEND_URL + '/login', authData)
      .subscribe({
        next: (response) => {
          this.token = response.token;

          if (this.token) {
            const expiresInduration = response.expiresIn;
            this.setAuthTimer(expiresInduration);

            this.userId = response.userId;

            this.authStatusListener.next(true);
            this.isAuth = true;

            const now = new Date();
            const expirationDate = new Date(now.getTime() + expiresInduration * 1000);
            console.log(expirationDate);

            this.saveAuthData(this.token, expirationDate, this.userId);
            this.router.navigate(['/']);
          }
        },
        error: () => this.authStatusListener.next(false),
      });
  }

  createUser(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient.post(BACKEND_URL + '/signup', authData).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => this.authStatusListener.next(false),
    });
  }

  logoutUser() {
    this.token = '';
    this.authStatusListener.next(false);
    this.isAuth = false;
    this.userId = '';
    
    clearTimeout(this.tokenTimer);
    this.clearAuthData();

    this.router.navigate(['/']);
  }

  
  autoAuthUser() {
    const authInformation = this.getAuthData();

    const now = new Date();

    if (!authInformation?.expirationDate || !authInformation.token) {
      return;
    }

    const expiresIn = authInformation?.expirationDate.getTime() - now.getTime();

    if (expiresIn > 0) {
      this.token = authInformation.token;
      this.userId = authInformation.userId;
      this.setAuthTimer(expiresIn / 1000);
      this.authStatusListener.next(true);
      this.isAuth = true;
    }
  }

  private setAuthTimer(duration: number) {
    console.log('Settinng timer ' + duration);
    this.tokenTimer = setTimeout(() => {
      this.logoutUser();
    }, duration * 1000);
  }

  private saveAuthData(token: string, expirationDate: Date, userId: string) {
    localStorage.setItem('token', token);
    localStorage.setItem('expiration', expirationDate.toISOString());
    localStorage.setItem('userId', userId);
  }

  private clearAuthData() {
    localStorage.removeItem('token');
    localStorage.removeItem('expiration');
    localStorage.removeItem('userId');
  }

  hasValidationErrors(fieldName: string, form: FormGroup) {
    const field = form.get(fieldName);

    return !!(field?.errors && (field?.touched || field?.dirty));
  }
}