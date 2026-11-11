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
  return {
    connection: {
      handlers,
      on: vi.fn((event: string, cb: (arg: unknown) => void) => {
        handlers[event] = cb;
      }),
      start: vi.fn(() => Promise.resolve()),
      stop: vi.fn(() => Promise.resolve()),
      invoke: vi.fn(() => Promise.resolve()),
    },
  };
});

vi.mock("./api/gameConnection", () => ({ createConnection: () => connection }));

import { useGame } from "./useGame";

beforeEach(() => {
  connection.start.mockReset().mockResolvedValue(undefined);
  connection.stop.mockReset().mockResolvedValue(undefined);
  connection.invoke.mockReset().mockResolvedValue(undefined);
  connection.on.mockClear();
  for (const key of Object.keys(connection.handlers)) delete connection.handlers[key];
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

    act(() => connection.handlers[ServerEvents.LobbyUpdated](room));
    act(() => connection.handlers[ServerEvents.GameStateUpdated](gameState));
    act(() => connection.handlers[ServerEvents.HandUpdated](hand));
    act(() => connection.handlers[ServerEvents.OpenRoomsUpdated](openRooms));

    expect(result.current.room).toEqual(room);
    expect(result.current.gameState).toEqual(gameState);
    expect(result.current.hand).toEqual(hand);
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

  it("stops the connection on unmount", async () => {
    const { unmount } = await mountConnected();

    unmount();

    expect(connection.stop).toHaveBeenCalledOnce();
  });
});
