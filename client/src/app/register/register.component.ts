import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  model: any = {};
  @Output() cancelRegister = new EventEmitter();

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
  }

  register() {
    this.accountService.register(this.model)
      .subscribe(
        data => {
          console.log(data);
          this.cancel();
        },
        err => console.log(err)
      );
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

}
