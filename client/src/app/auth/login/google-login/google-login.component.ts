import { GoogleLoginProvider, SocialAuthService } from '@abacritt/angularx-social-login';
import { Component, inject } from '@angular/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-google-login',
  imports: [],
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

  signInWithGoogle() {
    this.socialAuthService.signIn(GoogleLoginProvider.PROVIDER_ID);
  }

  ngOnDestroy() {
    this.socialSubscription.unsubscribe();
  }
}
