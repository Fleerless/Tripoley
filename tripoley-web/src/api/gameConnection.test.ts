import * as signalR from "@microsoft/signalr";
import { afterEach, describe, expect, it, vi } from "vitest";

afterEach(() => {
  vi.unstubAllEnvs();
  vi.resetModules();
});

describe("createConnection", () => {
  it("builds a disconnected HubConnection on the default hub URL", async () => {
    const { createConnection } = await import("./gameConnection");

    const conn = createConnection();

    expect(conn).toBeInstanceOf(signalR.HubConnection);
    expect(conn.baseUrl).toContain("/hubs/gamehub");
    expect(conn.state).toBe(signalR.HubConnectionState.Disconnected);
  });

  it("uses VITE_HUB_URL when it is set", async () => {
    vi.stubEnv("VITE_HUB_URL", "http://example.test/hub");
    vi.resetModules();

    const { createConnection } = await import("./gameConnection");

    expect(createConnection().baseUrl).toBe("http://example.test/hub");
  });
});
