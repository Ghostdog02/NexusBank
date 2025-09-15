import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./header/header.component";
import { AuthService } from './auth/auth.service';
import { LoginComponent } from './auth/login/login.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent],
  providers: [LoginComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  protected readonly title = signal('client');
  private authService = inject(AuthService);

  ngOnInit(): void {
    this.authService.autoAuthUser();
  }
}
