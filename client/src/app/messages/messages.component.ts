import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  // Outputs:
  messages: Message[] = [];
  pagination: Pagination;

  // Inputs:
  container: string = "Unread";
  pageNumber: number = 1;
  pageSize: number =5;

  // functional properties
  loading: boolean = false;

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  // Basic CRUD methods to communicate with services
  // Get messages for current user
  loadMessages() {
    this.loading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe(
      response => {
        this.messages = response.result;
        this.pagination = response.pagination;
        this.loading = false;
      }
    );
    
  }

  // Delete message
  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe(
      () => {
        this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
      }
    )
  }

  // Functional methods:

  // to support PaginationModule
  pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadMessages();
  }

}
