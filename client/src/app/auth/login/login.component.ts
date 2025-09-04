import { Component, inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [ReactiveFormsModule],
})
export class LogInComponent implements OnInit {
  private router = inject(Router);

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
  }

  onLogin() {
    if (this.loginForm.invalid) {
      console.log('Invalid form');
      // this.router.navigate(['auth/login']);
      return;
    } else {
      this.authService.logIn(this.loginForm.value.email, this.loginForm.value.password);
      this.router.navigate(['']);
    }

    this.loginForm.reset();
  }

  hasErrors(fieldName: string) : boolean {
    const errors = this.loginForm.get(fieldName)?.errors;
    
    if (errors) {
      return true;
    }

    return false;
  }
}
