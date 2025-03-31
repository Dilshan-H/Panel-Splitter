/*
  Project: Panel Splitter for Adobe Photoshop
  Author: dilshan-h [https://github.com/dilshan-h]
  Description: Split your image/canvas into cells using rows, columns & save them as high quality PDFs.
  Copyright (c) [2024] - MIT License
*/

// Save the current preferences
var startDisplayDialogs = app.displayDialogs;
var originalRulerUnits = app.preferences.rulerUnits;

// Set Adobe Photoshop to display no dialogs and set ruler units to pixels
app.displayDialogs = DialogModes.NO;
app.preferences.rulerUnits = Units.PIXELS;

var doc = app.activeDocument;
var continueScript = true;

// Check if the document is saved
if (!doc.saved) {
  alert(
    "Please save your document before running Panel Splitter!\nScript will be stopped now. Run again to continue."
  );
  continueScript = false;
}

if (continueScript) {
  // Confirmation dialog
  var dialog = new Window("dialog", "Confirmation | Panel Splitter v2.0");
  dialog.alignChildren = "left";
  var fontText = ScriptUI.newFont("Segoe UI", "Regular", 14);
  var fontBtns = ScriptUI.newFont("Segoe UI", "Regular", 14);

  dialog.add(
    "statictext",
    undefined,
    "Welcome to Panel Splitter!"
  ).graphics.font = fontText;
  dialog.add(
    "statictext",
    undefined,
    "Easily split your document into multiple panels and save them as high-quality PDFs."
  ).graphics.font = fontText;
  dialog.add(
    "statictext",
    undefined,
    "On the next screen, you will be asked to enter the number of rows and columns."
  ).graphics.font = fontText;
  dialog.add("statictext", undefined, "Do you want to proceed?").graphics.font =
    fontText;

  var buttonGroup = dialog.add("group");
  buttonGroup.alignment = "center";
  buttonGroup.alignChildren = "center";

  var yesButton = buttonGroup.add("button", undefined, "Yes");
  yesButton.graphics.font = fontBtns;
  var noButton = buttonGroup.add("button", undefined, "No");
  noButton.graphics.font = fontBtns;

  var footerGroup = dialog.add("group");
  footerGroup.alignment = "center";
  footerGroup.alignChildren = "center";
  footerGroup.add(
    "statictext",
    undefined,
    "Panel Splitter • Made with ❤ by dilshan-h • github.com/dilshan-h"
  );

  yesButton.onClick = function () {
    dialog.close();
  };

  noButton.onClick = function () {
    continueScript = false;
    dialog.close();
  };

  dialog.show();

  if (continueScript) {
    // Select output folder
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
    // Duplicate the document
    var tempDoc = doc.duplicate();
    try {
      var success = cropAndSavePDFs(tempDoc, outputFolder);
      if (success) {
        alert("Files successfully saved!");
      }
    } catch (e) {
      alert(
        "An error occurred: " +
          e.message +
          "\nScript will be stopped now. Please reach out to the developer."
      );
    } finally {
      tempDoc.close(SaveOptions.DONOTSAVECHANGES);
    }
  }
}

// Reset the application preferences
app.displayDialogs = startDisplayDialogs;
app.preferences.rulerUnits = originalRulerUnits;

// Function to prompt user for rows and columns
function promptRowsColumns() {
  var dialog = new Window(
    "dialog",
    "Enter Rows and Columns count | Panel Splitter"
  );
  dialog.alignChildren = "center";
  dialog.preferredSize.width = 200;
  dialog.add("statictext", undefined, "Rows:");
  var rowsInput = dialog.add("edittext", undefined, "2");
  rowsInput.characters = 5;
  dialog.add("statictext", undefined, "Columns:");
  var colsInput = dialog.add("edittext", undefined, "2");
  colsInput.characters = 5;

  var buttonGroup = dialog.add("group");
  buttonGroup.alignment = "center";
  buttonGroup.alignChildren = "center";

  var okButton = buttonGroup.add("button", undefined, "OK");
  okButton.graphics.font = fontBtns;
  var cancelButton = buttonGroup.add("button", undefined, "Cancel");
  cancelButton.graphics.font = fontBtns;

  dialog.add(
    "statictext",
    undefined,
    "Panel Splitter • Made with ❤ by dilshan-h • github.com/dilshan-h"
  );

  okButton.onClick = function () {
    dialog.close(1);
  };
  cancelButton.onClick = function () {
    dialog.close(0);
  };
  if (dialog.show() == 1) {
    var rows = parseInt(rowsInput.text);
    var columns = parseInt(colsInput.text);
    if (isNaN(rows) || isNaN(columns)) {
      alert("Please enter valid numbers.");
      return null;
    }
    return [rows, columns];
  } else {
    return null;
  }
}

// Function to clear existing guides
function clearGuides(doc) {
  doc.guides.removeAll();
}

// Function to add outer guides
function setOuterGuides(doc) {
  var width = doc.width.value;
  var height = doc.height.value;
  doc.guides.add(Direction.HORIZONTAL, 0);
  doc.guides.add(Direction.HORIZONTAL, height);
  doc.guides.add(Direction.VERTICAL, 0);
  doc.guides.add(Direction.VERTICAL, width);
}

// Function to add new guides based on rows and columns
function addGuides(doc, rows, columns) {
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
function saveAsPDF(tempDoc, selectionBounds, outputPath, outputFolder) {
  var bounds = selectionBounds;
  tempDoc.crop(bounds);
  var saveOptions = new PDFSaveOptions();
  saveOptions.compatibility = PDFCompatibility.PDF14;
  saveOptions.embedThumbnail = true;
  saveOptions.encoding = PDFEncoding.JPEG;
  saveOptions.jpegQuality = 12;
  saveOptions.layers = false;
  saveOptions.preserveEditing = false;
  saveOptions.view = false;
  tempDoc.saveAs(new File(outputFolder + outputPath), saveOptions);
}

// Main function
function cropAndSavePDFs(tempDoc, outputFolder) {
  var rowsColumns = promptRowsColumns();
  if (!rowsColumns || rowsColumns.length !== 2) return false;

  var rows = rowsColumns[0];
  var columns = rowsColumns[1];

  clearGuides(tempDoc);
  setOuterGuides(tempDoc);
  addGuides(tempDoc, rows, columns);

  // Unlock the background layer if it exists and is locked
  if (
    tempDoc.layers.length > 0 &&
    tempDoc.layers[tempDoc.layers.length - 1].isBackgroundLayer
  ) {
    tempDoc.layers[tempDoc.layers.length - 1].isBackgroundLayer = false;
  }

  // Flatten the document
  tempDoc.flatten();
  var flattenedState = tempDoc.activeHistoryState;

  var guides = tempDoc.guides;
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

      try {
        saveAsPDF(tempDoc, selectionBounds, outputPath, outputFolder);
      } catch (e) {
        alert("Error saving PDF: " + e.message);
        return false;
      }
      tempDoc.activeHistoryState = flattenedState;
      pdfCounter++;
    }
  }
  return true;
}
