"use strict";

// Helper functions for Panel Splitter

// Boundaries (in pixels) along one axis
function edgesFromGuides(positions, size) {
  const pts = [0, size];
  positions.forEach(function (p) {
    const r = Math.round(p);
    if (r > 0 && r < size) pts.push(r);
  });
  pts.sort(function (a, b) {
    return a - b;
  });
  return pts.filter(function (v, i) {
    return i === 0 || v !== pts[i - 1];
  });
}

// Evenly split an axis into n parts
function edgesFromSplit(n, size) {
  const out = [];
  for (let i = 0; i <= n; i++) out.push(Math.round((i * size) / n));
  return out;
}

// Row-major cells (left to right - top to bottom)
function buildCells(xEdges, yEdges) {
  const cells = [];
  for (let r = 0; r < yEdges.length - 1; r++) {
    for (let c = 0; c < xEdges.length - 1; c++) {
      const cell = {
        left: xEdges[c],
        top: yEdges[r],
        right: xEdges[c + 1],
        bottom: yEdges[r + 1],
      };
      if (cell.right - cell.left >= 1 && cell.bottom - cell.top >= 1)
        cells.push(cell);
    }
  }
  return cells;
}

// Pad index for sorting
function padIndex(i, total) {
  const width = String(total).length;
  let s = String(i);
  while (s.length < width) s = "0" + s;
  return s;
}

// Sanitize strings
function sanitizeName(s) {
  return String(s)
    .replace(/[\\/:*?"<>|]/g, "_")
    .trim();
}

module.exports = {
  edgesFromGuides,
  edgesFromSplit,
  buildCells,
  padIndex,
  sanitizeName,
};
