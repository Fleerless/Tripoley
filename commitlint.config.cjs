// Enforces Conventional Commits (feat:, fix:, chore:, ci:, docs:, test:, refactor:…).
// This format is what release-please reads to decide the next version, so the
// commitlint CI check keeps the automated versioning reliable.
module.exports = {
  extends: ["@commitlint/config-conventional"],
};
