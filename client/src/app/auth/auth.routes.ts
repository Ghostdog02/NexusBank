import { RouterModule, Routes } from "@angular/router";

import { LogInComponent } from "./login/login.component";
import { SignUpComponent } from "./signup/signup.component";
import { NgModule } from "@angular/core";
import { routes } from "../app.routes";

export const authRoutes: Routes = [
  {
    path: 'login',
    component: LogInComponent,
  },
  {
    path: 'signup',
    component: SignUpComponent,
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
];