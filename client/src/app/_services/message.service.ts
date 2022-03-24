import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/message';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { Group } from '../_models/group';
import { MessagesComponent } from '../messages/messages.component';
@Injectable({
  providedIn: 'root',
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThreadSource$ = this.messageThreadSource.asObservable();

  constructor(private httpClient: HttpClient) {}

  createHubConnection(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch((err) => console.log(err, 'Error'));

    this.hubConnection.on('ReceiveMessageThread', (messageThread) => {
      this.messageThreadSource.next(messageThread);
    });

    this.hubConnection.on('NewMessage', (message) => {
      this.messageThreadSource$.pipe(take(1)).subscribe((messageThread) => {
        this.messageThreadSource.next([...messageThread, message]);
      });
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections.some((x) => x.username === otherUsername)) {
        this.messageThreadSource$.pipe(take(1)).subscribe((messageThread) => {
          messageThread.forEach((message) => {
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          });
          this.messageThreadSource.next([...messageThread]);
        });
      }
    });
  }

  stopHubConnection() {
    if (this.hubConnection.on) {
      this.hubConnection.stop().catch((err) => console.log(err, 'Error'));
    } else if (this.hubConnection) {
      this.hubConnection.stop().catch((err) => console.log(err, 'Error'));
    }
  }

  getMessages(pageNumber, pageSize, container) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(
      this.baseUrl + 'messages',
      params,
      this.httpClient
    );
  }

  getMessageThread(username: string) {
    return this.httpClient.get<Message[]>(
      this.baseUrl + 'messages/thread/' + username
    );
  }

  async sendMessage(username: string, content: string) {
    // return this.httpClient.post<Message>(this.baseUrl + 'messages/', {
    //   recipientUsername: username,
    //   content,
    // });

    return this.hubConnection
      .invoke('SendMessage', { recipientUsername: username, content })
      .catch((error) => console.log(error, 'Error'));
  }
  deleteMessage(id: number) {
    return this.httpClient.delete(this.baseUrl + 'messages/' + id);
  }
}
