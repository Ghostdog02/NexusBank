import {
  Component,
  inject,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { RouterLink } from '@angular/router';

import { AuthService } from '../auth/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
  imports: [RouterLink],
})
export class HeaderComponent implements OnInit, OnDestroy {
  private authListenerSubs: Subscription = new Subscription();
  public authService = inject(AuthService);
  
  public userIsAuthenticated: boolean = false;
  public isAuthSignal!: boolean;

  ngOnInit(): void {
    this.userIsAuthenticated = this.authService.getIsAuth();
    this.authListenerSubs = this.authService
      .getAuthStatusListener()
      .subscribe((isAuthenticated) => (this.userIsAuthenticated = isAuthenticated));
  }
  
  onLogoutUser() {
    this.authService.logoutUser();
  }

  ngOnDestroy(): void {
    this.authListenerSubs.unsubscribe();
  }
}
