import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  loading: boolean = false;

  constructor(public messageService: MessageService) { }

  ngOnInit(): void {
  }

  // Basic CRUD methods:
  sendMessage() {
    this.loading = true;

    this.messageService
      .sendMessage(this.targetUsername, this.messageContent)
      .then(() => {
        this.messageForm.reset();
      })
      .finally(() => {
        this.loading = false;
      })
  }


}
