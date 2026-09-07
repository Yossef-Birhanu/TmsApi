import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,

  imports: [ RouterLink, RouterLinkActive],

  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {

  aboutMenuOpen = false;

  currentYear = new Date().getFullYear();

  toggleAboutMenu(): void {
    this.aboutMenuOpen = !this.aboutMenuOpen;
  }

  closeAboutMenu(): void {
    this.aboutMenuOpen = false;
  }

}