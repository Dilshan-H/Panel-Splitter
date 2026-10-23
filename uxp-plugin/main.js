"use strict";

const { app, core, action, constants } = require("photoshop");
const { entrypoints, storage, shell } = require("uxp");
const fs = storage.localFileSystem;
const grid = require("./lib/grid.js");

// --- Author info ---
const AUTHOR = {
  name: "Dilshan-H",
  github: "https://github.com/Dilshan-H/Panel-Splitter",
  coffee: "https://www.buymeacoffee.com/dilshanh",
};

const $ = (id) => document.getElementById(id);
const STORE_KEY = "panelSplitter.settings.v2";
const state = { folder: null, busy: false };

// --- PDF save descriptor ---
const PDF_FORMAT = {
  _obj: "photoshopPDFFormat",
  pdfPresetFilename: "High Quality Print",
  pdfPreserveEditing: false,
  pdfCompressionType: 7,
};

// --- UI helpers ---
function getChecked(groupId) {
  const r = Array.from($(groupId).querySelectorAll("sp-radio")).find(
    (x) => x.checked,
  );
  return r ? r.value : null;
}
function setChecked(groupId, value) {
  Array.from($(groupId).querySelectorAll("sp-radio")).forEach((r) => {
    r.checked = r.value === value;
  });
}
function setStatus(msg, kind) {
  const el = $("status");
  el.textContent = msg || "";
  el.className = kind || "";
}
function plural(n, word) {
  return n + " " + word + (n === 1 ? "" : "s");
}

// --- Settings ---
function readUI() {
  return {
    mode: getChecked("mode") || "guides",
    format: getChecked("format") || "pdf",
    quality: Number($("quality").value) || 10,
    rows: $("rows").value,
    cols: $("cols").value,
    prefix: $("prefix").value,
  };
}
function saveSettings(extra) {
  try {
    const prev = JSON.parse(localStorage.getItem(STORE_KEY) || "{}");
    localStorage.setItem(
      STORE_KEY,
      JSON.stringify(Object.assign(prev, readUI(), extra || {})),
    );
  } catch (e) {
    // Ignore JSON parse errors
  }
}
async function restoreSettings() {
  let s = {};
  try {
    s = JSON.parse(localStorage.getItem(STORE_KEY) || "{}");
  } catch (e) {
    // Ignore JSON parse errors
  }
  if (s.mode) setChecked("mode", s.mode);
  if (s.format) setChecked("format", s.format);
  if (s.quality) $("quality").value = s.quality;
  if (s.rows) $("rows").value = s.rows;
  if (s.cols) $("cols").value = s.cols;
  if (typeof s.prefix === "string") $("prefix").value = s.prefix;
  if (s.folderToken) {
    try {
      const f = await fs.getEntryForPersistentToken(s.folderToken);
      if (f && f.isFolder) {
        state.folder = f;
        $("folderPath").textContent = f.nativePath;
      }
    } catch (e) {
      // Ignore errors when accessing the folder
    }
  }
}

// --- Planning: which panels will be cut ---
function readGuides(doc) {
  const h = [],
    v = [];
  for (let i = 0; i < doc.guides.length; i++) {
    const g = doc.guides[i];
    (String(g.direction).toLowerCase() === "horizontal" ? h : v).push(
      g.coordinate,
    );
  }
  return { h, v };
}

function parseCount(text) {
  const n = Number(String(text).trim());
  return Number.isInteger(n) && n >= 1 && n <= 100 ? n : null;
}

// Returns { doc, xEdges, yEdges, cells } or { error }.
function plan(modeOverride) {
  if (!app.documents.length) return { error: "Open a document to begin." };
  const doc = app.activeDocument;
  const W = Math.round(doc.width),
    H = Math.round(doc.height);
  const mode = modeOverride || getChecked("mode") || "guides";
  let xEdges, yEdges;

  if (mode === "guides") {
    const g = readGuides(doc);
    if (!g.h.length && !g.v.length) {
      return {
        error:
          "No guides found. Drag guides from the rulers or use View > New Guide.",
      };
    }
    xEdges = grid.edgesFromGuides(g.v, W);
    yEdges = grid.edgesFromGuides(g.h, H);
  } else {
    const rows = parseCount($("rows").value),
      cols = parseCount($("cols").value);
    if (!rows || !cols)
      return { error: "Rows and columns must be whole numbers from 1 to 100." };
    xEdges = grid.edgesFromSplit(cols, W);
    yEdges = grid.edgesFromSplit(rows, H);
  }
  const cells = grid.buildCells(xEdges, yEdges);
  if (!cells.length) return { error: "These guides produce no panels." };
  return { doc, xEdges, yEdges, cells };
}

