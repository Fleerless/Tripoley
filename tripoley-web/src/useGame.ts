import { useCallback, useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { createConnection } from "./api/gameConnection";
import {
  HubMethods,
  ServerEvents,
  type CardDto,
  type GameStateView,
  type OpenRoomView,
  type RoomView,
  type SelfView,
} from "./api/contracts";

export interface Game {
  connected: boolean;
  room: RoomView | null;
  gameState: GameStateView | null;
  hand: SelfView | null;
  openRooms: OpenRoomView[];
  error: string | null;
  createRoom: (playerName: string) => Promise<RoomView | undefined>;
  joinRoom: (code: string, playerName: string) => Promise<RoomView | undefined>;
  setReady: (isReady: boolean) => Promise<void>;
  startGame: () => Promise<void>;
  playCard: (card: CardDto) => Promise<void>;
}

export function useGame(): Game {
  const connRef = useRef<signalR.HubConnection | null>(null);
  const [connected, setConnected] = useState(false);
  const [room, setRoom] = useState<RoomView | null>(null);
  const [gameState, setGameState] = useState<GameStateView | null>(null);
  const [hand, setHand] = useState<SelfView | null>(null);
  const [openRooms, setOpenRooms] = useState<OpenRoomView[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const conn = createConnection();
    connRef.current = conn;

    conn.on(ServerEvents.LobbyUpdated, (r: RoomView) => setRoom(r));
    conn.on(ServerEvents.GameStateUpdated, (s: GameStateView) => setGameState(s));
    conn.on(ServerEvents.HandUpdated, (h: SelfView) => setHand(h));
    conn.on(ServerEvents.OpenRoomsUpdated, (rooms: OpenRoomView[]) => setOpenRooms(rooms));

    conn
      .start()
      .then(() => setConnected(true))
      .catch((e) => setError(e instanceof Error ? e.message : String(e)));

    return () => {
      setConnected(false);
      connRef.current = null;
      void conn.stop();
    };
  }, []);

  const call = useCallback(
    async <T>(action: (c: signalR.HubConnection) => Promise<T>): Promise<T | undefined> => {
      const conn = connRef.current;
      if (!conn) return undefined;
      try {
        setError(null);
        return await action(conn);
      } catch (e) {
        setError(e instanceof Error ? e.message : String(e));
        return undefined;
      }
    },
    [],
  );

  const createRoom = useCallback(
    (playerName: string) =>
      call(async (c) => {
        const r = await c.invoke<RoomView>(HubMethods.CreateRoom, playerName);
        setRoom(r);
        return r;
      }),
    [call],
  );

  const joinRoom = useCallback(
    (code: string, playerName: string) =>
      call(async (c) => {
        const r = await c.invoke<RoomView>(HubMethods.JoinRoom, code, playerName);
        setRoom(r);
        return r;
      }),
    [call],
  );

  const setReady = useCallback(
    async (isReady: boolean) => {
      await call((c) => c.invoke(HubMethods.SetReady, isReady));
    },
    [call],
  );

  const startGame = useCallback(async () => {
    await call((c) => c.invoke(HubMethods.StartGame));
  }, [call]);

  const playCard = useCallback(
    async (card: CardDto) => {
      await call((c) => c.invoke(HubMethods.PlayCard, card));
    },
    [call],
  );

  return { connected, room, gameState, hand, openRooms, error, createRoom, joinRoom, setReady, startGame, playCard };
}
