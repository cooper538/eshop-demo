#!/usr/bin/env node

/**
 * Topological Task Sorter
 * Parses task markdown files and sorts them by dependencies using Kahn's algorithm.
 *
 * Usage:
 *   node sort-tasks.mjs <path>           # Human-readable output
 *   node sort-tasks.mjs <path> --json    # JSON output
 *   node sort-tasks.mjs <path> --markdown # Markdown table output
 */

import { readdir, readFile, stat } from 'fs/promises';
import { join, basename, relative } from 'path';

// Parse command line arguments
const args = process.argv.slice(2);
const jsonOutput = args.includes('--json');
const markdownOutput = args.includes('--markdown');
const searchPath = args.find(arg => !arg.startsWith('--')) || '.claude/project';

/**
 * Recursively find all task-*.md files
 */
async function findTaskFiles(dir) {
  const files = [];

  async function walk(currentDir) {
    try {
      const entries = await readdir(currentDir, { withFileTypes: true });

      for (const entry of entries) {
        const fullPath = join(currentDir, entry.name);

        if (entry.isDirectory()) {
          await walk(fullPath);
        } else if (entry.isFile() && entry.name.startsWith('task-') && entry.name.endsWith('.md')) {
          files.push(fullPath);
        }
      }
    } catch {
      // Directory doesn't exist or can't be read
    }
  }

  await walk(dir);
  return files;
}

/**
 * Parse a task markdown file and extract ID, name, and dependencies
 */
