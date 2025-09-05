import { Component, inject, OnInit } from "@angular/core";
import { RouterLink } from "@angular/router";
import { AuthService } from "../auth/auth.service";

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
  imports: [RouterLink],
})
export class HeaderComponent implements OnInit{
  authService = inject(AuthService);
  isAuthenticated: boolean | null = null;
  
  ngOnInit(): void {
    this.authService.getAuthStatusListener()
      .subscribe((authStatus) => {
        this.isAuthenticated = authStatus;
      });
  }

  onLogoutUser() {
    this.authService.logoutUser();
  }
}