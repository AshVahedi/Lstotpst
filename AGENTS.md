## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, use the installed graphify skill or instructions before doing anything else.

Rules:
- When graphify-out/graph.json exists, run `graphify query "<question>"` before your first grep, search or source-file read for a codebase question, and put this rule in every subagent prompt that explores code. Read raw files only for lines the graph or an earlier read already located. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
- A committed graphify-out/graph.json is rewritten whole on every build, so a large diff to it in a pull request is expected. Do not flag it as churn or suggest skipping the graph rebuild or its hooks.

## Last Outpost graph workflow

- Graphify is development-only tooling. Do not change gameplay or Unity package references for graph maintenance.
- Run `python3 Tools/Graphify/setup.py` once per checkout, then activate `.venv-graphify` so the official `graphify` command is on PATH.
- Use the repository-local pinned interpreter/package; do not auto-upgrade Graphify or switch to an unpinned uv tool environment from the generic upstream runbook. This workflow is code-only; do not start semantic/model extraction unless separately requested.
- Before changing important systems, run a scoped query such as `graphify explain "GameRun.Tick()"` or `graphify query "TacticalPurchases" --depth 1`; inspect the referenced source and tests before editing.
- Validate graph freshness with `python Tools/Graphify/update.py --check`. If stale, run `python Tools/Graphify/update.py` in the activated venv; this uses native incremental AST updates, no model/API backend.
- After significant source changes, refresh and include the portable graph snapshot in the commit. Native Git hooks update local graphs after commits/checkouts/merges/rewrites; hooks are local and must be installed in each clone.
- The graph is static evidence, with EXTRACTED/INFERRED confidence tags. It does not parse Unity `.asmdef` files or prove serialized references, Unity compilation, runtime behavior, or test success.
- Read `Documentation/GRAPHIFY.md` for setup, limitations, CI artifacts, and the distinction between project skill registration and the host's actual skill/hook support.
