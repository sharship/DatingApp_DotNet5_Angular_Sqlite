import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { map, take } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  // currentUser$: Observable<User>;

  constructor(public accountService: AccountService, private router: Router) { }

  ngOnInit(): void {
    // this.currentUser$ = this.accountService.currentUser$;
  }

  login() {
    this.accountService.login(this.model)
      .subscribe(
        () => {
          this.router.navigateByUrl('/members')
        }
      );
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  // showAdmin() {
  //   this.accountService.currentUser$.pipe(take(1))
  //     .subscribe(user => {
  //       return user.roles.some(role => {
  //         role.includes("Admin") || role.includes("Moderator");
  //       })
  //     })
  // }

}
