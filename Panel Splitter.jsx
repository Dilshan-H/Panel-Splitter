/*
  Project: Panel Splitter for Adobe Photoshop
  Author: dilshan-h [https://github.com/dilshan-h]
  Description: Split your image/canvas into cells using rows, columns & save them as high quality PDFs.
  Copyright (c) [2024] - MIT License
*/

// flag to control script execution
var continueScript = true;
// Save the current preferences
var startDisplayDialogs = app.displayDialogs;
// Set Adobe Photoshop to display no dialogs
app.displayDialogs = DialogModes.NO;
// Store the state of the document before changes
var doc = app.activeDocument;
var initialState = doc.activeHistoryState;

var dialog = new Window("dialog", "Confirmation - Panel Splitter");
dialog.alignChildren = "left";
var fontText = ScriptUI.newFont("Segoe UI", "Regular", 14);
var fontBtns = ScriptUI.newFont("Segoe UI", "Regular", 14);

// Add a message label
var messageLine1 = dialog.add(
  "statictext",
  undefined,
  "IMPORTANT: Make sure that;"
);
messageLine1.graphics.font = fontText;
var messageLine2 = dialog.add(
  "statictext",
  undefined,
  "1. You have saved your file."
);
messageLine2.graphics.font = fontText;
var messageLine3 = dialog.add(
  "statictext",
  undefined,
  "2. You have a backup of this document."
);
messageLine3.graphics.font = fontText;
var messageLine4 = dialog.add(
  "statictext",
  undefined,
  "Do you want to proceed?"
);
messageLine4.graphics.font = fontText;

// Add a group to contain buttons
var buttonGroup = dialog.add("group");
buttonGroup.alignment = "center";
buttonGroup.alignChildren = "center";

// Add Yes and No buttons
var yesButton = buttonGroup.add("button", undefined, "Yes");
yesButton.graphics.font = fontBtns;
var noButton = buttonGroup.add("button", undefined, "No");
noButton.graphics.font = fontBtns;

var bottomText = dialog.add(
  "statictext",
  undefined,
  "Panel Splitter • Made with ❤ by dilshan-h • github.com/dilshan-h"
);

// Functions to handle button click events
yesButton.onClick = function () {
  alert("On the next dialog box, select a location to save the panels.");
  dialog.close();
};

noButton.onClick = function () {
  alert("Script will be stopped now. Run again to continue.");
  dialog.close();
  continueScript = false;
};

dialog.show();

if (continueScript) {
  // ask the user for the output folders
  var outputFolder = Folder.selectDialog(
    "Select a folder for the output files"
  );
  if (outputFolder == null) {
    alert(
      "An output directory is required!\nScript will be stopped now. Run again to continue."
    );
    continueScript = false;
  }
}

if (continueScript) {
  cropAndSavePDFs();

  // Reset the application preferences
  app.displayDialogs = startDisplayDialogs;

  // Undo changes to revert the document state
  doc.activeHistoryState = initialState;

  app.activeDocument.close(SaveOptions.DONOTSAVECHANGES);
  alert("Files successfully saved!");
}

// Function to prompt user for rows and columns
function promptRowsColumns() {
  var rows = parseInt(prompt("Enter the number of rows:", ""));
  var columns = parseInt(prompt("Enter the number of columns:", ""));
  return [rows, columns];
}

// Function to clear existing guides
function clearGuides() {
  var doc = app.activeDocument;
  doc.guides.removeAll();
}

// Function to add outer guides
function setOuterGuides() {
  var doc = app.activeDocument;
  var width = doc.width.value;
  var height = doc.height.value;

  doc.guides.add(Direction.HORIZONTAL, 0);
  doc.guides.add(Direction.HORIZONTAL, height);
  doc.guides.add(Direction.VERTICAL, 0);
  doc.guides.add(Direction.VERTICAL, width);
}

// Function to add new guides based on rows and columns
function addGuides(rows, columns) {
  var doc = app.activeDocument;
  var width = doc.width.value;
  var height = doc.height.value;

  var horizontalSpacing = width / columns;
  var verticalSpacing = height / rows;

  for (var i = 1; i < rows; i++) {
    doc.guides.add(Direction.HORIZONTAL, verticalSpacing * i);
  }

  for (var j = 1; j < columns; j++) {
    doc.guides.add(Direction.VERTICAL, horizontalSpacing * j);
  }
}

// Function to save selection as PDF
function saveAsPDF(selectionBounds, outputPath) {
  var doc = app.activeDocument;
  var bounds = selectionBounds;

  doc.crop(bounds);
  var saveOptions = new PDFSaveOptions();
  saveOptions.compatibility = PDFCompatibility.PDF14;
  saveOptions.embedThumbnail = true;
  saveOptions.encoding = PDFEncoding.JPEG;
  saveOptions.jpegQuality = 12;
  saveOptions.layers = false;
  saveOptions.preserveEditing = false;
  saveOptions.view = false;
  doc.saveAs(new File(outputFolder + outputPath), saveOptions);
}

// Main function
function cropAndSavePDFs() {
  var rowsColumns = promptRowsColumns();
  if (!rowsColumns || rowsColumns.length !== 2) return;

  var rows = rowsColumns[0];
  var columns = rowsColumns[1];

  var doc = app.activeDocument;

  clearGuides();
  setOuterGuides();
  addGuides(rows, columns);

  // Merge visible layers
  doc.flatten();

  var guides = doc.guides;

  var pdfCounter = 1;

  var horizontalGuides = [];
  var verticalGuides = [];

  // Group guides based on orientation
  for (var i = 0; i < guides.length; i++) {
    if (guides[i].direction === Direction.HORIZONTAL) {
      horizontalGuides.push(guides[i]);
    } else if (guides[i].direction === Direction.VERTICAL) {
      verticalGuides.push(guides[i]);
    }
  }

  // Sort guides based on their coordinates
  horizontalGuides.sort(function (a, b) {
    return a.coordinate - b.coordinate;
  });

  verticalGuides.sort(function (a, b) {
    return a.coordinate - b.coordinate;
  });

  for (var i = 0; i < rows; i++) {
    for (var j = 0; j < columns; j++) {
      var top = horizontalGuides[i].coordinate;
      var bottom = horizontalGuides[i + 1].coordinate;

      var left = verticalGuides[j].coordinate;
      var right = verticalGuides[j + 1].coordinate;

      var selectionBounds = [left, top, right, bottom];
      var outputPath = "/Panel_" + pdfCounter + ".pdf";

      saveAsPDF(selectionBounds, outputPath);
      doc.activeHistoryState = doc.historyStates[doc.historyStates.length - 2];
      pdfCounter++;
    }
  }
}
