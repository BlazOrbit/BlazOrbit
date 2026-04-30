<!--
Thanks for the contribution! See CONTRIBUTING.md for the full workflow.
Pick the relevant sections; delete the ones that do not apply.
-->

## Summary

<!-- One or two sentences describing the change and why. -->

## Linked tasks / issues

<!-- e.g. Fixes #42, Closes TASKS.md L10N-07 -->

## Type of change

- [ ] feat — new component, parameter, or public behavior
- [ ] fix — bug fix
- [ ] refactor — no observable behavior change
- [ ] perf — performance-only change
- [ ] docs — documentation only
- [ ] test — tests only
- [ ] chore — deps, scripts, infra
- [ ] **breaking** — removes or changes existing public API (also tick one of the above)

## Checklist

- [ ] `dotnet build CdCSharp.BlazorBit.slnx -c Release` is green
- [ ] `dotnet test` is green
- [ ] New / modified public types & members are documented (XML doc) and added to `PublicAPI.Unshipped.txt`
- [ ] Tests added or updated for new behavior (per-component layout in `AGENTS.md` §Testing)
- [ ] `CHANGELOG.md` entry added under `[Unreleased]`
- [ ] If this closes a tracked task, `TASKS.md` is updated with `Estado: ✅ Resuelto (commit <hash>)`
- [ ] If this is a breaking change, the description below explains the migration path

## Notes for reviewer

<!-- Anything that helps the reviewer: tricky areas, follow-ups planned, deliberate
deviations from AGENTS.md (cite the §Exceptions and trade-offs row), etc. -->
