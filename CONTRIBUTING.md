# Contributing to BlazOrbit

Thanks for considering a contribution. This project is a small library with a strict architectural contract — please skim this document before opening a PR so the review goes quickly.

For the day-to-day rules of the codebase (component architecture, CSS conventions, async patterns, public API tracking) see [`AGENTS.md`](AGENTS.md). For the release / branching strategy see [`VERSIONING.md`](VERSIONING.md).

## Branching

`master` is protected. Work happens on short-lived branches that target `master` via PR.

| Prefix | When to use | Example |
|---|---|---|
| `feature/` | New component, new behavior, new public API | `feature/datagrid-virtualization` |
| `fix/` | Bug fix, behavior correction | `fix/dropdown-double-dispose` |
| `chore/` | Dep bumps, scripts, infra, docs-only changes | `chore/bump-aspnetcore-10.0.7` |
| `docs/` | Documentation-only changes (README, AGENTS.md, this file) | `docs/contributing-guide` |
| `refactor/` | Internal restructuring with no observable behavior change | `refactor/component-pipeline` |
| `test/` | Test-only additions | `test/dropdown-disposal` |

Aim to keep a branch alive for **1–3 days**. Long-lived branches drift from `master` and create painful rebases.

The `scripts/dev-tools.ps1` helper script wraps the common operations (`feature`, `fix`, `commit`, `squash`, `ready`, `pr`, `cleanup`). See `AGENTS.md` §Common commands for the catalogue.

## Commits

We use [Conventional Commits](https://www.conventionalcommits.org/) so the changelog and the version bump can be inferred from the log:

```
<type>(<scope>): <description>

<optional body>

<optional footer, e.g. Fixes #123>
```

Allowed types: `feat`, `fix`, `docs`, `test`, `refactor`, `chore`, `perf`, `breaking`.

`<scope>` is the task ID from `TASKS.md` (e.g. `l10n-07`, `claude-04`) when the commit closes a tracked task; otherwise a short component or area name (e.g. `dropdown`, `theme`).

## Pull requests

**Strategy: each PR = one squash commit on `master`.** Keep your local history readable while you work, then squash before opening the PR. The dev-tools helper has a `ready` command that runs `squash + rebase + push` in one step.

Before requesting review, run:

```bash
dotnet build BlazOrbit.slnx -c Release
dotnet test
```

Both must be green. The PR is also blocked on:

- **CI gates** (`release-gate.yml`): build, tests, public API diff, blocking-issue scan.
- **At least one approving review** from a maintainer.
- **`PublicAPI.Unshipped.txt` updated** if you changed the public surface (the analyzer's code-fix does this for you — see `AGENTS.md` §Public API tracking).
- **`TASKS.md` updated** when you close a tracked task (mark `Estado: ✅ Resuelto (commit <short-hash>)`; see `MEMORY.md` notes if you have access to the maintainer's auto-memory).

## Reporting issues

The repo ships GitHub issue templates under `.github/ISSUE_TEMPLATE/`:

- **Bug report** for behavior that diverges from documented contract.
- **Feature request** for new components, parameters, or behaviors.

Search closed issues first — most regressions have a recurring cause. If you cannot find a duplicate, fill in the template completely; "doesn't work" reports get closed with a request for a repro.

## Local environment

The build pipeline is non-standard — read `AGENTS.md` §Build pipeline before touching anything under `src/BlazOrbit.BuildTools` or any `.targets` file. In particular:

- `CssBundle/*.css`, `wwwroot/css/*`, `wwwroot/js/*`, `package.json`, `tsconfig.json`, `vite.config*.js`, `.npmrc` are **all generated**. Do not edit them on disk; edit the corresponding generator or template under `src/BlazOrbit.BuildTools/`.
- `dotnet clean` deletes the generated assets — they are recreated on the next build.

## Code of conduct

Be civil. Disagreements about architecture are normal; personal attacks are not. Maintainers reserve the right to close threads that derail.
