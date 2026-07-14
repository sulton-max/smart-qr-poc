# Smart QR — Version Track

Per-version iteration docs (scope + log). Status table + lifecycle in `../planning.md` § Versions.

Each version under `v{X.Y}/v{X.Y}.md` — `v0.1` (foundation) · `v0.2` (migration layer) · `v0.3` (accounts) · `v0.4` (SDK adoption) · `v0.5` (code styling) · `v0.6` · `v0.7` (static content + export) · `v0.9` (interactive landing hero — **experiment**, renumber later).

## Track conventions

> Govern **both** tracks — version (`v{X.Y}`) and polish (`p{X.Y}` under `polish-track/`).

- **Hierarchy:** track → **iteration** (a heading, bare name) → **task** (a `- [ ]` one-liner — the *what*) → **step** (a nested `- [ ]` — dev-time detail, the *how*).
- **Steps are scaffolding** — added while building a task to hold detailed context; **not** the durable record.
- **Never advance or close** an iteration without the developer's explicit go; verify completion with the developer.
- **Cleanup (on iteration done):** drop the **steps**, keep the **tasks** (abstract one-liners). Git history holds the step-level detail — the plan stays scannable. Don't drop tasks.
