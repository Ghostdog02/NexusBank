import { Component, signal } from '@angular/core';
// import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from "./header/header.component";
import { HomeComponent } from './home/home.component';

@Component({
  selector: 'app-root',
  imports: [HeaderComponent, HomeComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('client');
}
