#!/usr/bin/env bash
# Enable strict mode when running inside GitHub Actions runner
if [ "${GITHUB_ACTIONS:-false}" = "true" ]; then
    set -euo pipefail
fi

Major="${1:-}"
if [ -z "$Major" ]; then
    echo "::error::No major version argument supplied. Usage: $0 <major> (e.g. v3)" >&2
    exit 1
fi
echo "Looking for tags that start with: $Major"

# Fetch tags
git fetch --tags --quiet

# Collect matching tags (newest first by creatordate)
mapfile -t MATCHING < <(
    git for-each-ref --sort=-creatordate --format='%(refname:short)' refs/tags \
    | grep -E "^${Major}(\.|$)" || true
)

if [ ${#MATCHING[@]} -eq 0 ]; then
    echo "::error::No tag found starting with '${Major}'. Showing latest tags:"
    git for-each-ref --sort=-creatordate --format='%(refname:short)' refs/tags | head -n 10
    exit 1
fi

echo "Showing up to 10 of top ${#MATCHING[@]} matching tags (newest first):"
for tag in "${MATCHING[@]:0:10}"; do
    echo " - $tag"
done

Tag="${MATCHING[0]}"
Version="${Tag#v}"

echo "Selected tag: $Tag -> version: $Version"

# If running in GitHub Actions, write to the runner files
if [ -n "${GITHUB_OUTPUT-}" ]; then
    printf 'TAG_NAME=%s\n' "$Tag" >> "$GITHUB_OUTPUT"
    printf 'VERSION=%s\n' "$Version" >> "$GITHUB_OUTPUT"
fi

if [ -n "${GITHUB_ENV-}" ]; then
    printf 'TAG_NAME=%s\n' "$Tag" >> "$GITHUB_ENV"
    printf 'VERSION=%s\n' "$Version" >> "$GITHUB_ENV"
fi

# Always print results
printf 'TAG_NAME=%s\n' "$Tag"
printf 'VERSION=%s\n' "$Version"
