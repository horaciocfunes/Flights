import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="header">
      <div class="header-inner">
        <a routerLink="/" class="logo">
          <span class="logo-icon">✈</span>
          SKYROUTE
        </a>
      </div>
    </header>
    <main class="main">
      <router-outlet />
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }
    .header {
      background: #0a0f2e;
      border-bottom: 1px solid rgba(201, 168, 76, 0.22);
      padding: 0 2rem;
      height: 60px;
      display: flex;
      align-items: center;
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .header-inner {
      max-width: 1200px;
      margin: 0 auto;
      width: 100%;
      display: flex;
      align-items: center;
    }
    .logo {
      color: #c9a84c;
      text-decoration: none;
      font-size: 1.2rem;
      font-weight: 800;
      letter-spacing: 0.22em;
      display: flex;
      align-items: center;
      gap: 0.6rem;
    }
    .logo-icon { letter-spacing: 0; }
    .main { min-height: calc(100vh - 60px); }
  `]
})
export class AppComponent {}
