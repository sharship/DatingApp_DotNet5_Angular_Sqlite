import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;

  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  createHubConnection(caller: User, recipientName: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + "message?user=" + recipientName, {
        accessTokenFactory: () => caller.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .catch(err => console.error(err));

    
    this.hubConnection.on('ReceiveMessageThread', (messages: Message[]) => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('NewMessage', (message: Message) => {

      this.messageThread$.pipe(take(1)).subscribe(messages => {
        this.messageThreadSource.next([...messages, message]);
      });

    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections.some(conn => conn.username === recipientName)) {
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          messages.forEach(msg => {
            if (!msg.dateTimeRead) {
              msg.dateTimeRead = new Date(Date.now());
            }
          });
          this.messageThreadSource.next([...messages]);
        })
      }
    });

  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }


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
  async sendMessage(recipientName: string, content: string) {
    // return this.http.post<Message>(this.baseUrl + "messages", {
    //   recipientUsername: recipientName,
    //   content
    // });

    return this.hubConnection.invoke('SendMessage', {
        recipientUsername: recipientName,
        content
      })
      .catch(error => {
        console.log(error);
      });
  }

  // Delete message
  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }

}
