import { GoogleSigninButtonDirective, SocialAuthService } from '@abacritt/angularx-social-login';
import { Component, inject } from '@angular/core';

@Component({
  selector: 'app-google-login',
  imports: [GoogleSigninButtonDirective],
  providers: [],
  templateUrl: './google-login.component.html',
  styleUrl: './google-login.component.css',
})
export class GoogleLoginComponent {
  socialAuthService = inject(SocialAuthService);

  ngOnInit(): void {
    this.socialAuthService.authState.subscribe((user) => {
      console.log(user);
      //perform further logics
    });
  }
}
