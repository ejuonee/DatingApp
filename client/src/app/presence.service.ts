import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { on } from 'events';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from './_models/user';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  hubUrl: string = environment.hubUrl;
  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);

  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) {}

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: (): string => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch((err) => {
      console.log(err, 'Error');
      this.toastr.error(err.message, 'Error');
    });
    this.hubConnection.on('UserIsOnline', (username) => {
      // this.toastr.info(username + ' is online', 'Info');
      this.onlineUsers$.pipe(take(1)).subscribe((usernames) => {
        this.onlineUsersSource.next([...usernames, username]);
      });
    });
    this.hubConnection.on('UserIsOffline', (username) => {
      // this.toastr.warning(username + ' is offline', 'Warning');

      this.onlineUsers$.pipe(take(1)).subscribe((usernames) => {
        this.onlineUsersSource.next([
          ...usernames.filter((u) => u !== username),
        ]);
      });
    });

    this.hubConnection.onclose(() => {
      this.toastr.error('Connection closed', 'Error');
    });
    this.hubConnection.on('GetOnlineUsers', (username: string[]) => {
      this.onlineUsersSource.next(username);
    });

    this.hubConnection.on('NewMessageReceived', ({ username, knownAs }) => {
      this.toastr
        .info(knownAs + ' has sent you a message', 'Info')
        .onTap.pipe(take(1))
        .subscribe(() => {
          this.router.navigateByUrl('/members/' + username + '?tab=3');
        });
    });
  }

  stopHubConnection() {
    this.hubConnection.stop().catch((err) => {
      console.log(err, 'Error');
      this.toastr.error(err.message, 'Error');
    });
  }
}
