import * as signalR from "@microsoft/signalr";
import { getToken, HUB_URL } from "./session";

export function nextReconnectDelayMs(attempt: number): number {
  const cap = Math.min(30_000, 1000 * 2 ** attempt);
  return cap / 2 + Math.random() * (cap / 2);
}

const reconnectPolicy: signalR.IRetryPolicy = {
  nextRetryDelayInMilliseconds: ({ previousRetryCount }) =>
    nextReconnectDelayMs(previousRetryCount),
};

export function createConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, { accessTokenFactory: getToken })
    .withAutomaticReconnect(reconnectPolicy)
    .configureLogging(signalR.LogLevel.Information)
    .build();
}
