import { Routes } from "@angular/router";

import { LogInComponent } from "./login/login.component";
import { SignUpComponent } from "./signup/signup.component";

export const authRoutes: Routes = [
  {
    path: 'login',
    component: LogInComponent,
    //canActivate: [authGuard]
  },
  {
    path: 'signup',
    component: SignUpComponent,
    //canActivate: [authGuard],
  },
];