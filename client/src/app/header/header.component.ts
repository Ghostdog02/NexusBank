import { ChangeDetectorRef, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { RouterLink } from "@angular/router";
import { Subscription } from "rxjs";

import { AuthService } from "../auth/auth.service";


@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
  imports: [RouterLink],
  // changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  //private authListenerSubs: Subscription = new Subscription();
  private authService = inject(AuthService);
  //private cdr = inject(ChangeDetectorRef);
  //public userIsAuthenticated$: Observable<boolean> = new Observable();

  //public userIsAuthenticated: boolean | null = null;

  public userIsAuthenticated = this.authService.getAuthSignal();

  // ngOnInit(): void {
  //   this.authListenerSubs = this.authService
  //     .getAuthStatusListener()
  //     .subscribe((isAuthenticated) => {
  //       this.userIsAuthenticated = this.cdr.detectChanges();
  //     });
  // }

  onLogoutUser() {
    this.authService.logoutUser();
  }

  // ngOnDestroy(): void {
  //   this.authListenerSubs.unsubscribe();
  // }
}