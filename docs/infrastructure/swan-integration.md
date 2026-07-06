# Swan.io Integration

## Endpoints

| Purpose | URL |
|---|---|
| OAuth2 token (client credentials) | `https://oauth.swan.io/oauth2/token` |
| Partner GraphQL API (sandbox) | `https://api.swan.io/sandbox-partner/graphql` |
| Admin/testing GraphQL API (sandbox) | `https://api.swan.io/sandbox-partner-admin/graphql` — separate schema, under investigation; unconfirmed whether it exposes sandbox-only KYC bypass mutations (e.g. `createSandboxUser` / `endorseSandboxUser`). Confirm by introspection before relying on it. |

Auth is OAuth2 client-credentials against the token endpoint above using `SWAN_CLIENT_ID`/`SWAN_CLIENT_SECRET`; the resulting project access token (1h TTL) is sent as `Authorization: Bearer <token>` on GraphQL calls.

## Schema introspection (StrawberryShake)

Local schema files live under `NexusBank.Infrastructure/External/Swan/`. `schema.graphql` is a large generated introspection dump (several hundred KB) — it's committed so builds/CI can generate the typed client offline, but it should never be hand-edited; always regenerate it via the command below when Swan's schema changes. `schema.extensions.graphql` and `.graphqlrc.json` are the small hand-authored files in the same folders and are safe to edit directly.

To (re)download a schema:

`dotnet-tools.json` is already committed with `strawberryshake.tools` in it, so on an existing checkout just run `dotnet tool restore` to install it — do **not** re-run the two commands below, since `--force` would overwrite the committed manifest. They're kept here only for bootstrapping a repo that has no manifest yet:

```bash
dotnet new tool-manifest --force   # only if no manifest exists yet
dotnet tool install StrawberryShake.Tools --local

dotnet graphql init <graphql-endpoint-url> -n <ClientName> \
  -p NexusBank.Infrastructure/External/Swan[/<subfolder>] \
  --tokenEndpoint https://oauth.swan.io/oauth2/token \
  --clientId $SWAN_CLIENT_ID \
  --clientSecret $SWAN_CLIENT_SECRET
```

Commands run so far:
- **Partner API** → `NexusBank.Infrastructure/External/Swan/` (client name `Swan`):
  ```bash
  dotnet graphql init https://api.swan.io/sandbox-partner/graphql -n Swan \
    -p NexusBank.Infrastructure/External/Swan \
    --tokenEndpoint https://oauth.swan.io/oauth2/token \
    --clientId $SWAN_CLIENT_ID --clientSecret $SWAN_CLIENT_SECRET
  ```
- **Admin/testing API** → `NexusBank.Infrastructure/External/Swan/Admin/` (client name `SwanAdmin`), pending confirmation:
  ```bash
  dotnet graphql init https://api.swan.io/sandbox-partner-admin/graphql -n SwanAdmin \
    -p NexusBank.Infrastructure/External/Swan/Admin \
    --tokenEndpoint https://oauth.swan.io/oauth2/token \
    --clientId $SWAN_CLIENT_ID --clientSecret $SWAN_CLIENT_SECRET
  ```

`-x|--headers` can be used instead of `--tokenEndpoint`/`--clientId`/`--clientSecret` to pass a pre-fetched bearer token manually (`-x "Authorization=Bearer <token>"`), but the CLI's built-in OAuth flags above are simpler since they fetch the token for you.

## Confirmed schema facts (partner API)

- Individual onboarding is created via `createIndividualAccountHolderOnboarding` (project-token, authenticated) — not `createPublicIndividualAccountHolderOnboarding` (that variant is for unauthenticated public signup forms).
- Finalization status is read via `accountHolderOnboarding(id)` → `statusInfo.status` (`Valid` / `Invalid` / `Finalized`) and `account { id }` once finalized.
- There is **no mutation in the partner schema to force onboarding/KYC approval** — `updateSupportingDocument` only edits document type/purpose metadata, it has no `status` field. KYC approval in sandbox is a manual action via the Dashboard Event Simulator, mirroring the fact that Swan (a licensed EMI) — not the partner — is legally responsible for identity verification, in sandbox and prod alike.
- Valid webhook event type strings are queryable live via `{ webhookEventTypes }` rather than a fixed schema enum; `Onboarding.Updated` is what we key off of for finalization (its `resourceId` is the onboarding id, which matches `User.SwanOnboardingId` directly).
