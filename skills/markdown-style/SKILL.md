---
name: markdown-style
description: "Markdown authoring and review skill - the two-layer rule set (syntax canon = valid, portable Markdown from the Markdown Guide; style overlay = opinionated house form from Google's style guide) plus the review procedure. Load when authoring or restructuring any .md (README, ADR, runbook, how-to, design doc) or on an explicit 'lint / style-check / fix this markdown', 'ATX vs setext', 'should I use a TOC?', or 'fix the headings / list indentation' ask. Markdown form only - not prose clarity (that is Vale) or spelling (codespell / hunspell)."
metadata:
  type: reference
  sources: "Distilled 2026-07 from josiahsiegel/claude-plugin-marketplace (doc-master/markdown-style). Two layers derived from the Markdown Guide basic-syntax reference (CC BY-SA 4.0) and Google's developer-docs Markdown style guide (Apache 2.0) - see Attribution. Trimmed to house voice: dangling sibling-skill routing removed, per-finding approval ceremony replaced with the decisive apply-then-flag procedure."
---

# markdown-style

The Markdown authoring and review skill. Owns two layers of rules and the procedure for applying them:

1. **Syntax canon** - what valid, portable Markdown looks like. Full canon: `references/syntax-canon.md`.
2. **Style overlay** - opinionated rules a reviewer enforces on top of valid syntax. Full overlay: `references/style-overlay.md`.

The question this skill answers is **'is this doc well-formed?'** - not 'does this doc belong here?' Style review never decides whether a doc should exist; it assumes the doc earned its place and asks whether the prose structure and Markdown are clean.

## How to run a review

Two passes, syntax before style. The reviewer reads a syntax violation differently from a style violation, so do not interleave them.

### Pass 1 - syntax (must-fix)

Walk the file top to bottom. Syntax violations are bugs (invalid or non-portable Markdown: setext where ATX is expected, unfenced code block, missing blank line around a block element, `)` instead of `.` in an ordered list, missing space after `#`), not judgment calls - **fix them directly in one Edit pass**. No approval gate; the diff is self-explaining and each fix cites its rule by short name (e.g. `syntax/headings/atx-space-after`).

### Pass 2 - style (should-fix)

Re-walk for style-overlay violations. These are opinionated. **Apply the clear wins directly** - fenced blocks with a language tag, single H1 as the title, informative link text (never 'here'), no trailing whitespace, product-name capitalization. **Batch the genuine judgment calls** - `[TOC]` on a borderline-length doc, table-vs-list, reference-vs-inline links, heading-uniqueness prefixes - into one short list, each with a recommendation, and move. Do not gate each finding on a reply.

Defer to the project on any conflict with a local convention (e.g. a repo standardized on `_underscore_` emphasis) - note the conflict, defer, move on. No audit markers or `[reviewed]` stamps in the file; the diff is the audit trail.

## The two layers - quick reference

Detailed rules with examples live in `references/`. These summaries cover the violations that account for most findings.

### Syntax (must-fix)

| Construct           | Rule                                                                            |
|---------------------|---------------------------------------------------------------------------------|
| Headings            | ATX (`#`-`######`), space after `#`, blank lines before and after.              |
| Paragraphs          | Separated by a blank line. Do not indent.                                       |
| Line breaks         | Trailing-two-spaces is controversial (invisible). Prefer a paragraph break.     |
| Emphasis            | `**bold**`, `*italic*`, `***both***`. Asterisks mid-word (underscores break).   |
| Blockquotes         | `>` prefix; `>` on the blank line between paragraphs; nest with `>>`.           |
| Ordered lists       | `1.` `2.` `3.` (period, not `)`). Start at 1. Numbering can be lazy.            |
| Unordered lists     | Choose one of `-` / `*` / `+`; do not mix within a list.                        |
| Nested list content | Indent 4 spaces (1 tab). Code inside a list item indents 8 spaces (2 tabs).     |
| Inline code         | Single backticks. Double backticks if the code contains a backtick.             |
| Code blocks         | Fenced with a language tag. Indented blocks are valid but discouraged.          |
| Horizontal rule     | Three or more `---` / `***` / `___` alone on a line with blank lines around.    |
| Links               | `[text](url)`. Autolink with `<https://...>`. Reference links resolve elsewhere.|
| Images              | `![alt](path "title")`. Always include alt text.                                |
| Inline HTML         | Allowed. Separate block-level HTML with blank lines. Do not indent the tags.    |

