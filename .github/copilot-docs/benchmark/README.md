<!-- Maintainer-facing meta-doc (humans). Not an agent doc. -->
# DAG Docs Benchmark (regression safety net)

**Status: STARTER EXAMPLE — not wired into CI.** A small, versioned set of questions that guards
the [DAG documentation](../README.md) against regressions as it grows. Adding docs for one area can
degrade agent answers in another (context dilution or conflicting instructions); these tests turn
"did we make the agent worse?" into a repeatable check.

## The three-file pattern (canonical, tool-agnostic)

Each test is a folder under `tests/<id>/` with:

| File | Purpose |
| --- | --- |
| `question.md` | The prompt asked to a fresh agent/model. Ends with an evaluator note to **not** read `benchmark/`. |
| `expected-answer.md` | The gold-standard answer (pointer-based) used as the grading reference. |
| `evaluation.json` | Machine-readable rubric: `runs`, `pass_threshold`, `rubric_weights`, `must_mention`, `should_mention`, `must_not_hallucinate`. |

`evaluation.json` is the **source of truth** for a test's intent. The promptfoo config mirrors it.

## Starter tests

| Test | Guards | Notable |
| --- | --- | --- |
| `L1-architecture-tiers` | architecture-overview | dependency direction never reverses |
| `L2-command-execution` | git-command-execution + structured-commands | structured vs legacy patterns |
| `L2-translation-strings` | translation-system | must cite `update-loc.cmd` / `English.xlf` |
| `L3-commit-flow` | commit-flow | must say commit uses the **legacy** `ArgumentString` path |
| `L3-checkout-flow` | checkout-flow | must say checkout uses the **structured** `IGitCommand` path |

The last two deliberately test the same distinction in opposite directions — exactly the kind of
subtlety doc growth can break.

## Running it (promptfoo)

```pwsh
cd .github/copilot-docs/benchmark
$env:OPENAI_API_KEY = "<your key>"      # or ANTHROPIC_API_KEY; update the provider ids in the yaml
npx promptfoo@latest eval -c promptfooconfig.yaml --repeat 5
npx promptfoo@latest view               # results UI
```

- [`build-prompt.js`](build-prompt.js) loads the whole docs corpus **excluding `benchmark/`** and
  appends the question. Assertions come from [`promptfooconfig.yaml`](promptfooconfig.yaml).
- `--repeat 5` averages out model variance (the whitepaper uses 5 runs, 80% threshold).

## Critical gotchas

- **Never let the model read `benchmark/`.** `build-prompt.js` enforces this by skipping the
  `benchmark/` subtree; the `question.md` footnote is a second layer for other runners. If the
  model sees `expected-answer.md`, every test passes trivially and the suite is worthless.
- **Repeat runs.** A single run is noisy; a test passing 3/5 is a signal.
- **Grade pointers, not prose.** `must_mention` uses exact `ClassName`/`Method` strings.
- **Pin one model** and keep it fixed — you're detecting *changes* over time, not comparing models.

## Limitations (be honest about what this does NOT test)

promptfoo calls an LLM API with the docs in-context; it does **not** run the real VS Code agent
with file tools. So it faithfully catches **content errors** and **conflicting-instruction**
regressions, but it does **not** reproduce VS Code **token-budget dilution** (where the agent stops
reading before reaching the right doc). For that, run the same `question.md` prompts through the
actual agent periodically.

## Weighted scoring (optional)

This starter uses a simple all-assertions-pass gate. To replicate `evaluation.json`'s weighted 0.8
threshold in promptfoo, wrap a test's asserts in an `assert-set`:

```yaml
assert:
  - type: assert-set
    threshold: 0.8
    assert:
      - { type: icontains-all, value: ['FormCommit', 'DoCommit'], weight: 4 }
      - { type: llm-rubric, value: '...', weight: 6 }
```

## Adding a test

1. `tests/<id>/` with `question.md` (+ the evaluator footnote), `expected-answer.md`, `evaluation.json`.
2. Add a `tests:` entry to `promptfooconfig.yaml` mirroring `evaluation.json`.
3. Keep answers **pointer-based** and verified against source (see the
   [doc-management skill](../../skills/doc-management/SKILL.md)).
