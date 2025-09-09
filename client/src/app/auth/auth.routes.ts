import { Routes } from "@angular/router";

import { LoginComponent } from "./login/login.component";
import { SignupComponent } from "./signup/signup.component";

export const authRoutes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'signup',
    component: SignupComponent,
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
];