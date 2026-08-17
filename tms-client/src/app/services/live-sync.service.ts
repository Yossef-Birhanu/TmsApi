import { isPlatformBrowser } from '@angular/common';
import {
  inject,
  PLATFORM_ID,
  Injectable,
  signal
} from '@angular/core';

import { Subject } from 'rxjs';

import {
  HubConnection,
  HubConnectionBuilder
} from '@microsoft/signalr';

export interface EnrollmentStatusEvent {
  id: string;
  status: 'Pending' | 'Approved' | 'Rejected';
}

@Injectable({
  providedIn: 'root'
})
export class LiveSyncService {

  private platformId = inject(PLATFORM_ID);

  private connection: HubConnection | null = null;

  private eventsSubject =
    new Subject<EnrollmentStatusEvent>();

  // Store will subscribe to this
  events$ = this.eventsSubject.asObservable();

  // Connection state for UI
  connectionState =
    signal<'connected' | 'reconnecting' | 'disconnected'>(
      'disconnected'
    );

  connect() {

    // Prevent duplicate connections
    if (this.connection) return;

    // SignalR should only run in the browser
    if (!isPlatformBrowser(this.platformId)) return;

    this.connection = new HubConnectionBuilder()
      .withUrl('/hubs/tms')
      .withAutomaticReconnect([
        0,
        2000,
        10000,
        30000
      ])
      .build();

    // Receive enrollment status updates
    this.connection.on(
      'ReceivedEnrollmentStatusUpdated',
      (
        enrollmentId: string,
        status: 'Pending' | 'Approved' | 'Rejected'
      ) => {

        this.eventsSubject.next({
          id: enrollmentId,
          status
        });

      }
    );

    // Reconnecting
    this.connection.onreconnecting(() => {
      this.connectionState.set('reconnecting');
    });

    // Reconnected
    this.connection.onreconnected(() => {
      this.connectionState.set('connected');
    });

    // Disconnected
    this.connection.onclose(() => {
      this.connectionState.set('disconnected');
    });

    // Start connection
    this.connection
      .start()
      .then(() => {
        this.connectionState.set('connected');
        console.log('SignalR connected');
      })
      .catch((err: unknown) => {
        console.error(
          'SignalR connection error:',
          err
        );
      });
  }
}