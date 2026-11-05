// TypeScript mirror of the server's wire DTOs. Enums serialize as strings,
// and SignalR serializes property names as camelCase.

export type Suit = "Clubs" | "Diamonds" | "Hearts" | "Spades";

export type Rank =
  | "Two" | "Three" | "Four" | "Five" | "Six" | "Seven" | "Eight"
  | "Nine" | "Ten" | "Jack" | "Queen" | "King" | "Ace";

export interface CardDto {
  suit: Suit;
  rank: Rank;
}

export type GamePhase =
  | "WaitingForDeal"
  | "DealComplete"
  | "HeartsInProgress"
  | "PokerInProgress"
  | "MichiganRummyInProgress"
  | "GameComplete";

export type RoundPhase =
  | "AwaitingOpeningLead"
  | "RunInProgress"
  | "RunStopped"
  | "RoundComplete";

export type ChipPoolType =
  | "AceOfHearts" | "KingOfHearts" | "QueenOfHearts" | "JackOfHearts"
  | "TenOfHearts" | "KingQueenOfHearts" | "EightNineTen" | "Kitty" | "Pot";

export interface PoolView {
  type: ChipPoolType;
  chipCount: number;
}

// Public per-seat view: counts only, never the cards themselves.
export interface PlayerSeatView {
  seatIndex: number;
  name: string;
  chipCount: number;
  cardsRemaining: number;
}

// Public table state broadcast to everyone in the room.
export interface GameStateView {
  phase: GamePhase;
  roundPhase: RoundPhase | null;
  currentLeaderIndex: number | null;
  currentTurnSeatIndex: number | null;
  lastPlayedCard: CardDto | null;
  nextCard: CardDto | null;
  winnerIndex: number | null;
  lastRoundWinner: string | null;
  pools: PoolView[];
  seats: PlayerSeatView[];
}

// Private view sent only to the owning connection.
export interface SelfView {
  seatIndex: number | null;
  hand: CardDto[];
  legalPlays: CardDto[];
}

export type RoomState = "Lobby" | "InProgress" | "Completed";

export interface RoomMemberView {
  playerId: string;
  name: string;
  isReady: boolean;
}

export interface RoomView {
  code: string;
  state: RoomState;
  members: RoomMemberView[];
}

export interface OpenRoomView {
  code: string;
  playerCount: number;
  capacity: number;
}

// Names the server pushes to clients (must match server's ClientEvents).
export const ServerEvents = {
  LobbyUpdated: "LobbyUpdated",
  GameStateUpdated: "GameStateUpdated",
  HandUpdated: "HandUpdated",
  OpenRoomsUpdated: "OpenRoomsUpdated",
} as const;

// Hub methods the client invokes (must match GameHub method names).
export const HubMethods = {
  CreateRoom: "CreateRoom",
  JoinRoom: "JoinRoom",
  SetReady: "SetReady",
  StartGame: "StartGame",
  PlayCard: "PlayCard",
} as const;
