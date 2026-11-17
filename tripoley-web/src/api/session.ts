export const HUB_URL =
  (import.meta.env.VITE_HUB_URL as string | undefined) ??
  "http://localhost:5106/hubs/gamehub";

const AUTH_URL = HUB_URL.replace(/\/hubs\/.*$/, "") + "/auth/session";
const TOKEN_KEY = "tripoley.session.token";

export async function getToken(): Promise<string> {
  const stored = localStorage.getItem(TOKEN_KEY);
  if (stored && !isExpired(stored)) return stored;

  const token = await mint();
  localStorage.setItem(TOKEN_KEY, token);
  return token;
}

async function mint(): Promise<string> {
  const res = await fetch(AUTH_URL, { method: "POST" });
  if (!res.ok) throw new Error(`Session mint failed: ${res.status}`);
  const { token } = (await res.json()) as { token: string };
  return token;
}

function isExpired(token: string): boolean {
  try {
    const [, payload] = token.split(".");
    const claims = JSON.parse(
      atob(payload.replace(/-/g, "+").replace(/_/g, "/")),
    ) as { exp?: number };
    if (!claims.exp) return true;
    return Date.now() >= claims.exp * 1000 - 30_000;
  } catch {
    return true;
  }
}
