import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
  effect,
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
  //changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  //private authListenerSubs: Subscription = new Subscription();
  private authService = inject(AuthService);
  //private cdr = inject(ChangeDetectorRef);
  //public userIsAuthenticated$: Observable<boolean> = new Observable();

  //public userIsAuthenticated: boolean | null = null;

  public userIsAuthenticated = computed(() => this.authService.isAuthenticated());

  //public userIsAuthenticated;

  // ngOnInit(): void {
  //   // this.authListenerSubs = this.authService
  //   //   .getAuthStatusListener()
  //   //   .subscribe((isAuthenticated) => {
  //   //     this.userIsAuthenticated = isAuthenticated
  //   //   });

  // }

  constructor() {
    effect(() => {
      this.userIsAuthenticated();
    });
  }

  onLogoutUser() {
    this.authService.logoutUser();
  }

  // ngOnDestroy(): void {
  //   this.authListenerSubs.unsubscribe();
  // }
}
