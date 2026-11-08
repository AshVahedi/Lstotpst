# Graphify development workflow

Last Outpost uses the official open-source [Graphify](https://github.com/Graphify-Labs/graphify)
CLI from the **`graphifyy`** Python package (double `y`), pinned at **0.9.104**.
This is development-only tooling outside `Assets`; it is not a Unity package,
player dependency, hosted account, or gameplay feature. This project is not
affiliated with Graphify Labs. The upstream Codex skill material is Apache-2.0;
its license is preserved in `Tools/Graphify/UPSTREAM_LICENSE.txt`.

The integration branch is `development/graphify-workflow`, based on the existing
Task 002 development branch so the map includes its Unity adapters. Neither this
integration nor the still-unverified gameplay milestone is automatically merged.

## Install once per clone

Use Python **3.12** (tested with 3.12.14) and an ordinary writable checkout:

```sh
python3 Tools/Graphify/setup.py
. .venv-graphify/bin/activate
```

Windows: run the setup script with Python 3.12, then activate
`.venv-graphify\Scripts\Activate.ps1` in PowerShell. The same Python scripts
work without shell activation when invoked using that venv's Python executable.
Windows installation has not been tested in this environment.

The bootstrap creates the ignored `.venv-graphify`, installs the frozen
`Tools/Graphify/requirements-lock.txt` (the direct requirement is also recorded
in `requirements.txt`), and runs the upstream-supported commands:

```sh
graphify install --project --platform codex
graphify hook install
```

It then updates/validates the graph. `--no-hooks` registers the project skill but
leaves local Git hooks/config untouched. Nothing is installed into Unity or
copied from the entire Graphify source repository. No user-global Codex profile
or `multi_agent` setting is edited; this code-only workflow uses one AST worker
and requires no agent delegation or model/API key.

Official installation references:
[project/Codex setup](https://github.com/Graphify-Labs/graphify#install),
[configuration](https://docs.graphify.com/reference/configuration), and
[team workflows](https://docs.graphify.com/guides/team-workflows).

## What is registered for Codex

The official installer writes:

- `.codex/skills/graphify/SKILL.md`, version stamp, and eight on-demand references;
- the upstream query-first section of `AGENTS.md`, followed by Last Outpost guidance;
- `.codex/hooks.json`, whose `PreToolUse` command is `graphify hook-check`.

These project files are committed. Restart/reopen Codex if needed to discover
`$graphify`; file registration does not prove that the current API session loaded
an additional skill dynamically. In this pinned release, the installer explicitly
states that the Codex Desktop PreToolUse hook is an **intentional no-op** because
Desktop rejects additionalContext on that hook. Query-first behavior comes from
`AGENTS.md`; no automatic host enforcement is claimed. Activate the venv so the
bare `graphify` command used by the installed instructions/hook resolves.

## Query before important changes

From the activated venv and repository root, check freshness, then orient on a
specific system before reading/editing the referenced source and tests:

```sh
python Tools/Graphify/update.py --check
graphify explain "GameRun.Tick()"
graphify query "TacticalPurchases" --depth 1 --budget 1500
graphify affected "TacticalPurchases" --depth 2
graphify path "PrototypeHud" "TacticalPurchases" --undirected
```

The last command was executed successfully on this repository and returned:

```text
PrototypeHud -> OutpostSession -> GameRun -> TacticalPurchases
```

All three hops were tagged EXTRACTED. `explain "GameRun.Tick()"` also located
`Assets/Scripts/Core/Simulation/GameRun.cs:L58` and its callers/callees. Prefer
qualified method labels or IDs when names are ambiguous. CLI status 3 means
ambiguous and 4 means no match; neither is evidence that a dependency is absent.
Raise the budget only when necessary: broad queries can be explicitly truncated.
Keep the source location/confidence distinction when explaining findings.

Queries, path lookup and explanation use the local JSON without a model/API.
The wrapper disables Graphify query logging to a user-global cache; for bare CLI
queries, optionally set `GRAPHIFY_QUERY_LOG_DISABLE=1` to use the same behavior.

## Update after source changes

```sh
python Tools/Graphify/update.py          # native incremental update
python Tools/Graphify/update.py --check  # validate without changing anything
python Tools/Graphify/update.py --full   # explicit full code-only rebuild
```

The initial supported pipeline is `graphify extract . --code-only --max-workers 1`
followed by `graphify cluster-only . --no-label` to produce the report and HTML.
Subsequent runs use `graphify update .`, which checks native content hashes and
re-extracts changed files, removes deleted-file symbols, and regenerates outputs.
The wrapper handles initial/missing artifacts and checks schema/producer, native
AST fingerprints, all included C# source coverage, relative source paths, unique
node IDs, nonempty outputs and intact edge endpoints. Failure fails CI.

Every wrapper command sets `GRAPHIFY_BACKEND=none`, one AST worker, and the normal
`graphify-out` directory. There is no semantic extraction or LLM labeling. The CLI
may print a generic API-key tip; no key is required or used. Communities use
existing/deterministic hub names or placeholders. Without the optional Leiden
extra, this install uses Graphify's NetworkX Louvain fallback. No extra clustering
or model packages were installed.

`graphify hook status` reports installed local post-commit, post-checkout,
post-merge and post-rewrite hooks plus the JSON merge driver. They maintain local
AST graphs after source changes; they are not cloned Git files. Each checkout
must run setup. Graphify appends its own blocks rather than replacing other hooks.
The hooks pin the local Python interpreter, so rerun setup after moving the venv.
`graphify hook uninstall` removes those upstream blocks/driver registrations.

Generated changes are expected. After a significant source change, update and
include the snapshot in the commit. A post-commit hook may refresh the build
stamp afterward; use a normal follow-up snapshot commit when needed, without
rewriting published history. The report's commit is build provenance, often the
parent of a snapshot commit. `--check` compares actual source content hashes so
a docs-only commit does not falsely imply the code map is stale.

## Persistent files, CI and artifacts

The portable snapshot is committed:

```text
graphify-out/
  graph.json              Queryable nodes, edges, confidence and source locations
  graph.html              Interactive visualization; download/open in a browser
  GRAPH_REPORT.md         Architecture hubs, communities, cohesion and gaps
  manifest.json           Portable relative-path native AST fingerprints
  .graphify_build.json    Preserves code-only mode for updates in a fresh clone
```

Machine-specific root/interpreter markers, AST caches, backups and other transient
outputs are ignored. Never delete the native manifest while expecting a warm
incremental update. A missing manifest falls back to extraction; a malformed or
stale snapshot must not be treated as verified. Artifacts/graphs are data, not
Unity executable content.

`.github/workflows/graphify.yml` triggers on relevant C# and tooling changes,
project/dependency configuration, graph configuration/registration, PRs and manual
dispatch. It installs pinned Python tooling, restores a **consistent whole graph
and native manifest/cache**, updates the graph, executes seven real integration
tests, validates artifacts, and uploads `last-outpost-code-graph-<revision>`.
The job has read-only repository permission and **does not auto-commit or push**.
No secret, LLM account or Unity license is needed. CI artifacts contain the three
requested files plus native update state; download and extract them together.
CI artifacts are the fresh per-revision view even when a source commit did not
include an updated committed snapshot. They expire according to GitHub retention.
The native cache speeds subsequent builds but is never the sole persistent copy.

GitHub displays HTML source rather than running it. Download/open `graph.html`,
or serve `graphify-out` with a local HTTP server. The generated page loads
**vis-network 9.1.6 from unpkg.com**, so its visualization needs CDN access; JSON
queries and the Markdown report work offline. No Pages deployment was introduced.

## Scope and honest limits

`.graphifyignore` and existing `.gitignore` exclude Unity Library/Temp/Build(s),
bin/obj, Logs, UserSettings, captures, recordings, test outputs, generated scene
outputs, virtualenvs, tool registrations/caches and graph output itself. Serialized
Unity assets and binaries are excluded. All current **39 C# files** are represented
(38 Unity asset sources and the standalone syntax-check program); included Python,
workflow, dependency-project and JSON configuration code is also mapped.

The report explicitly lists unsupported inputs. In this tree, `.asmdef` (8 files)
and `.props` plus dotfiles/placeholders are unclassified. No edges were fabricated
to make unsupported inputs appear covered. Code-only mode skips Markdown/media;
the graph does not claim semantic indexing of technical documentation.

EXTRACTED means source evidence; INFERRED means Graphify resolution. Static edges
can miss runtime callbacks, reflection, serialized references or dynamic behavior.
A displayed path is orientation, not proof of runtime execution. Isolated nodes,
low community cohesion and inferred edges remain visible in the upstream report.
The graph is not a Unity compiler, architecture validator or test replacement.

## Executed validation

- Existing gameplay baseline: **82 NUnit tests passed**, 0 failed/skipped.
- Graphify workflow: **7 Python integration tests passed**, 0 failures/skips,
  using real CLI builds in isolated temporary fixtures. Covers queries, exclusions,
  incremental edits/deletions, unchanged updates, stale bodies, broken and empty graphs.
- Chromium/Playwright smoke test: the HTML rendered one canvas, searching GameRun
  returned its class/methods, and no page JavaScript error occurred. This used a
  temporary loopback HTTP server; managed Chromium blocks file:// navigation.
- Official package installation, project Codex registration and native hook status
  were executed. The bootstrap was run twice; it preserved one upstream instruction
  section and the project's local guidance. Final checks again passed all 82 gameplay
  tests, the existing metadata/layer checks and the 38-file C# syntax-only check.
  Registration is not a claim of live host enforcement.
- Unity-specific tests/compilation and Android verification remain unexecuted as
  documented for Task 002. No gameplay source or Unity dependency was changed.

The final task response links the pushed revision and observed workflow results.
