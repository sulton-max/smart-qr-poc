# Smart QR — Research

Technical research dumps feeding architecture + iteration decisions.

- `design-research/design-research.md` — product UI design source (light + dark, Tailwind v4 `@theme` + `@wow-two-beta/ui` tokens).
- `form-design-research/` — form-design exploration (`ideas.excalidraw`).
- `data-seam/` — Dapper-read / EF-write POC (runnable). Measures the `Attach` snapshot trap (load→attach→mutate) + the jsonb-converter gap. Feeds the backend-SDK read-seam chat and validation § *Phases*.
- `error-ordering/error-ordering.md` — cited answer to the open error-ordering question (`v0.9/handoff.md`): resolve+authorize (`404`/`403`) vs payload validation (`400`/`422`). RFC 9110 / AIP-211 / CWE-203-204 / OWASP + Rails·DRF·Laravel·ASP.NET Core source-verified sequencing, rigour-tagged `[S]`/`[F]`/`[G]`/`[O]`.
