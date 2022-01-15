import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/message';
@Injectable({
  providedIn: 'root'
})
export class MessageService {

  baseUrl= environment.apiUrl;

  constructor(private httpClient : HttpClient) { }

  getMessages(pageNumber, pageSize,container) {

    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params,this.httpClient);
  }

  getMessageThread(username :string){
    return this.httpClient.get<Message[]>(this.baseUrl +'messages/thread/' + username);
  }

  sendMessage(username: string, content: string){
    return this.httpClient.post<Message>(this.baseUrl + 'messages/', {recipientUsername: username, content});
  }
  deleteMessage(id: number){
    return this.httpClient.delete(this.baseUrl + 'messages/' + id);
  }
}
