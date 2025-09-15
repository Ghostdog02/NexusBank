import { ApplicationRef, Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [ReactiveFormsModule],
  providers: [ApplicationRef],
})
export class LoginComponent implements OnInit {
  public authService: AuthService = new AuthService();
  public loginForm: FormGroup = new FormGroup({});

  ngOnInit(): void {
    this.loginForm = new FormGroup({
      email: new FormControl(null, {
        validators: [Validators.required, Validators.email],
      }),
      password: new FormControl(null, {
        validators: [
          Validators.required,
          Validators.maxLength(64),
          Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$/),
        ],
      }),
    });

    this.loginForm.markAllAsTouched();
  }

  async onSubmit() {
    if (this.loginForm.invalid) {
      console.log('Invalid form');
      return;
    }

    await this.authService.loginUser(this.loginForm.value.email, this.loginForm.value.password);

    this.loginForm.reset();
  }

  hasErrors(fieldName: string): boolean {
    return this.authService.hasValidationErrors(fieldName, this.loginForm);
  }
}
