interface ConnectionStatusProps {
  connected: boolean;
  error: string | null;
}

export function ConnectionStatus({ connected, error }: ConnectionStatusProps) {
  return (
    <div className="conn-status">
      <span className={connected ? "conn-dot online" : "conn-dot"} />
      <span>{connected ? "connected" : "connecting…"}</span>
      {error && <span className="conn-error">{error}</span>}
    </div>
  );
}
