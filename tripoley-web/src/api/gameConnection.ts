import * as signalR from "@microsoft/signalr";

const HUB_URL =
  (import.meta.env.VITE_HUB_URL as string | undefined) ??
  "http://localhost:5106/hubs/gamehub";

export function createConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
}
