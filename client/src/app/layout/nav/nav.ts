import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms'; // <-- 1. Import this
import { AccountService } from '../../../core/services/account-service';


@Component({
  selector: 'app-nav',
  imports: [FormsModule],
  templateUrl: './nav.html',
  styleUrl: './nav.css',
})
export class Nav {
  protected accountService = inject(AccountService);
  protected creds: any = {};
  protected loggedIn = signal(false);
  protected currentUser = signal<any>(null);

  login() {
    this.accountService.login(this.creds).subscribe({
      next: (result: any) => {
        console.log(result);
        this.loggedIn.set(true);
        this.currentUser.set(result);
        this.creds = {}
      },
      error: (error) => alert(error.message),
    });
  }

  logout() {
    this.accountService.logout()
  }
}
