import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [ReactiveFormsModule],
})
export class LoginComponent implements OnInit {
  //private router = inject(Router);

  public authService: AuthService = new AuthService();
  public loginForm: FormGroup = new FormGroup({});

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      email: new FormControl(null, [Validators.required, Validators.email]),
      password: new FormControl(null, [
        Validators.required,
        Validators.maxLength(64),
        Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$/),
      ]),
    });

    this.loginForm.markAllAsTouched();
  }

  onLogin() {
    if (this.loginForm.invalid) {
      console.log('Invalid form');
      // this.router.navigate(['auth/login']);
      return;
    } else {
      this.authService.isAuthenticated.set(false);
      this.authService.loginUser(this.loginForm.value.email, this.loginForm.value.password);
    }

    this.loginForm.reset();
  }

  hasErrors(fieldName: string): boolean {
    return this.authService.hasValidationErrors(fieldName, this.loginForm);
  }
}
