/// Mirrors the Users service contract (POST /api/auth/register, /api/auth/login,
/// GET /api/users/me). Field names match the C# records (camelCased on the wire).

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string; // ISO-8601, from the server's DateTimeOffset
}

export interface User {
  id: string;
  email: string;
  displayName: string;
  createdAt: string;
}
