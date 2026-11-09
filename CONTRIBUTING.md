# Contributing

## Branches

- `develop` — default branch; all work lands here.
- `production` — deploys to AWS on push (via PR from `develop`).

Work on a feature branch, open a PR into `develop`, then promote `develop` → `production` to ship.

## Commit messages

[Conventional Commits](https://www.conventionalcommits.org), enforced in CI by commitlint:

    <type>(optional scope): <summary>

Common types: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `ci`, `build`, `perf`.

- `feat:` → minor version bump
- `fix:` → patch version bump
- `feat!:` or a `BREAKING CHANGE:` footer → major bump

## Releases

Versioning is automated by [release-please](https://github.com/googleapis/release-please).
Each push to `develop` keeps a **Release PR** open that bumps the version in
`.release-please-manifest.json` and updates `CHANGELOG.md`. Merge that PR to tag
`vX.Y.Z` and cut a GitHub Release, then promote `develop` → `production` to deploy it.

The live version is served at `GET /version` on the API and shown in the client status bar.
