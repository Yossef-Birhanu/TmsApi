import { isPlatformBrowser } from '@angular/common';
import { inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder
} from '@microsoft/signalr';
import { Subject } from 'rxjs';

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

  private eventsSubject = new Subject<EnrollmentStatusEvent>();

  // SignalR events exposed to the store
  events$ = this.eventsSubject.asObservable();

  // Connection state exposed to the UI
  connectionState = signal<
    'connected' | 'reconnecting' | 'disconnected'
  >('disconnected');

  connect(): void {

    // Prevent duplicate connections
    if (this.connection) {
      return;
    }

    // SignalR WebSocket connection only runs in browser
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.connection = new HubConnectionBuilder()
      .withUrl('/hubs/tms')
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    // Receive enrollment status changes from .NET
    this.connection.on(
      'ReceiveEnrollmentStatusUpdated',
      (
        enrollmentId: string,
        status: 'Pending' | 'Approved' | 'Rejected'
      ) => {
        console.log(
          'Enrollment status update:',
          enrollmentId,
          status
        );

        this.eventsSubject.next({
          id: enrollmentId,
          status
        });
      }
    );

    // Connection lifecycle
    this.connection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.connectionState.set('reconnecting');
    });

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.connectionState.set('connected');
    });

    this.connection.onclose(() => {
      console.log('SignalR disconnected');
      this.connectionState.set('disconnected');
    });

    // Start connection
    this.connection
      .start()
      .then(() => {
        console.log('SignalR connected');
        this.connectionState.set('connected');
      })
      .catch(err => {
        console.error('SignalR connection error:', err);
        this.connectionState.set('disconnected');
      });
  }
}