Full canon with examples and known-broken edge cases: `references/syntax-canon.md`.

### Style (should-fix)

| Concern               | Rule                                                                                          |
|-----------------------|-----------------------------------------------------------------------------------------------|
| H1                    | Exactly one H1, used as the document title (match or nearly match the filename). Rest start at H2. |
| Heading style         | ATX only. No setext underlines.                                                               |
| Heading uniqueness    | Avoid bare repeated subheadings ('Summary', 'Example') under multiple parents. Prefix them.   |
| Document skeleton     | Title -> optional owner -> 1-3 sentence intro -> `[TOC]` -> `## Topic` sections -> `## See also`. |
| Table of contents     | `[TOC]` for any doc that would not fit on one screen. Between intro and first H2.              |
| Line length           | 80 chars. Exceptions: links, tables, headings, code blocks. Prose around a long link still wraps. |
| Trailing whitespace   | None. Prefer a paragraph break to the two-space line-break trick.                             |
| Lists                 | Lazy numbering (`1.` repeated) for long lists; full numbering for short stable ones. Prefer lists to tables for one-dimensional data. |
| Code fencing          | Always fenced, never indented. Always declare a language (`text` if none). Escape wrapped shell newlines with `\`. |
| Links                 | Repo-absolute paths (`/path/to/page.md`) over `https://...` for in-repo links. Avoid `../` traversal. |
| Link text             | Informative. Never 'here', 'link', or a raw URL.                                              |
| Reference links       | Use in tables, when the URL hurts readability, or when a target repeats. Define just before the next heading. |
| Tables                | Only for scannable two-dimensional data. No prose cells (cells cannot wrap).                  |
| Images                | Sparingly. Always alt text.                                                                    |
| Capitalization        | Preserve product / tool / binary capitalization (`Markdown`, not `markdown`; `npm`, `iOS`).   |

Full overlay with rationales: `references/style-overlay.md`.

## What this skill does NOT cover

Markdown **form** only - heading style, list indentation, code-fence language tags, link syntax, line length, and the rest of the two rule sets above. It does not cover:

- **Whether a doc should exist, or where it belongs** - that is a content / architecture decision, not a form review. Style review assumes the doc earned its place.
- **Prose-level enforcement** - terminology consistency, inclusive-language substitutions, banned phrases, voice / tense / mood. Reach for [Vale](https://vale.sh) (a prose linter that runs configurable style packs over Markdown). It composes with this skill: this skill fixes the Markdown form, Vale enforces prose terminology.
- **Spelling and grammar** - use `codespell` / `hunspell` / `LanguageTool` in addition, not instead.
- **AsciiDoc / reStructuredText / org-mode / MDX** - Markdown only; other markup has its own canonical rules (e.g. `asciidoctor --safe-mode`).

## Hard limits

- Syntax findings (must-fix) take precedence over style findings (should-fix). Never re-order.
- Never invent rules. Every finding cites a rule from `references/syntax-canon.md` or `references/style-overlay.md`.
- The style overlay is opinionated. On any dispute, defer to the user / project - note the conflict and move on.

## Examples

<example>
<user_query>Is `===` under a heading line valid Markdown?</user_query>
<response>
Yes - that is setext-style H1, valid Markdown. But the style overlay (and Google's guide) prefer ATX (`# Heading`): setext is annoying to maintain, cannot express H3 or below, and obscures the level when scanning. Recommendation: replace `===` with a single `#` - a syntax-equivalent rewrite. Point me at the file and I will apply it.
</response>
</example>

<example>
<user_query>Lint this README - the formatting is messy.</user_query>
<response>
Running the two-pass review. Pass 1 fixes syntax bugs (fences, blank lines, ATX headings, list markers) directly. Pass 2 applies the clear style wins and flags any judgment calls (TOC, table-vs-list) as a short list. Send the path.
</response>
</example>

## Attribution

The two layers are distilled from publicly available style references - load-bearing rules only, not verbatim reproduction:

- **Syntax canon** - the Markdown Guide basic-syntax reference (<https://www.markdownguide.org/basic-syntax/>), CC BY-SA 4.0.
- **Style overlay** - Google's developer-documentation Markdown style guide (<https://google.github.io/styleguide/docguide/style.html>), from `google/styleguide`, Apache License 2.0.

When the user needs the original text, link out - do not paste long excerpts.
