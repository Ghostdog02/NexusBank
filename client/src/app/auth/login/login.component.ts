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
  private router = inject(Router);

  public authService: AuthService = new AuthService();
  public loginForm: FormGroup = new FormGroup({});

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      email: new FormControl(null, [Validators.required, Validators.email]),
      password: new FormControl(null, [
        Validators.required,
        Validators.pattern(`^(?=.*[A-Za-z])(?=.*d)(?=.*[@$!%*#?&])[A-Za-zd@$!%*#?&]{8,}$`),
      ]),
    });
  }

  onLogin() {
    if (this.loginForm.invalid) {
      console.log('Invalid form');
      this.router.navigate(['auth/login']);
    } else {
      this.authService.login(this.loginForm.value.email, this.loginForm.value.password);
      this.router.navigate(['']);
    }
  }
}
