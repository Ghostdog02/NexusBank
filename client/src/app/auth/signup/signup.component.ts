import { Component, inject } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { AuthService } from "../auth.service";
import { Router } from "@angular/router";

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  imports: [ReactiveFormsModule]
})
export class SignUpComponent {
  private router = inject(Router);

  public authService: AuthService = new AuthService();
  public signUpForm: FormGroup = new FormGroup({});

  ngOnInit(): void {
    this.signUpForm = new FormGroup({
      email: new FormControl(null, [Validators.required, Validators.email]),
      password: new FormControl(null, [
        Validators.required,
        Validators.maxLength(64),
        Validators.pattern(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$/),
      ]),
    });

    this.signUpForm.markAllAsTouched();
  }

  onSignUp() {
    if (this.signUpForm.invalid) {
      console.log('Invalid form');
      return;
      // this.router.navigate(['auth/login']);
    } else {
      this.authService.createUser(this.signUpForm.value.email, this.signUpForm.value.password);
    }

    this.signUpForm.reset();
  }

  hasErrors(fieldName: string): boolean {
    return this.authService.hasValidationErrors(fieldName, this.signUpForm);
  }
}