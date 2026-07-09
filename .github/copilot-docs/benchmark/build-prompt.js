// Builds the prompt for each benchmark test by loading the DAG documentation corpus
// and appending the test question. Used by promptfooconfig.yaml (`prompts: file://build-prompt.js`).
//
// CRITICAL: this deliberately EXCLUDES the benchmark/ folder so the model can never see
// expected-answer.md while answering (which would make every test pass trivially).
//
// This approximates the DAG runtime: instead of the agent selectively opening docs via file
// tools (which promptfoo cannot do), we place the whole (small) docs corpus in context and test
// whether the docs, taken together, still yield the correct pointers. It catches content errors
// and conflicting-instruction regressions. It does NOT reproduce VS Code token-budget dilution.

const fs = require('fs');
const path = require('path');

/** Recursively collect *.md files under dir, skipping any benchmark/ subtree. */
function collectDocs(dir, baseDir, out) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.name === 'benchmark') {
      continue; // never leak the expected answers into the prompt
    }

    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      collectDocs(full, baseDir, out);
    } else if (entry.name.endsWith('.md')) {
      const rel = path.relative(baseDir, full).replace(/\\/g, '/');
      out.push(`\n\n===== FILE: ${rel} =====\n${fs.readFileSync(full, 'utf8')}`);
    }
  }
}

module.exports = async function ({ vars }) {
  const docsRoot = path.resolve(__dirname, '..'); // .github/copilot-docs
  const parts = [];
  collectDocs(docsRoot, docsRoot, parts);

  const system = [
    'You are an expert on the Git Extensions codebase.',
    'Answer ONLY using the Git Extensions DAG documentation provided below.',
    'Prefer precise code pointers (ClassName / Method / file path). Do NOT invent classes or methods.',
    'If the documentation does not cover something, say so rather than guessing.',
    '',
    '===== GIT EXTENSIONS DAG DOCUMENTATION =====',
    parts.join(''),
  ].join('\n');

  return [
    { role: 'system', content: system },
    { role: 'user', content: vars.question },
  ];
};
