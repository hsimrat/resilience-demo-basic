# Resilience Demo (Basic)\n\nPublic basic version. Premium code is private.

# Resilience Demo (Basic) — HttpClientFactory, Named/Typed Clients, Polly

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web-5C2D91?logo=dotnet)](https://learn.microsoft.com/aspnet/core)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](/LICENSE)
[![Build](https://img.shields.io/badge/Build-local-blue)](#quick-start)

A minimal, public **teaching repo** that shows the *right way* to call external APIs in .NET:

- **`IHttpClientFactory`** with **Named** & **Typed** clients
- **Polly** for **retry**, **timeout**, and **circuit breaker**
- Optional **ASP.NET rate limiting** on the server
- A tiny **simulation endpoint** to reproduce flaky/time-out/throttled behavior

> 🔒 A separate **premium** codebase contains production extras (advanced policies, observability, fallbacks, hedging, OpenTelemetry, etc.). This repo is the simpler, public baseline.

---

## Contents

- [Quick Start](#quick-start)
- [Why not `new HttpClient()` per call?](#why-not-new-httpclient-per-call)
- [How it’s wired](#how-its-wired)
- [Run the demo](#run-the-demo)
- [Simulation IDs & behavior](#simulation-ids--behavior)
- [Menu mapping](#menu-mapping)
- [Notes & gotchas](#notes--gotchas)
- [FAQ](#faq)
- [License](#license)

---

## Quick Start

```bash
# 1) Clone
git clone https://github.com/<YOUR-USER>/resilience-demo-basic.git
cd resilience-demo-basic

# 2) Optional: create .gitignore if missing
dotnet new gitignore

# 3) Restore & run (API)
dotnet restore
dotnet build -c Release
dotnet run -c Release --project src/Your.ApiProject   # adjust path

# 4) Open Swagger
# https://localhost:5170/swagger or whatever your profile/port is

If you’re on Windows with self-signed dev certs, run dotnet dev-certs https --trust once.

Why not new HttpClient() per call?

Socket exhaustion: TCP sockets pile up in TIME_WAIT.

Stale DNS: long-lived handlers must rotate to pick up new IPs.

Overhead: repeated TLS handshakes and connection setup.

✅ Use IHttpClientFactory so a pooled SocketsHttpHandler handles connection reuse, DNS refresh (via handler lifetime), HTTP/2, decompression, proxies, etc.

How it’s wired

Caller
  │
  ▼
Typed Client (e.g., PaymentsClient : IPaymentsClient)
  │            ┌───────────────────────────────┐
  │  uses      │  HttpMessageHandler pipeline  │
  │            │  (built by AddHttpClient)     │
  │            ├── Correlation / logging       │
  │            ├── Polly: Retry / Timeout / CB │
  │            └── SocketsHttpHandler (pooled) │
  ▼
External Service (simulated via /sim/payments/{id})


Named client: per-service config (base URL, headers, handler lifetime).

Typed client: domain surface (CaptureAsync(id)) hides HTTP plumbing.

Run the demo

Start API (see Quick Start).

Open Swagger to try the simulation endpoint:

POST /sim/payments/{id}

Or run your console/menu client if included (maps to IDs below).

Simulation IDs & behavior

Adjust to match your code if you’ve changed the patterns.

| Pattern      | Behavior                                      |
| ------------ | --------------------------------------------- |
| `ok-*`       | Always 200 OK                                 |
| `retry-*`    | First **2** attempts = 500, then **200**      |
| `slow-*`     | Sleeps (e.g., 3000 ms) → triggers **timeout** |
| `boom-*`     | Always 500 → **circuit breaker** can open     |
| `throttle-*` | Returns 429 + `Retry-After` header            |

Menu mapping

If your console/menu maps options to IDs, a typical run looks like:

| Menu | Scenario                  | Example ID       | Expected                                  |
| ---- | ------------------------- | ---------------- | ----------------------------------------- |
| 1    | Baseline success          | `ok-123`         | Fast 200, warms connection pool           |
| 2    | Timeout                   | `slow-3000`      | Polly timeout, clean abort                |
| 3    | Retry (2x fail → succeed) | `retry-test-801` | 500 → 500 → 200 (needs **2** retries)     |
| 4    | Circuit breaker           | `boom-500`       | Open after N fails, half-open, then close |
| 5    | Rate limit with backoff   | `throttle-xyz`   | 429 seen; backoff & retry respected       |


Notes & gotchas

Retries: WaitAndRetryAsync count is in addition to the initial attempt.
“Fail twice then succeed” needs 2 retries → 3 total attempts.

Timeouts: Prefer Polly Timeout over HttpClient.Timeout for better composition & telemetry.

Circuit breaker: Pick sensible exceptionsAllowedBeforeBreaking and durationOfBreak.

Rate limiting: Client honors 429 + Retry-After; server can use UseRateLimiter() for fairness.

DNS freshness: Use SetHandlerLifetime(TimeSpan.FromMinutes(2)) (or your ops standard).

FAQ

Q: Where’s the advanced stuff (fallback cache, hedging, OTEL, idempotency keys)?
A: Kept in a separate private repo as part of a premium codebase.

Q: Can I use this in prod as-is?
A: Treat this as teaching code. The patterns are production-grade; config and guardrails are simplified.


