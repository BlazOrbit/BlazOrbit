# Versioning Strategy

BlazOrbit follows **Semantic Versioning (SemVer)** with a simplified Git Flow built around two long-lived branches: `master` and `develop`.

---

## Semantic Versioning

Versions follow the format `MAJOR.MINOR.PATCH`:

| Segment | Change Type | Example |
|---------|-------------|---------|
| **MAJOR (X)** | Breaking change that requires consumer action | `1.0.0` → `2.0.0` |
| **MINOR (Y)** | New functionality, backward-compatible | `1.0.0` → `1.1.0` |
| **PATCH (Z)** | Bug fix or small improvement, backward-compatible | `1.0.0` → `1.0.1` |

Additional labels:

| Pattern | Meaning | Example |
|---------|---------|---------|
| `X.Y.Z-preview.N` | Active development build from `develop` | `1.0.0-preview.42` |
| `X.Y.Z-rc.N` | Release candidate, feature-complete | `1.0.0-rc.2` |

---

## Branch Structure

```
master   ●────●────●────●────●────●────►  (tagged stable releases)
         ↑    ↑    ↑    ↑    ↑    ↑
develop  ●────┬────┬────┬────┬────┬────►  (continuous integration)
              │    │    │    │    │
              ▼    ▼    ▼    ▼    ▼
             PR   PR   PR   PR   PR    (short-lived, squash merged)
```

| Branch | Purpose | Protection | Merge Strategy |
|--------|---------|------------|----------------|
| `master` | Stable releases | Required PR, 1 approval, green CI | Squash merge via release branch |
| `develop` | Continuous integration | Required PR, green CI, squash merge | Squash merge |
| `feature/*` | Isolated changes | Not protected | Squash to `develop` |
| `fix/*` | Bug fixes | Not protected | Squash to `develop` |
| `hotfix/*` | Urgent production fixes | Not protected | PR directly to `master` |

---

## Development Flow

### 1. Start a Feature

```powershell
./scripts/dev-tools.ps1 feat my-feature
# Work, commit frequently using Conventional Commits
./scripts/dev-tools.ps1 ready  # Squash + rebase onto develop + push
# Open Pull Request on GitHub targeting develop
```

### 2. Merge to `develop`

- Must pass the **Preview Gate** workflow (`preview-gate.yml`):
  - Build (Debug & Release)
  - Run all tests
  - Public API shipped integrity check
- Merge with **Squash and merge**.
- After merge, the **Preview Publish** workflow (`preview-publish.yml`) automatically publishes a `preview` NuGet version.

---

## Commit Convention

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]

Fixes #<issue-number>
```

Types:
- `feat` — New feature
- `fix` — Bug fix
- `docs` — Documentation
- `test` — Tests
- `refactor` — Code refactoring
- `chore` — Maintenance
- `perf` — Performance improvement
- `breaking` — Breaking change

---

## Severity Labels

Issues are classified by severity. Some labels block releases:

| Label | Meaning | Blocks Release |
|-------|---------|----------------|
| `severity/blocker` | Must be fixed before any release | Yes |
| `severity/critical` | Security or data-loss risk | Yes |
| `severity/major` | Significant bug or regression | No |
| `severity/minor` | Small bug or inconvenience | No |
| `severity/polish` | Quality-of-life improvement | No |

---

---

## Support Policy

- **Latest stable** (`master`) receives bug fixes and security patches.

---

## Changelog

The project maintains a `CHANGELOG.md` file that lists all notable changes per version. The changelog is updated as part of the release preparation. You can view pending changes with:

```powershell
./scripts/admin-tools.ps1 changelog
```

---

## CI/CD Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `preview-gate.yml` | PR to `develop` | Quality gates (build, test, public API shipped integrity) |
| `preview-publish.yml` | Push to `develop` | Auto-publish preview NuGet packages |
| `release-gate.yml` | PR to `master` | Release gates (blocking issues, vulnerabilities, public API shipped integrity) |
| `release-publish.yml` | Tag `v*` | Publish stable release to NuGet |
| `setup-repository.yml` | Manual | Configure labels and branch protection |

---
