import { Component, inject } from "@angular/core";
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-auth-error',
  templateUrl: './auth-error.component.html',
})
export class AuthErrorComponent {
  public data : { message: string } = inject(MAT_DIALOG_DATA);
}