function describe(p) {
  const r = p.yEdges.length - 1,
    c = p.xEdges.length - 1;
  return (
    plural(r, "row") +
    " x " +
    plural(c, "column") +
    " = " +
    plural(p.cells.length, "panel")
  );
}

function refresh() {
  if (state.busy) return;
  const mode = getChecked("mode") || "guides";
  $("guidesBox").style.display = mode === "guides" ? "block" : "none";
  $("gridBox").style.display = mode === "grid" ? "block" : "none";
  $("qualityBox").style.display =
    getChecked("format") === "jpg" ? "block" : "none";
  try {
    const p = plan();
    $(mode === "guides" ? "guideSummary" : "gridSummary").textContent =
      p.error || describe(p);
  } catch (e) {
    $("guideSummary").textContent = "";
  }
}

let refreshTimer = null;
function scheduleRefresh() {
  clearTimeout(refreshTimer);
  refreshTimer = setTimeout(refresh, 250);
}

// --- Photoshop operations ---
function px(v) {
  return { _unit: "pixelsUnit", _value: v };
}

async function play(descriptors) {
  const res = await action.batchPlay(descriptors, {
    synchronousExecution: true,
  });
  const bad = res && res.find((r) => r && r._obj === "error");
  if (bad) throw new Error(bad.message || "Photoshop reported an error");
  return res;
}

// Crop with doc.crop() method, fallback to crop action if fails
async function cropTo(doc, c) {
  const w = c.right - c.left,
    h = c.bottom - c.top;
  const ok = () => Math.round(doc.width) === w && Math.round(doc.height) === h;
  try {
    await doc.crop({
      left: c.left,
      top: c.top,
      right: c.right,
      bottom: c.bottom,
    });
  } catch (e) {
    // try fallback
  }
  if (ok()) return;
  await play([
    {
      _obj: "crop",
      to: {
        _obj: "rectangle",
        top: px(c.top),
        left: px(c.left),
        bottom: px(c.bottom),
        right: px(c.right),
      },
      angle: { _unit: "angleUnit", _value: 0 },
      delete: true,
      cropAspectRatioModeKey: {
        _enum: "cropAspectRatioModeClass",
        _value: "pureAspectRatio",
      },
      constrainProportions: false,
      _options: { dialogOptions: "dontDisplay" },
    },
  ]);
  if (!ok())
    throw new Error("Photoshop could not crop to " + w + " x " + h + " px.");
}

// Remove stray "copy" suffixes from some PS versions
async function removeCopySuffix(folder, name) {
  const dot = name.lastIndexOf(".");
  const stray = name.slice(0, dot) + " copy" + name.slice(dot);
  let entry = null;
  try {
    entry = await folder.getEntry(stray);
  } catch (e) {
    return;
  }
  try {
    const target = await folder.getEntry(name);
    await target.delete();
  } catch (e) {
    // Ignore errors when deleting the target
  }
  await entry.moveTo(folder, { newName: name, overwrite: true });
}

async function saveCurrent(doc, o, name) {
  const file = await o.folder.createFile(name, { overwrite: true });
  if (o.format === "jpg") {
    await doc.saveAs.jpg(
      file,
      { quality: o.quality, embedColorProfile: true },
      true,
    );
  } else {
    const token = await fs.createSessionToken(file);
    await play([
      {
        _obj: "save",
        as: PDF_FORMAT,
        in: { _path: token, _kind: "local" },
        documentID: doc.id,
        copy: true,
        _options: { dialogOptions: "dontDisplay" },
      },
    ]);
  }
  await removeCopySuffix(o.folder, name);
}

// --- Export to temp doc ---
async function exportCells(ctx, p, o) {
  const original = p.doc;
  const work = await original.duplicate("Panel Splitter [TEMP]", true);
  let saved = 0;
  try {
    if (
      o.format === "jpg" &&
      work.bitsPerChannel !== constants.BitsPerChannelType.EIGHT
    ) {
      work.bitsPerChannel = constants.BitsPerChannelType.EIGHT;
    }
    const base = work.activeHistoryState;
    const total = p.cells.length;
    const ext = o.format === "jpg" ? ".jpg" : ".pdf";
    for (let i = 0; i < total; i++) {
      if (ctx.isCancelled) break;
      ctx.reportProgress({
        value: i / total,
        commandName: "Working on panel " + (i + 1) + " of " + total,
      });
      await cropTo(work, p.cells[i]);
      await saveCurrent(work, o, o.prefix + grid.padIndex(i + 1, total) + ext);
      saved++;
      work.activeHistoryState = base;
    }
  } finally {
    try {
      work.closeWithoutSaving();
    } catch (e) {
      // ignore already closed state
    }
    try {
      app.activeDocument = original;
    } catch (e) {
      // ignore
    }
  }
  return saved;
}

