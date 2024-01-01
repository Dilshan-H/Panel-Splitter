# Panel Splitter for Adobe Photoshop

![GitHub license](https://img.shields.io/github/license/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub last commit](https://img.shields.io/github/last-commit/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub pull requests](https://img.shields.io/github/issues-pr/Dilshan-H/Panel-Splitter?style=for-the-badge)

<!-- ![GitHub stars](https://img.shields.io/github/stars/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub forks](https://img.shields.io/github/forks/Dilshan-H/Panel-Splitter?style=for-the-badge) -->

![Panel Splitter for Photoshop - Cover](https://github.com/Dilshan-H/Panel-Splitter/assets/77499497/ad1e722b-be01-4689-86bd-1d0e997b8015)

## What is Panel Splitter?

Panel Splitter is a script that helps you to instantly crop images/canvas along the guides in Photoshop and export them as high quality PDFs. All you need to provide is how many rows and columns that are needed. Panel Splitter will prepare the guides, crop each panel and save them as PDFs for you!

## Simple Demo

Here's a example of what this tool can do:

![Demo](https://github.com/Dilshan-H/Panel-Splitter/assets/77499497/8ccfeb01-8aef-4052-ad3a-6994a36b5142)


## Why use Panel Splitter?

There is an alternative option that already available in Adobe Photoshop named `Slices from Guides`. But, it lacks the ability to export each panel in high quality state. (the `Export` feature is deprecated in modern versions as well). This tools helps in this situation by filling those gaps.

## How to use this tool?

### Automatic Installation (Windows Only)

1. Download the latest release from [here](https://github.com/Dilshan-H/Panel-Splitter/releases).
2. Double click on the downloaded file `Panel.Splitter.for.Adobe.Photoshop.exe` to install the script. Make sure that you have Adobe Photoshop installed in your computer before running the installer. Otherwise, the installer will extract the script files to your desktop. From there, you can manually install the script to Photoshop [See next section].
3. If your Photoshop application is already opened, close and restart it. (First time only - to load the script to PS)

### Manual Installation

1. Download the latest release from [here](https://github.com/Dilshan-H/Panel-Splitter/releases).
2. Extract the downloaded zip file.
3. Copy the `Panel Splitter.jsx` file to the following location:
   - Windows: `C:\Program Files\Adobe\Adobe Photoshop <version>\Presets\Scripts\`
   - Mac: `/Applications/Adobe Photoshop <version>/Presets/Scripts/`
4. If your Photoshop application is already opened, close and restart it. (First time only - to load the script to PS)

### Usage
1. Open the image/canvas/panel
2. Please make sure to **save your document** and **keep a backup copy** for safety.
3. Go to `File > Scripts > Panel Splitter`
4. Provide a location to export the processed panels.
5. Input amount of rows and columns respectively in the next dialogs.
6. That's it.. After a moment, the script will show a message saying that the process is complete. Check the output location for PDF files.

## [ICYMI]: Important Notes for Users

- Always save your document before running this script!
- Make sure to keep a backup copy just in case, if something goes wrong.

## Contributing

Got an idea? Found a bug? Feel free to [open an issue](https://github.com/dilshan-h/Panel-Splitter/issues/new) or submit a pull request. For major changes, please open an issue first to discuss what you would like to change.

1. Clone/Fork this repository.
2. Setup your development environment as discussed in here: https://extendscript.docsforadobe.dev/index.html
3. There are additional resources available within `References` folder - you can check them out as well.
4. Make your changes to the script and test it with `Adobe ExtendScript extension` for Visual Studio Code.

## License & Copyrights

**The MIT License**

• This program is free software: you can redistribute it and/or modify it under the terms of the MIT License. Attribution is required by leaving the author name and license info intact.
Please refer to the LICENSE file for more details.

• Adobe, Photoshop, ExtendScript, Visual Studio Code are copyrights and/or trademarks of their respective owners.

• Image Credits: [Andrea Piacquadio from Pexels](https://www.pexels.com/photo/photo-of-woman-looking-at-the-mirror-774866/)
