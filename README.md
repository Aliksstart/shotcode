![ShotCode](src/WinForms/Resources/shx.jpg)

# ShotCode

**ShotCode** is a local TOTP vault and code generator for Windows.
No cloud, no accounts, no background activity — the vault file stays on your machine and codes are computed offline.

---

## Features

- [x] Encrypted local vault: **Argon2id** (RFC 9106) key derivation, **AES-256-GCM** encryption
- [x] Offline TOTP generation (RFC 6238): SHA-1 / SHA-256 / SHA-512, 6–8 digits, 15–120 s period
- [x] Base32 secret input (RFC 4648) with strict validation
- [x] Service list with live codes and countdown
- [x] Copy code to clipboard (double click)
- [x] Auto-lock on inactivity, with key and secret wiping
- [x] Atomic vault writes — a power loss can't leave the file half-written
- [x] Documented binary storage format (see `FILE_FORMAT.md`)
- [x] Unsupported or corrupted records are skipped instead of breaking the whole vault
- [ ] Delete and edit entries
- [ ] Import from `otpauth://` URIs and Google Authenticator migration QR
- [ ] Automatic clipboard clearing
- [ ] Full-screen code view (for typing on another device)
- [ ] System tray mode
- [ ] HOTP (RFC 4226)
- [ ] TPM-backed vault (already reserved in the format)

---

## Requirements

Windows, .NET 8. Build: `dotnet build -c Release`

---

## Tests

The cryptographic core is covered by official test vectors:

- **RFC 6238** — TOTP, all three algorithms, including post-2038 timestamps
- **RFC 9106** — Argon2id
- **RFC 4648** — Base32, including negative cases (bad padding, non-alphabet characters)
- Vault round-trip, wrong-password behaviour, auto-lock, thread safety

---

## Threat model

**Protects against:** loss or theft of the vault file, leaked backups, offline attacks on encrypted secrets.

**Does not protect against:** a compromised OS, keyloggers, or memory scraping while the vault is unlocked.

The machine is assumed to be trusted at the moment secrets are accessed.

## Non-goals

Cloud sync, mobile apps, password management, custom cryptographic primitives, enterprise compliance features.

---

## Status

**v0.5** — working version for personal use. The storage format may change before 1.0.
The project has not been independently security-audited. Use at your own risk.