// --- Button handlers ---
async function chooseFolder() {
  try {
    const f = await fs.getFolder();
    if (!f) return;
    state.folder = f;
    $("folderPath").textContent = f.nativePath;
    saveSettings({ folderToken: await fs.createPersistentToken(f) });
    setStatus("");
  } catch (e) {
    setStatus("Could not open that folder: " + e.message, "error");
  }
}

async function placeGrid() {
  const p = plan("grid");
  if (p.error) return setStatus(p.error, "error");
  try {
    await core.executeAsModal(
      async () => {
        p.doc.guides.removeAll();
        p.xEdges
          .slice(1, -1)
          .forEach((x) => p.doc.guides.add(constants.Direction.VERTICAL, x));
        p.yEdges
          .slice(1, -1)
          .forEach((y) => p.doc.guides.add(constants.Direction.HORIZONTAL, y));
      },
      { commandName: "Panel Splitter: Placing guides" },
    );
    setStatus(
      "Guides placed. Nudge them if needed, then switch to Current guides.",
      "ok",
    );
  } catch (e) {
    setStatus("Could not place guides: " + e.message, "error");
  }
}

async function runExport() {
  if (state.busy) return;
  const p = plan();
  if (p.error) return setStatus(p.error, "error");
  if (!state.folder) return setStatus("Choose a folder to save to.", "error");

  const ui = readUI();
  const prefix = grid.sanitizeName(ui.prefix) || "Panel_";
  const o = {
    folder: state.folder,
    format: ui.format,
    quality: ui.quality,
    prefix: prefix,
  };

  saveSettings();
  state.busy = true;
  $("exportBtn").disabled = true;
  setStatus("Exporting " + plural(p.cells.length, "panel") + "...");
  try {
    let saved = 0;
    await core.executeAsModal(
      async (ctx) => {
        saved = await exportCells(ctx, p, o);
      },
      { commandName: "Panel Splitter: Exporting panels" },
    );
    const where = state.folder.nativePath;
    setStatus(
      saved === p.cells.length
        ? "Saved " + plural(saved, "file") + " to " + where
        : "Stopped. Saved " + saved + " of " + p.cells.length + " to " + where,
      saved === p.cells.length ? "ok" : "error",
    );
  } catch (e) {
    setStatus("Export failed: " + (e && e.message ? e.message : e), "error");
  } finally {
    state.busy = false;
    $("exportBtn").disabled = false;
    refresh();
  }
}

// --- Wire up ---
function onChoice() {
  setTimeout(() => {
    saveSettings();
    refresh();
  }, 0);
}

["mode", "format"].forEach((id) => {
  $(id).addEventListener("change", onChoice);
  Array.from($(id).querySelectorAll("sp-radio")).forEach((r) =>
    r.addEventListener("click", onChoice),
  );
});
["rows", "cols"].forEach((id) =>
  $(id).addEventListener("input", scheduleRefresh),
);
$("quality").addEventListener("change", () => saveSettings());
$("prefix").addEventListener("change", () => saveSettings());
$("refreshBtn").addEventListener("click", refresh);
$("folderBtn").addEventListener("click", chooseFolder);
$("placeBtn").addEventListener("click", placeGrid);
$("exportBtn").addEventListener("click", runExport);

// Update Guide counts - on changes
try {
  action.addNotificationListener(
    ["make", "delete", "set", "open", "close", "select"].map((event) => ({
      event,
    })),
    scheduleRefresh,
  );
} catch (e) {
  // refresh on progress
}

// External web links
async function openLink(url, reason) {
  try {
    await shell.openExternal(url, reason);
  } catch (e) {
    setStatus(
      "Could not open the link: " + (e && e.message ? e.message : e),
      "error",
    );
  }
}
$("authorName").textContent = AUTHOR.name;
$("ghBtn").addEventListener("click", () =>
  openLink(
    AUTHOR.github,
    "Opening the Panel Splitter GitHub page in your browser.",
  ),
);
$("coffeeBtn").addEventListener("click", () =>
  openLink(AUTHOR.coffee, "Opening the Buy Me a Coffee page in your browser."),
);

entrypoints.setup({
  panels: {
    panelSplitter: {
      show() {
        refresh();
      },
    },
  },
});

restoreSettings().then(refresh);
