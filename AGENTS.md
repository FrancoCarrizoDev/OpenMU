# Project agent instructions

This checkout is the server half of the private MuOnline Season 1 project.

## Git policy for this fork

- The project branch is `season1-classic`.
- `origin` is the writable `FrancoCarrizoDev/OpenMU` fork.
- `upstream` is the read-only `MUnique/OpenMU` source; its official branch is `master` and its push URL must remain disabled.
- Push project commits only to `origin`.
- To receive official changes, fetch `upstream`, merge `upstream/master` into `season1-classic`, resolve conflicts deliberately, validate, and push to `origin/season1-classic`.
- For ordinary features, branch from the current `season1-classic` and merge the focused feature branch back after validation.
- Before Git mutations, inspect the working tree, current branch, remotes, and recent history.
- Never blindly merge or force-clean an old Orca branch. Its meaningful work may already be integrated while its worktree still contains duplicate/generated changes. Read the Orca comment and compare against `season1-classic` first.
- Do not remove an Orca worktree or discard dirty files without explicit user authorization.

## Change and validation policy

- Keep commits focused and include the source, configuration, migrations, documentation, and tests required by the change.
- Do not commit build output, local tool caches, dependency directories, or generator noise. Generated files already tracked by the repository may be committed only after their diff is reviewed and they are required by the source definition change.
- Preserve both the Season 1 behavior and relevant upstream fixes when resolving conflicts; do not select one entire side automatically.
- Run `git diff --check` before committing.
- The baseline server validation is:

  ```powershell
  dotnet build src/MUnique.OpenMU.sln -c Release --no-restore --nologo
  ```

- Run focused tests for every affected subsystem. Initialization, migrations, packet definitions, combat, persistence, and gameplay changes require their corresponding regression tests.

As of 2026-09-13, the published and validated baseline was commit `4675c75b7`. Treat this as historical context and verify the current branch tip before acting.