async function parseTaskFile(filePath) {
  try {
    const content = await readFile(filePath, 'utf-8');

    // Extract task name from first heading
    const nameMatch = content.match(/^#\s+Task\s+\d+:\s*(.+)$/m) ||
                      content.match(/^#\s+(.+)$/m);
    const name = nameMatch ? nameMatch[1].trim() : basename(filePath, '.md');

    // Extract ID from Metadata table
    const idMatch = content.match(/\|\s*ID\s*\|\s*([^|\s][^|]*[^|\s]|[^|\s])\s*\|/i);
    const id = idMatch ? idMatch[1].trim() : null;

    // Extract Dependencies from Metadata table
    const depsMatch = content.match(/\|\s*Dependencies\s*\|\s*([^|]*)\s*\|/i);
    let dependencies = [];

    if (depsMatch) {
      const depsStr = depsMatch[1].trim();
      if (depsStr && depsStr !== '-' && depsStr !== 'none' && depsStr !== '—') {
        dependencies = depsStr.split(',').map(d => d.trim()).filter(Boolean);
      }
    }

    return { id, name, dependencies, filePath };
  } catch {
    return null;
  }
}

/**
 * Topological sort using Kahn's algorithm
 * Returns { sorted: Task[], cycles: string[] }
 */
function topologicalSort(tasks) {
  const taskMap = new Map(tasks.map(t => [t.id, t]));
  const inDegree = new Map();
  const adjList = new Map();

  // Initialize
  for (const task of tasks) {
    inDegree.set(task.id, 0);
    adjList.set(task.id, []);
  }

  // Build graph
  for (const task of tasks) {
    for (const dep of task.dependencies) {
      if (taskMap.has(dep)) {
        adjList.get(dep).push(task.id);
        inDegree.set(task.id, inDegree.get(task.id) + 1);
      }
    }
  }

  // Find all nodes with in-degree 0
  const queue = [];
  for (const [id, degree] of inDegree) {
    if (degree === 0) {
      queue.push(id);
    }
  }

  const sorted = [];

  while (queue.length > 0) {
    const current = queue.shift();
    sorted.push(taskMap.get(current));

    for (const neighbor of adjList.get(current)) {
      inDegree.set(neighbor, inDegree.get(neighbor) - 1);
      if (inDegree.get(neighbor) === 0) {
        queue.push(neighbor);
      }
    }
  }

  // Check for cycles
  const cycles = [];
  if (sorted.length !== tasks.length) {
    const remaining = tasks.filter(t => !sorted.includes(t));
    cycles.push(...remaining.map(t => t.id));
  }

  return { sorted, cycles };
}

/**
 * Validate dependencies - find missing references
 */
function validateDependencies(tasks) {
  const validIds = new Set(tasks.map(t => t.id));
  const warnings = [];

  for (const task of tasks) {
    for (const dep of task.dependencies) {
      if (!validIds.has(dep)) {
        warnings.push(`Task "${task.id}" references unknown dependency "${dep}"`);
      }
    }
  }

  return warnings;
}

/**
 * Format output as human-readable text
 */
function formatText(sorted, cycles, warnings, entryPoints) {
  const lines = [];

  lines.push(`✅ Topological order (${sorted.length} tasks):\n`);

  sorted.forEach((task, index) => {
    const deps = task.dependencies.length > 0
      ? ` (depends on: ${task.dependencies.join(', ')})`
      : '';
    lines.push(`${index + 1}. ${task.id} - ${task.name}${deps}`);
  });

  if (entryPoints.length > 0) {
    lines.push(`\n⚡ Entry points (no dependencies): ${entryPoints.map(t => t.id).join(', ')}`);
  }

  if (cycles.length > 0) {
    lines.push(`\n❌ Circular dependencies detected: ${cycles.join(', ')}`);
  }

  if (warnings.length > 0) {
    lines.push(`\n⚠️ Warnings:`);
    warnings.forEach(w => lines.push(`   - ${w}`));
  }

  return lines.join('\n');
}

/**
 * Format output as JSON
 */
function formatJson(sorted, cycles, warnings, entryPoints) {
  return JSON.stringify({
    tasks: sorted.map((task, index) => ({
      order: index + 1,
      id: task.id,
      name: task.name,
      dependencies: task.dependencies,
      filePath: task.filePath
    })),
    entryPoints: entryPoints.map(t => t.id),
    cycles,
    warnings,
    totalTasks: sorted.length
  }, null, 2);
}

/**
 * Format output as Markdown table
 */
function formatMarkdown(sorted, cycles, warnings, entryPoints) {
  const lines = [];

  lines.push('## Execution Order (Topological Sort)\n');
  lines.push('| Order | Task ID | Name | Dependencies |');
  lines.push('|-------|---------|------|--------------|');

  sorted.forEach((task, index) => {
    const deps = task.dependencies.length > 0 ? task.dependencies.join(', ') : '-';
    lines.push(`| ${index + 1} | ${task.id} | ${task.name} | ${deps} |`);
  });

  if (entryPoints.length > 0) {
    lines.push(`\n**Entry points (no dependencies):** ${entryPoints.map(t => t.id).join(', ')}`);
  }

  if (cycles.length > 0) {
    lines.push(`\n**Circular dependencies detected:** ${cycles.join(', ')}`);
  }

  if (warnings.length > 0) {
    lines.push(`\n**Warnings:**`);
    warnings.forEach(w => lines.push(`- ${w}`));
  }

  return lines.join('\n');
}

// Main execution
async function main() {
  // Check if path exists
  try {
    await stat(searchPath);
  } catch {
    console.error(`Error: Path "${searchPath}" does not exist`);
    process.exit(1);
  }

  // Find and parse task files
  const taskFiles = await findTaskFiles(searchPath);

  if (taskFiles.length === 0) {
    console.log(`No task files found in ${searchPath}`);
    process.exit(0);
  }

  const parsedTasks = await Promise.all(taskFiles.map(parseTaskFile));
  const tasks = parsedTasks.filter(t => t && t.id);

  if (tasks.length === 0) {
    console.log('No valid tasks with ID found');
    process.exit(0);
  }

  // Validate and sort
  const warnings = validateDependencies(tasks);
  const { sorted, cycles } = topologicalSort(tasks);
  const entryPoints = sorted.filter(t => t.dependencies.length === 0);

  // Output
  if (jsonOutput) {
    console.log(formatJson(sorted, cycles, warnings, entryPoints));
  } else if (markdownOutput) {
    console.log(formatMarkdown(sorted, cycles, warnings, entryPoints));
  } else {
    console.log(formatText(sorted, cycles, warnings, entryPoints));
  }

  // Exit with error code if cycles detected
  if (cycles.length > 0) {
    process.exit(1);
  }
}

main().catch(err => {
  console.error('Error:', err.message);
  process.exit(1);
});
