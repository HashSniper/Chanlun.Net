import * as signalR from '@microsoft/signalr';
import { getBaseUrl } from './chanlunApi';

const HUB_URL = '/chanlunhub';

export function getHubUrl(): string {
  const baseUrl = getBaseUrl();
  return `${baseUrl}${HUB_URL}`;
}

export function createSignalRConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(getHubUrl(), {
      transport: signalR.HttpTransportType.WebSockets |
                 signalR.HttpTransportType.ServerSentEvents |
                 signalR.HttpTransportType.LongPolling,
      withCredentials: false,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
}

export async function startSignalRConnection(): Promise<signalR.HubConnection> {
  const conn = createSignalRConnection();

  conn.onreconnecting((err) => {
    console.warn('[SignalR] Reconnecting...', err);
  });
  conn.onreconnected((connectionId) => {
    console.log('[SignalR] Reconnected. ConnectionId:', connectionId);
  });
  conn.onclose((err) => {
    console.warn('[SignalR] Connection closed.', err);
  });

  await conn.start();
  console.log('[SignalR] Connected to', getHubUrl());
  return conn;
}

export async function stopSignalRConnection(conn: signalR.HubConnection): Promise<void> {
  if (conn.state !== signalR.HubConnectionState.Disconnected) {
    await conn.stop();
  }
}

export function onTdxDataUpdated(
  conn: signalR.HubConnection,
  callback: () => void,
): () => void {
  const wrappedCallback = () => {
    console.log('[SignalR] Received TdxDataUpdated, triggering refresh');
    callback();
  };
  conn.on('TdxDataUpdated', wrappedCallback);
  return () => {
    conn.off('TdxDataUpdated', wrappedCallback);
  };
}
