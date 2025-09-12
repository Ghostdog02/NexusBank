import { HttpClient } from "@angular/common/http";
import { ChangeDetectorRef, inject, Injectable, signal } from '@angular/core';

import { environment } from '../../../environments/environment';
import { AuthData } from './auth-data.model';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { Router } from '@angular/router';
import { FormGroup } from '@angular/forms';

const BACKEND_URL = environment.apiUrl + '/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private httpClient = inject(HttpClient);
  private authStatusListener = new BehaviorSubject<boolean>(this.getInitialValue());
  private router = inject(Router);

  private tokenTimer?: ReturnType<typeof setTimeout>;
  private userId: string | null = null;

  private token: string = '';
  //private hasTimerStarted = signal<boolean>(false);

  public isAuthenticated = signal<boolean>(this.getInitialValue());

  public isAuth: boolean = this.getInitialValue();

  getAuthStatusListener() {
    return this.authStatusListener.asObservable();
  }

  // getAuthSignal() {
  //   return this.isAuthenticated.asReadonly();
  // }

  getIsAuth() {
    return this.isAuth;
  }

  getIsAuthSignal() {
    return this.isAuthenticated();
  }

  getInitialValue() {
    if (this.getAuthData()) {
      return true;
    }

    return false;
  }

  getToken() {
    return this.token;
  }

  getAuthData() {
    const token = localStorage.getItem('token');
    const expirationDate = localStorage.getItem('expiration');
    const userId = localStorage.getItem('userId');

    if (!token || !expirationDate) {
      return;
    }

    return {
      token: token,
      expirationDate: new Date(expirationDate),
      userId: userId,
    };
  }

  async loginUser(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };

    try {
      const response = await firstValueFrom(
        this.httpClient.post<{ token: string; expiresIn: number; userId: string }>(
          BACKEND_URL + '/login',
          authData
        )
      );

      if (response == undefined) {
        throw new Error('Response from loginUser endpoint returned undefined');
      }

      this.token = response!.token;

      if (this.token) {
        const expiresInDuration = response!.expiresIn;

        if (!expiresInDuration) {
          throw new Error('ExpiresInDuration field was null from login endpoint response()');
        }

        this.setAuthTimer(expiresInDuration);

        this.userId = response!.userId;

        if (!this.userId) {
          throw new Error('The user id was null from login endpoint response()');
        }

        const now = new Date();
        const expirationDate = new Date(now.getTime() + expiresInDuration * 1000);
        console.log(expirationDate);

        this.saveAuthData(this.token, expirationDate, this.userId);

        this.isAuthenticated.set(true);
        this.authStatusListener.next(true);
        this.isAuth = true;
        
        

        this.router.navigateByUrl('/');
      }
    } catch (error) {
      this.isAuthenticated.set(false);
      this.authStatusListener.next(false);
      this.isAuth = false;

      console.log(error);
    }
  }

  async createUser(email: string, password: string) {
    const authData: AuthData = { email: email, password: password };

    try {
      const response = await firstValueFrom(
        this.httpClient.post(BACKEND_URL + '/signup', authData)
      );

      if (!response) {
        throw new Error('An error ocurred during user creation');
      }

      this.router.navigate(['/']);
    } catch (error) {
      this.isAuthenticated.set(false);
      this.authStatusListener.next(false);
      this.isAuth = false;
    }
  }

  logoutUser() {
    this.isAuthenticated.set(false);
    this.authStatusListener.next(false);
    this.isAuth = false;

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
      this.isAuthenticated.set(true);
      this.authStatusListener.next(true);
      this.isAuth = true;
    }
  }

  // private updateAuthState(isAuth: boolean) {
  //   this.isAuthenticated.set(isAuth);
  //   this.authStatusListener.next(isAuth);
  // }

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
