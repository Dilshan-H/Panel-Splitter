# Panel Splitter for Adobe Photoshop

![GitHub license](https://img.shields.io/github/license/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub last commit](https://img.shields.io/github/last-commit/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub pull requests](https://img.shields.io/github/issues-pr/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub stars](https://img.shields.io/github/stars/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub forks](https://img.shields.io/github/forks/Dilshan-H/Panel-Splitter?style=for-the-badge)

![Panel Splitter for Adobe Photoshop - Cover](https://github.com/user-attachments/assets/922b3745-f7d2-4cb8-8139-9ce7fe8c3a56)

## Transform Your Photoshop Workflow with Panel Splitter

Say goodbye to manual image splitting in Photoshop! Panel Splitter is designed to automate cropping your images or canvas along guides and exporting them as high-quality PDFs/JPEGs. Just tell it how many rows and columns you need, or use your existing guides layout -> select the export format & location -> hit `Export Panels`.

## 🚀 What’s New in This Update?

Earlier version of Panel Splitter was an ExtendScript based script that required manual installation and lacked a user-friendly interface. The new version is a modern UXP plugin that Adobe ecosystem supports, providing a seamless experience with a native interface.

- All tools at one panel
- Quickly add guides to your canvas (rows x columns)
- Supports exporting both PDF & JPEG formats
- Uses pixels as the default unit instead of user's unit settings
- Set custom quality for JPEG outputs (1-12 range)
- Now you can set a custom file name prefix for the sliced panels

## ⏩ Simple Demo

Here's a example of what this tool can do:

![Demo](https://github.com/Dilshan-H/Panel-Splitter/assets/77499497/8ccfeb01-8aef-4052-ad3a-6994a36b5142)

## 🖼️ Inside Panel Splitter

<img width="1360" height="800" alt="Panel Splitter in action" src="https://github.com/user-attachments/assets/16d037a2-0447-4aab-98e2-42bb4aa97956" />

## 📥 Download & Installation

**Supported Photoshop Versions:** Photoshop 24.0 (2023) or later. Older versions have not been tested and may not work properly. In that case, you can try the legacy version of Panel Splitter available [here](https://github.com/Dilshan-H/Panel-Splitter/tree/main/legacy-version).


### Get the plugin from Adobe Exchange:

> Plugin is under review
<!--
[![Adobe Exchange](https://img.shields.io/badge/Adobe%20Exchange-Install%20Now-orange?style=for-the-badge&logo=academia)](https://exchange.adobe.com/apps/cc/ed576e29/panel-splitter)
-->

### Or from GitHub releases:

You can download the latest version of **Panel Splitter** from the [releases page](https://github.com/Dilshan-H/Panel-Splitter/releases).

- If you have **Adobe Creative Cloud** installed:

  You just need to double click the downloaded `.ccx` file, and it will show a prompt to install the plugin. After installation, restart Photoshop and you should see the **Panel Splitter** plugin in the `Plugins` menu.

- If you don't have/use **Adobe Creative Cloud**:

  Install a file archiver like [7-Zip](https://www.7-zip.org/) -> Right click on the downloaded `.ccx` file -> Select `7-Zip` -> `Extract to <filename>.ccx` -> Copy the extracted directory to your Photoshop plugins directory (e.g., `C:\Program Files\Adobe\Adobe Photoshop <version>\Plug-ins\` on Windows or `/Applications/Adobe Photoshop <version>/Plug-ins/` on Mac) -> Restart Photoshop and you should see the **Panel Splitter** plugin in the `Plugins` menu.

> [!NOTE]
> Please note that if you download the plugin via GitHub, you may need to enable developer mode in Photoshop to use it. Follow [this guide](https://developer.adobe.com/photoshop/uxp/2022/guides/devtool/installation) to enable developer mode.

## 🤔 Why use Panel Splitter?

Panel Splitter helps designers, artists, and anyone who needs to split images into multiple panels effortlessly. Whether you’re preparing presentation panels, multi‑page layouts, or tiled artwork, this tool automates the process and delivers high‑quality outputs that you can send directly to print or use digitally. It saves time, reduces errors, and enhances your workflow efficiency.

There is an alternative option that already available in Adobe Photoshop named `Slices from Guides`. But, it lacks the ability to export each panel in high quality state. (the `Export` feature is deprecated in modern versions as well). This tool fills that gap.

## ✨ Key Features

- Automatic Guide Creation: Skip the manual setup; Panel Splitter generates guides for you.
- Export panels as PDFs or JPEGs
- Define custom export quality for JPEGs (1-12 range)
- Customizable Splitting: Define your rows and columns with ease.
- Free & Open Source, no ads, no trackers, no paywalls.

### 🎨 Usage Steps

1. Open the image/canvas/panel in Adobe Photoshop.
2. Please make sure to **save your document**.
3. Open the Panel Splitter plugin from the `Plugins` menu.
4. Select `Split by method` (either by `Current Guides` or `Rows and Columns`).
5. If you choose `Rows and Columns`, enter the number of rows and columns you want to split your image into. (**TIP**: You can use `Place Guides` button to automatically place guides on your canvas and optionally nudge them as needed -> Then select `Current Guides`)
6. Select `Save as` format (PDF or JPEG). If you choose JPEG, you can set the quality (1-12 range).
7. Select the output directory where you want to save the split panels.
8. You can customize the file name prefix for the exported panels. (Default is `Panel_`).
9. Click the `Export Panels` button to start the process. The plugin will automatically split your image and save the panels in the specified format and location.

## 🤝 Contributing to Panel Splitter

Panel Splitter is a Free and Open Source project. Please consider supporting its development with a small donation.

<a href="https://www.buymeacoffee.com/dilshanh" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>

Love what Panel Splitter can do? Help us make it even better! Whether you’ve found a bug, have a feature idea, or want to tweak the code, we’re excited to collaborate.
Feel free to [open an issue](https://github.com/dilshan-h/Panel-Splitter/issues/new) or submit a pull request. For major changes, please open an issue first to discuss what you would like to change.

**Development Setup**

1. Clone/Fork this repository Or download the source code as a ZIP file and extract it.
2. Follow the instructions in the [Adobe UXP Developer Tool](https://developer.adobe.com/photoshop/uxp/2022/guides/devtool/installation) to set up your development environment. (You need a Adobe account, Creative Cloud Desktop App, and Adobe Photoshop installed on your system)
3. Load the plugin from the UXP Developer Tool.
4. With the plugin loaded, you can make changes to the source code and see them reflected in Photoshop in real-time. Use the UXP Developer Tool to reload the plugin after making changes.
5. _Optional:_ Install `Prettier` for code formatting and `cSpell` for spell checking in your code editor to maintain code quality.

> [!WARNING]  
> Please note that plugin `id` defined in the `manifest.json` file must be unique. If you are testing your own version of the plugin, make sure to change the `id` to avoid conflicts with the official release.

## License & Copyrights

- **The MIT License**: This program is free software: you can redistribute it and/or modify it under the terms of the MIT License. Attribution is required by leaving the author name and license info intact.
  Please refer to the LICENSE file for more details.

- You are not allowed to use the Panel Splitter name, logo, or any other branding elements for your own projects. Please refrain from attempting to upload or distribute modified versions of Panel Splitter on Adobe Exchange or any other platform without prior permission from the author.

- _Adobe, Adobe Exchange, Creative Cloud, Photoshop, UXP Developer Tool_ are either registered trademarks or trademarks of Adobe in the United States and/or other countries.

## Privacy and Data Collection

Panel Splitter does not collect any personal data. No ads, no trackers, no data sharing with third parties. Curious about how we keep your privacy first? Check out our [Privacy Policy](https://github.com/Dilshan-H/Panel-Splitter/blob/main/PRIVACY.md) for more details.

## Disclaimer

Panel Splitter for Adobe Photoshop is provided "as is" without warranties of any kind, express or implied. While we strive to ensure the Application functions smoothly and securely, we are not responsible for any loss of data, system issues, or other damages arising from its use. By using Panel Splitter, you acknowledge that you understand and accept these terms.
