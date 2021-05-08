import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1); // act as a buffer to store latest user
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, private presence: PresenceService) { }

  login(model: any) {
    return this.http.post(this.baseUrl + "account/login", model)
      .pipe(
        map((user : User) =>{
          if (user) {
            this.setCurrentUser(user);
            this.presence.createHubConnection(user);
          }
          return user;
        })
      )
  }

  register(model: any) {
    return this.http.post(this.baseUrl + "account/register", model)
      .pipe(
        map((user : User) =>{
          if (user) {
            this.setCurrentUser(user);
            this.presence.createHubConnection(user);
          }
          return user;
        })
      );
  }

  setCurrentUser (user: User) {
    // get role payload from token, and push them into user roles array
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);

    localStorage.setItem("user", JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem("user");
    this.currentUserSource.next(null);
    this.presence.stopHubConnection();
  }

  getDecodedToken(token) {
    // The atob() method decodes a base-64 encoded string.
    return JSON.parse(atob(token.split('.')[1]));
  }

  
}
