import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
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
  @Input() messages: Message[];
  @Input() targetUsername: string;

  // Outputs
  messageContent: string;
  @ViewChild('messageForm') messageForm: NgForm;

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {}

  // Basic CRUD methods:
  sendMessage() {
    this.messageService.sendMessage(this.targetUsername, this.messageContent).subscribe(
      message => {
        this.messages.push(message);
        this.messageForm.reset();
      }
    )
  }


}
