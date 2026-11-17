import { act, renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  HubMethods,
  ServerEvents,
  type CardDto,
  type GameStateView,
  type OpenRoomView,
  type RoomView,
  type SelfView,
} from "./api/contracts";

const { connection } = vi.hoisted(() => {
  const handlers: Record<string, (arg: unknown) => void> = {};
  const lifecycle: Record<string, () => void> = {};
  return {
    connection: {
      handlers,
      lifecycle,
      on: vi.fn((event: string, cb: (arg: unknown) => void) => {
        handlers[event] = cb;
      }),
      onreconnecting: vi.fn((cb: () => void) => {
        lifecycle.reconnecting = cb;
      }),
      onreconnected: vi.fn((cb: () => void) => {
        lifecycle.reconnected = cb;
      }),
      onclose: vi.fn((cb: () => void) => {
        lifecycle.closed = cb;
      }),
      start: vi.fn(() => Promise.resolve()),
      stop: vi.fn(() => Promise.resolve()),
      invoke: vi.fn(() => Promise.resolve()),
    },
  };
});

vi.mock("./api/gameConnection", () => ({
  createConnection: () => connection,
  nextReconnectDelayMs: () => 10,
}));

import { useGame } from "./useGame";

beforeEach(() => {
  connection.start.mockReset().mockResolvedValue(undefined);
  connection.stop.mockReset().mockResolvedValue(undefined);
  connection.invoke.mockReset().mockResolvedValue(undefined);
  connection.on.mockClear();
  for (const key of Object.keys(connection.handlers)) delete connection.handlers[key];
  for (const key of Object.keys(connection.lifecycle)) delete connection.lifecycle[key];
});

async function mountConnected() {
  const view = renderHook(() => useGame());
  await waitFor(() => expect(view.result.current.connected).toBe(true));
  return view;
}

describe("useGame", () => {
  it("starts the connection, subscribes to every server event, and reports connected", async () => {
    await mountConnected();

    expect(connection.start).toHaveBeenCalledOnce();
    for (const event of Object.values(ServerEvents)) {
      expect(connection.on).toHaveBeenCalledWith(event, expect.any(Function));
    }
  });

  it("surfaces a start failure as an error and stays disconnected", async () => {
    connection.start.mockReset().mockRejectedValue(new Error("no server"));

    const { result } = renderHook(() => useGame());

    await waitFor(() => expect(result.current.error).toBe("no server"));
    expect(result.current.connected).toBe(false);
  });

  it("applies each server push event to state", async () => {
    const { result } = await mountConnected();

    const room: RoomView = { code: "ABCD", state: "Lobby", members: [] };
    const gameState = { phase: "MichiganRummyInProgress" } as GameStateView;
    const hand: SelfView = { seatIndex: 1, hand: [], legalPlays: [] };
    const openRooms: OpenRoomView[] = [{ code: "ABCD", playerCount: 1, capacity: 9 }];

    act(() => connection.handlers[ServerEvents.OpenRoomsUpdated](openRooms));
    act(() => connection.handlers[ServerEvents.LobbyUpdated](room));
    act(() => connection.handlers[ServerEvents.GameStateUpdated](gameState));
    act(() => connection.handlers[ServerEvents.HandUpdated](hand));

    expect(result.current.room).toEqual(room);
    expect(result.current.gameState).toEqual(gameState);
    expect(result.current.hand).toEqual(hand);
    expect(result.current.openRooms).toEqual(openRooms);
  });

  it("clears a stale room when open rooms arrive, so a lost seat drops back to the lobby", async () => {
    const { result } = await mountConnected();

    const room: RoomView = { code: "ABCD", state: "InProgress", members: [] };
    const gameState = { phase: "MichiganRummyInProgress" } as GameStateView;
    const hand: SelfView = { seatIndex: 1, hand: [], legalPlays: [] };

    act(() => connection.handlers[ServerEvents.LobbyUpdated](room));
    act(() => connection.handlers[ServerEvents.GameStateUpdated](gameState));
    act(() => connection.handlers[ServerEvents.HandUpdated](hand));
    expect(result.current.room).toEqual(room);

    const openRooms: OpenRoomView[] = [{ code: "WXYZ", playerCount: 1, capacity: 9 }];
    act(() => connection.handlers[ServerEvents.OpenRoomsUpdated](openRooms));

    expect(result.current.room).toBeNull();
    expect(result.current.gameState).toBeNull();
    expect(result.current.hand).toBeNull();
    expect(result.current.openRooms).toEqual(openRooms);
  });

  it("createRoom invokes the hub, stores and returns the room", async () => {
    const room: RoomView = { code: "WXYZ", state: "Lobby", members: [] };
    connection.invoke.mockReset().mockResolvedValue(room);
    const { result } = await mountConnected();

    let returned: RoomView | undefined;
    await act(async () => {
      returned = await result.current.createRoom("Jeff");
    });

    expect(connection.invoke).toHaveBeenCalledWith(HubMethods.CreateRoom, "Jeff");
    expect(returned).toEqual(room);
    expect(result.current.room).toEqual(room);
  });

  it("maps an invoke failure to an error and returns undefined", async () => {
    connection.invoke.mockReset().mockRejectedValue(new Error("illegal move"));
    const { result } = await mountConnected();

    let returned: RoomView | undefined;
    await act(async () => {
      returned = await result.current.createRoom("Jeff");
    });

    expect(returned).toBeUndefined();
    expect(result.current.error).toBe("illegal move");
  });

  it("joinRoom, setReady, startGame and playCard invoke their hub methods", async () => {
    const { result } = await mountConnected();
    const card: CardDto = { suit: "Clubs", rank: "Three" };

    await act(async () => {
      await result.current.joinRoom("ABCD", "Jeff");
    });
    await act(async () => {
      await result.current.setReady(true);
    });
    await act(async () => {
      await result.current.startGame();
    });
    await act(async () => {
      await result.current.playCard(card);
    });

    expect(connection.invoke).toHaveBeenCalledWith(HubMethods.JoinRoom, "ABCD", "Jeff");
    expect(connection.invoke).toHaveBeenCalledWith(HubMethods.SetReady, true);
    expect(connection.invoke).toHaveBeenCalledWith(HubMethods.StartGame);
    expect(connection.invoke).toHaveBeenCalledWith(HubMethods.PlayCard, card);
  });

  it("tracks the connection indicator across reconnect lifecycle events", async () => {
    const { result } = await mountConnected();

    act(() => connection.lifecycle.reconnecting());
    expect(result.current.connected).toBe(false);

    act(() => connection.lifecycle.reconnected());
    expect(result.current.connected).toBe(true);

    act(() => connection.lifecycle.closed());
    expect(result.current.connected).toBe(false);
  });

  it("restarts after an unexpected close instead of giving up", async () => {
    const { result } = await mountConnected();
    expect(connection.start).toHaveBeenCalledOnce();

    act(() => connection.lifecycle.closed());
    expect(result.current.connected).toBe(false);

    await waitFor(() => expect(connection.start).toHaveBeenCalledTimes(2), { timeout: 2000 });
    await waitFor(() => expect(result.current.connected).toBe(true));
  });

  it("stops the connection on unmount", async () => {
    const { unmount } = await mountConnected();

    unmount();

    expect(connection.stop).toHaveBeenCalledOnce();
  });
});
