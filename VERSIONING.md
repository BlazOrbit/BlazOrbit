# VERSIONING.md — BlazOrbit Versioning Strategy

> Simplified workflow with linear history. No "metro lines".

---

## 1. Philosophy

- **One integration branch**: `develop`
- **Linear history**: fast-forward or rebase only
- **Short PRs**: maximum 1-3 days lifetime
- **One task = one commit** (squash WIPs before push)
- **Linked issues**: each commit references an issue

---

## 2. Branch Structure

```
master   ●────●────●────●────●────●────►  (tagged releases)
         ↑    ↑    ↑    ↑    ↑    ↑
develop  ●────┬────┬────┬────┬────┬────►  (continuous integration)
              │    │    │    │    │
              ▼    ▼    ▼    ▼    ▼
             PR   PR   PR   PR   PR    (short, squash merge)
```

| Branch | Purpose | Protection | Merge |
|--------|---------|------------|-------|
| `master` | Stable releases | ✅ Required PR, 1 approval, green CI | Fast-forward only |
| `develop` | Continuous integration | ✅ Required PR, green CI, squash merge | Squash merge |
| `feature/*` | Isolated changes | ❌ Not protected | Squash to develop |

---

## 3. Versioning (SemVer)

| Pattern | Meaning | Example |
|---------|---------|---------|
| `X.Y.Z-preview.N` | Active development | `1.0.0-preview.42` |
| `X.Y.Z-rc.N` | Release candidate | `1.0.0-rc.2` |
| `X.Y.Z` | Stable release | `1.0.0` |

### Bump Rules

| Change | Version | Example |
|--------|---------|---------|
| Breaking change | Major | `1.0.0` → `2.0.0` |
| New feature | Minor | `1.0.0` → `1.1.0` |
| Bugfix | Patch | `1.0.0` → `1.0.1` |

---

## 4. Development Flow

### 4.1 Start Feature

```powershell
./scripts/dev-tools.ps1 feature my-feature
# Work, commit frequently
./scripts/dev-tools.ps1 ready  # Squash + rebase + push
# Create PR on GitHub
```

### 4.2 PR to develop

- Must pass: **Preview Gate**
  - Build and Test
  - Code Coverage
  - Check Public API
- Merge: **Squash and merge**
- After merge: Preview auto-published

### 4.3 PR to master (Release)

- Must pass: **Release Gate**
  - Build Check
  - Check Blocking Issues (no `severity/blocker` or `severity/critical`)
  - Check Public API
- Merge: **Squash and merge**
- After merge: Create tag to trigger release

### 4.4 Create Release

```powershell
./scripts/admin-tools.ps1 release 1.0.0
# Creates tag v1.0.0 → triggers Release Publish
```

---

## 5. Commit Convention

```
<type>(<scope>): <description>

[optional body]

Fixes #<issue-number>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `test`: Tests
- `refactor`: Code refactoring
- `chore`: Maintenance
- `breaking`: Breaking change

---

## 6. Scripts

### Developer Scripts (`./scripts/dev-tools.ps1`)

| Command | Description |
|---------|-------------|
| `sync` | Update develop from origin |
| `feature <name>` | Create feature branch from develop |
| `fix <name>` | Create fix branch from develop |
| `commit "message"` | Commit with conventional commits |
| `squash` | Squash all commits into one |
| `ready` | Prepare PR: squash + rebase + push |
| `fix-conflict` | After resolving conflicts |
| `push` | Safe push with force-with-lease |
| `pr "Title" "Desc"` | Open PR page on GitHub |
| `cleanup` | Clean merged branches |
| `status` | Show repository status |

### Admin Scripts (`./scripts/admin-tools.ps1`)

| Command | Description |
|---------|-------------|
| `status` | Show branches status |
| `check-pr <branch>` | Verify PR ready to merge |
| `rc <version>` | Create release candidate |
| `release <version>` | Publish stable release |
| `hotfix <version>` | Create hotfix branch |
| `changelog` | Show pending changelog |
| `cleanup` | Clean merged branches |

---

## 7. Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `preview-gate.yml` | PR to `develop` | Quality gates (build, test, coverage, public api) |
| `preview-publish.yml` | Push to `develop` | Auto-publish preview version |
| `release-gate.yml` | PR to `master` | Release gates (blocking issues, public api) |
| `release-publish.yml` | Tag `v*` | Publish stable release |
| `setup-repository.yml` | Manual | Configure labels and branch protection |

---

## 8. Severity Labels

Used to mark issues and block releases:

| Label | Meaning | Blocks Release |
|-------|---------|----------------|
| `severity/blocker` | Blocks release | ✅ Yes |
| `severity/critical` | Critical issue | ✅ Yes |
| `severity/major` | Major issue | ❌ No |
| `severity/minor` | Minor issue | ❌ No |
| `severity/polish` | Polish/QOL | ❌ No |

---

## 9. Hotfixes

For urgent fixes on production:

```powershell
./scripts/admin-tools.ps1 hotfix 1.0.1
# Fix, commit, push
# Create PR directly to master
# Fast-track through release gate
./scripts/admin-tools.ps1 release 1.0.1
```

---

## Summary

1. **Work on feature branches** from `develop`
2. **Squash to 1 commit** before PR
3. **PR to develop** passes Preview Gate
4. **PR to master** passes Release Gate (no blockers)
5. **Tag** triggers stable release
6. **Linear history** maintained throughout
