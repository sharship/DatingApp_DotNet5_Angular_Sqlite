import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-message-thread',
  templateUrl: './member-message-thread.component.html',
  styleUrls: ['./member-message-thread.component.css']
})
export class MemberMessageThreadComponent implements OnInit {
  // Inputs
  messages: Message[];
  @Input() targetUsername: string;

  // Outputs
  messageContent: string;
  @ViewChild('messageForm') messageForm: NgForm;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
    // this.messageService.messageThread$.pipe(take(1)).subscribe(messages => {
    //   this.messages = messages;
    // });
  }

  // Basic CRUD methods:
  sendMessage() {
    this.messageService.sendMessage(this.targetUsername, this.messageContent).then(
      () => {
        this.messageForm.reset();
      }
    )
  }


}
