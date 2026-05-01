# Security Policy

## Supported Versions

Security fixes are published for the latest minor of each supported major:

| Version | Supported |
|---------|-----------|
| 1.x     | ✅        |
| 0.x     | ❌        |

## Reporting a Vulnerability

Please report security issues privately — do **not** open a public GitHub issue.

- **Email**: `dev@blazorbit.com`
- **GitHub**: use the [Security Advisories](https://github.com/BlazOrbit/BlazOrbit/security/advisories/new) form to open a private report. Preferred channel when possible, since it keeps the disclosure conversation attached to the repo.

Include in your report:

1. Affected package and version (`BlazOrbit.X` + `1.y.z-preview.n`).
2. Reproducer — ideally a minimal project or the smallest snippet that triggers the issue.
3. Impact — what an attacker could achieve (data exfiltration, XSS, DoS, …).
4. Whether the issue requires user interaction or a specific host (Server vs. WASM) to manifest.

## Response Timeline

- **Acknowledgement**: within 3 business days.
- **Triage** (severity assessment, reproduction): within 7 business days.
- **Fix**: depends on severity. Critical issues target a patch release within 14 days of reproduction; lower-severity items are scheduled against the next minor.
- **Disclosure**: coordinated. The reporter is credited in the release notes unless anonymity is requested.

## Scope

In-scope vulnerabilities:

- Input sanitization bugs in components that render user-supplied content (XSS, HTML/CSS injection).
- JS interop surfaces that allow script injection into the host page (`ThemeInterop` localStorage keys, `BOBSvgIcon` markup paths, etc.).
- Cross-origin resource issues in the shipped static web assets.

Out of scope:

- Vulnerabilities that require the library author's local dev environment (dev-time `.targets` / BuildTools). These are development-environment issues, not library-ship issues.
- Issues in third-party dependencies (`Microsoft.AspNetCore.*`, FluentValidation, etc.) — report upstream.
- DoS via pathological component usage in consumer apps (e.g. rendering millions of rows without virtualization).

Thanks for helping keep BlazOrbit and its consumers safe.
