import { HttpClient } from "@angular/common/http";
import { inject, Injectable, signal } from "@angular/core";

import { environment } from "../../../environments/environment";
import { AuthData } from "./auth-data.model";
import { BehaviorSubject, Subject } from "rxjs";
import { Router, ActivatedRoute } from "@angular/router";
import { FormGroup } from "@angular/forms";

const BACKEND_URL = environment.apiUrl + '/auth' 

@Injectable({ providedIn: 'root' })
export class AuthService {
  private httpClient = inject(HttpClient);
  private authStatusListener = new BehaviorSubject<boolean>(false);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private tokenTimer?: ReturnType<typeof window.setTimeout>;
  private userId: string | null = null;
  //private isAuth = false;
  private token = '';

  public isAuthenticated = signal<boolean>(false);

  getAuthStatusListener() {
    return this.authStatusListener.asObservable();
  }

  getAuthSignal() {
    return this.isAuthenticated;
  }

  // private initializeAuthState(): void {
  //   const authData = this.getAuthData();
  //   if (!authData?.token || !authData?.expirationDate) {
  //     this.authStatusListener.next(false);
  //     return;
  //   }

  //   const now = new Date();
  //   const expiresIn = authData.expirationDate.getTime() - now.getTime();

  //   if (expiresIn > 0) {
  //     // Set internal state when token is valid
  //     this.token = authData.token;
  //     this.userId = authData.userId || null;
  //     this.isAuth = true;
  //     this.setAuthTimer(expiresIn / 1000);
  //     this.authStatusListener.next(true);
  //   } else {
  //     // Token expired, clear it
  //     this.clearAuthData();
  //     this.authStatusListener.next(false);
  //   }
  // }

  // getIsAuth(): boolean {
  //   return this.isAuth;
  // }

  getToken() {
    return this.token;
  }

  getAuthData() {
    const token = window.localStorage.getItem('token');
    const expirationDate = window.localStorage.getItem('expiration');
    const userId = window.localStorage.getItem('userId');

    if (!token || !expirationDate) {
      return;
    }

    return {
      token: token,
      expirationDate: new Date(expirationDate),
      userId: userId,
    };
  }

  loginUser(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient
      .post<{ token: string; expiresIn: number; userId: string }>(BACKEND_URL + '/login', authData)
      .subscribe({
        next: (response) => {
          this.token = response.token;

          if (this.token) {
            const expiresInDuration = response.expiresIn;
            this.setAuthTimer(expiresInDuration);

            this.userId = response.userId;

            this.updateAuthState(true);

            const now = new Date();
            const expirationDate = new Date(now.getTime() + expiresInDuration * 1000);
            console.log(expirationDate);

            this.saveAuthData(this.token, expirationDate, this.userId);
            this.router.navigate(['..']);
            this.router.navigate(['..', 'home'], { relativeTo: this.route });
            this.router.navigate(['..']);
          } else {
            this.updateAuthState(false);
          }
        },
        error: () => {
          this.updateAuthState(false);
        },
      });
  }

  createUser(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };
    this.httpClient.post(BACKEND_URL + '/signup', authData).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: () => {
        this.updateAuthState(false);
      },
    });
  }

  logoutUser() {
    this.updateAuthState(false);
    this.token = '';
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
      this.updateAuthState(true);
    }
  }

  private updateAuthState(isAuth: boolean) {
    this.isAuthenticated.set(isAuth);
    this.authStatusListener.next(isAuth);
  }

  private setAuthTimer(duration: number) {
    console.log('Settinng timer ' + duration);
    this.tokenTimer = setTimeout(() => {
      this.logoutUser();
    }, duration * 1000);
  }

  private saveAuthData(token: string, expirationDate: Date, userId: string) {
    window.localStorage.setItem('token', token);
    window.localStorage.setItem('expiration', expirationDate.toISOString());
    window.localStorage.setItem('userId', userId);
  }

  private clearAuthData() {
    window.localStorage.removeItem('token');
    window.localStorage.removeItem('expiration');
    window.localStorage.removeItem('userId');
  }

  hasValidationErrors(fieldName: string, form: FormGroup) {
    const field = form.get(fieldName);

    return !!(field?.errors && (field?.touched || field?.dirty));
  }
}