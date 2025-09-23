import { GoogleSigninButtonModule, SocialAuthService } from '@abacritt/angularx-social-login';
import { Component, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from '../../auth.service';

@Component({
  selector: 'app-google-login',
  imports: [GoogleSigninButtonModule],
  providers: [],
  templateUrl: './google-login.component.html',
  styleUrl: './google-login.component.css',
})
export class GoogleLoginComponent {
  socialAuthService = inject(SocialAuthService);
  authService = inject(AuthService);
  socialSubscription!: Subscription;

  ngOnInit(): void {
    this.socialSubscription = this.socialAuthService.authState.subscribe((user) => {
      // this.authService.loginUserWithGoogle(user);
    });
  }

  ngOnDestroy() {
    this.socialSubscription.unsubscribe();
  }
}
