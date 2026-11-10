// Enforces Conventional Commits (feat:, fix:, chore:, ci:, docs:, test:, refactor:…).
// This format is what release-please reads to decide the next version, so the
// commitlint CI check keeps the automated versioning reliable.
module.exports = {
  extends: ["@commitlint/config-conventional"],
  rules: {
    // Bodies/footers here carry prose and URLs; don't hard-wrap them at 100.
    "body-max-line-length": [0, "always", 100],
    "footer-max-line-length": [0, "always", 100],
  },
};
