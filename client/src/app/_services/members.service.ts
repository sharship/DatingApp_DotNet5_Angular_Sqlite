import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  members: Member[] = [];
  baseUrl = environment.apiUrl;
  constructor(private http: HttpClient) { }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member)
      .pipe(
        map(() => {
          const index = this.members.indexOf(member);
          this.members[index] = member;
        })
      );
  }

  getMembers() {
    if (this.members.length > 0)
      return of(this.members);
    
    return this.http.get<Member[]>(this.baseUrl + 'users')
      .pipe(
        map(members => {
          this.members = members;
          return members;
        })
      );
  }

  getMemberByUsername(username: string) {
    const member = this.members.find(m => m.username === username);
    if (member !== undefined) 
      return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }
}
