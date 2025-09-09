import { Component, inject, OnDestroy, OnInit } from "@angular/core";
import { RouterLink, RouterLinkActive } from "@angular/router";
import { AuthService } from "../auth/auth.service";
// import { Subscription } from "rxjs";
import { AsyncPipe } from "@angular/common";
import { Observable, Subscription } from "rxjs";


@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
  imports: [RouterLink, RouterLinkActive],
})
export class HeaderComponent implements OnInit, OnDestroy {
  private authListenerSubs: Subscription = new Subscription();
  private authService = inject(AuthService);
  // public userIsAuthenticated$!: Observable<boolean>;

  public userIsAuthenticated: boolean = false;

  ngOnInit(): void {
    this.userIsAuthenticated = this.authService.getIsAuth();
    this.authListenerSubs = this.authService
      .getAuthStatusListener()
      .subscribe((isAuthenticated) => {
        this.userIsAuthenticated = isAuthenticated;
      });
    // this.authService.getAuthStatusListener();
  }

  onLogoutUser() {
    this.authService.logoutUser();
  }

  ngOnDestroy(): void {
    this.authListenerSubs.unsubscribe();
  }
}