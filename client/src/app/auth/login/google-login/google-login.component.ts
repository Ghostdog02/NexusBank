import { GoogleSigninButtonModule, SocialAuthService } from '@abacritt/angularx-social-login';
import { Component, inject } from '@angular/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-google-login',
  imports: [GoogleSigninButtonModule],
  providers: [],
  templateUrl: './google-login.component.html',
  styleUrl: './google-login.component.css',
})
export class GoogleLoginComponent {
  socialAuthService = inject(SocialAuthService);
  socialSubscription!: Subscription;

  ngOnInit(): void {
    this.socialSubscription = this.socialAuthService.authState.subscribe((user) => {
      console.log(user);
      //perform further logics
    });
  }

  handleOauthResponse() {}

  ngOnDestroy() {
    this.socialSubscription.unsubscribe();
  }
}
