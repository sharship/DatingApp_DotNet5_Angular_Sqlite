import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Basice CRUD methods to communicate with BE:
  
  // Get message: Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForCurrentUser([FromQuery] MessageParams messageParams)
  getMessages(pageNumber: number, pageSize: number, container: string) {

    let params: HttpParams = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('container', container);

    // call BE controller to get message of type specified in http params
    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }
  
  // Get thread: Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string otherusername)
  getMessageThread(otherUsername: string) {
    return this.http.get<Message[]>(this.baseUrl + "messages/thread/" + otherUsername);
  }

  // Create message
  sendMessage(recipientName: string, content: string) {
    return this.http.post<Message>(this.baseUrl + "messages", {
      recipientUsername: recipientName,
      content
    });
  }

  // Delete message
  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }

}
