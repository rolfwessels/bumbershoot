# Cursor Rules for 1p-api-portal

## Custom Commands

### /pr-review
**Description**: Systematically review all changes between main and current branch, check adherence to how-to-ai.md guidelines, report issues, and generate PR title and description.

**Instructions**:
1. Get the current branch name (remember you might be on windows so check pwd first!)
2. Runs the make task to generate a comprehensive summary

```bash
docker compose --profile dev exec dev make pr-review
```

This creates `PR_REVIEW.md` containing:
- Current branch and base branch info
- File statistics (lines added/removed)
- Commit history
- Full diff of all changes

(If pr-review task does not exist, please add it to the Makefile)

3. For each changed file, check adherence to `docs/how-to-ai.md` guidelines:
   - SOLID principles followed
   - Methods are < 10 lines (report any longer)
   - No unnecessary comments. Only comment on public API and in tests we comment Arrange, Act, Assert (report any found)
   - Records use primary constructors
   - Constructor dependency injection used
   - `var` used when type is obvious
   - Test files use Setup() method pattern (not [SetUp] attribute)
   - Test files follow sample reuse pattern (one base sample, variations reuse it)
   - Test files use NSubstitute (not Moq)
   - No extra features beyond what's specified
   - Use `OrFailNoKey()` helper for database lookups instead of manual null checks (e.g., `await context.GetX().OrFailNoKey()`)
   - Note: Manual null checks with ValidationException are acceptable for STATE validation of already-loaded objects (e.g., `if (publishedValues == null) throw new ValidationException("must have values")`)
4. Check for common issues:
   - New features should have tests
   - Linter errors introduced
   - Breaking changes without justification
5. Generate a PR title (short, imperative mood, e.g., "Add feature flag support for GraphQL queries")
6. Generate a PR description with:
   - Summary of changes
   - List of key files modified
   - Any concerns or suggestions
   - Confirmation that guidelines were followed (or list violations)

**Output Format**: As markdown!
```md
## PR Review Results

### Issues Found:
[List any violations or concerns]

### Suggested PR Title:
[Short imperative title]

### Suggested PR Description:
[Multi-line description with bullet points]
```

**Usage**: Type `/pr-review` in chat

