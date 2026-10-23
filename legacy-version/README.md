# Panel Splitter for Adobe Photoshop (Legacy Version)

![GitHub license](https://img.shields.io/github/license/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub last commit](https://img.shields.io/github/last-commit/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub pull requests](https://img.shields.io/github/issues-pr/Dilshan-H/Panel-Splitter?style=for-the-badge)

<!-- ![GitHub stars](https://img.shields.io/github/stars/Dilshan-H/Panel-Splitter?style=for-the-badge)
![GitHub forks](https://img.shields.io/github/forks/Dilshan-H/Panel-Splitter?style=for-the-badge) -->

![Panel Splitter for Photoshop - Cover](https://github.com/user-attachments/assets/67d6ccf8-491c-4e40-b10a-5070292ed3ad)


**NO LONGER MAINTAINED** 
---

> [!CAUTION]
> This is the legacy version of Panel Splitter for Adobe Photoshop which uses ExtendScript and is compatible with older versions of Photoshop. As of 23, October 2026, **this version is no longer maintained**. We recommend using the UXP version, which is actively maintained and offers enhanced features and performance.

## Transform Your Photoshop Workflow with Panel Splitter

Say goodbye to manual image splitting in Photoshop! Panel Splitter is the ultimate script designed to automate cropping your images or canvas along guides and exporting them as high-quality PDFs. Just tell it how many rows and columns you need, and watch it work its magic—preparing guides, cropping panels, and saving them as high quality PDFs in seconds. Ready to streamline your Photoshop workflow? Let’s dive in!

## 🚀 What’s New in This Update?

- Enhanced Performance: Faster processing and improved stability for a seamless experience.
- Bug fixes and feature enhancements, including a revamped UI!
- Background Update Checks: Automatically ensures you’re always using the latest version — no manual re-installations required!
- Usage Analytics (Optional): Share anonymous data to help us improve Panel Splitter based on how you use it.
- And more...

## ⏩ Simple Demo

Here's a example of what this tool can do:

![Demo](https://github.com/Dilshan-H/Panel-Splitter/assets/77499497/8ccfeb01-8aef-4052-ad3a-6994a36b5142)

## 🤔 Why use Panel Splitter?

Panel Splitter is a game-changing Photoshop script for designers, artists, and anyone who needs to split images into multiple panels effortlessly. Whether you’re preparing presentation panels or layouts, multi-page designs, or tiled artwork, this tool automates the process and delivers professional-grade PDF exports every time.

There is an alternative option that already available in Adobe Photoshop named `Slices from Guides`. But, it lacks the ability to export each panel in high quality state. (the `Export` feature is deprecated in modern versions as well). This tools helps in this situation by filling those gaps + We have more features in mind 😋

## ✨ Key Features

- Automatic Guide Creation: Skip the manual setup—Panel Splitter generates guides for you.
- High-Quality PDF Exports: Crisp, professional outputs ready for print or digital use.
- Customizable Splitting: Define your rows and columns with ease.
- Background Updates: Stay current with automatic version checks.
- Analytics Insights: Opt-in to help us refine the tool while keeping your data private.

## How to use Panel Splitter?

### 🖥️ Automatic Installation (Windows Only)

1. Download the executable from v2.2 release [here](https://github.com/Dilshan-H/Panel-Splitter/releases/tag/v2.2).
2. Double click on the downloaded file `Panel.Splitter.for.Adobe.Photoshop.exe` to install the software.
3. Start `Panel Splitter` and click `Install`, Panel Splitter will automatically identify installed Photoshop versions and configure as necessary. (But if this fails somehow, you can follow the 'Manual Installtion' method mentioned below.)
4. If your Photoshop application is already opened, close and restart it. (First time only - to load the script to PS)

### 🛠️ Manual Installation

1. Start `Panel Splitter` and click on `Extract`.
2. On the folder browsing dialog, locate the `Scripts` folder in Adobe Photoshop installation location. Generally it locates in;
   - Windows: `C:\Program Files\Adobe\Adobe Photoshop <version>\Presets\Scripts\`
   - Mac: `/Applications/Adobe Photoshop <version>/Presets/Scripts/`
3. If your Photoshop application is already opened, close and restart it. (First time only - to load the script to PS)

### ⚙ Install on Mac

1. Download the source code from latest release [here](https://github.com/Dilshan-H/Panel-Splitter/releases).
2. Extract the downloaded zip file.
3. Copy the `Panel Splitter.jsx` file (inside `legacy-version` directory) to the following location:
   - Mac: `/Applications/Adobe Photoshop <version>/Presets/Scripts/`
4. If your Photoshop application is already opened, close and restart it. (First time only - to load the script to PS)

### 🎨 Usage Steps

1. Open the image/canvas/panel in Adobe Photoshop.
2. Please make sure to **save your document**.
3. Navigate to `File > Scripts > Panel Splitter`
4. Provide a location to export the processed panels.
5. Input amount of rows and columns in the next dialog.
6. Relax — Panel Splitter will process everything and notify you when it’s done. Check your folder for the results!

## 🤝 Contributing to Panel Splitter

If you find Panel Splitter useful consider supporting its development by buying me a coffee. 

<a href="https://www.buymeacoffee.com/dilshanh" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>

**Note**: Since this is a legacy version, we are not accepting any new features or updates for this version. But, if you want to contribute to the UXP version, please feel free to do so.

**Development Setup**

1. Clone/Fork this repository.
2. Setup your development environment as discussed in here: https://extendscript.docsforadobe.dev/index.html
3. Set up Visual Studio as needed for updates on WPF application.
4. There are additional resources available within `Legacy-Version/References` folder - you can check them out as well.
5. Make your changes to the script and test it with `Adobe ExtendScript extension` for Visual Studio Code.

## License & Copyrights

**The MIT License**

• This program is free software: you can redistribute it and/or modify it under the terms of the MIT License. Attribution is required by leaving the author name and license info intact.
Please refer to the LICENSE file for more details.

• Adobe, Photoshop, ExtendScript, Visual Studio Code are copyrights and/or trademarks of their respective owners.

• Image Credits: [Andrea Piacquadio from Pexels](https://www.pexels.com/photo/photo-of-woman-looking-at-the-mirror-774866/)

## Privacy and Data Collection

Panel Splitter (Legacy Version) collects anonymized usage data by default to help us enhance the tool, but you can disable this anytime in the settings. Curious about what we collect and how we keep your privacy first? Check out our [Privacy Policy](https://github.com/Dilshan-H/Panel-Splitter/blob/main/legacy-version/PRIVACY.md) for more details.

In summary:

- No personal data collected (like emails etc.)
- Data stored in PostHog servers in EU.
- No sharing with third parties beyond PostHog.
- We use only the PostHog API, no SDKs - so, no complex data capture or session recordings.
- On PostHog `Discard client IP data` option is enabled and `process_person_profile` property is set to `false` in each request making them anonymous events.

## Disclaimer

Panel Splitter for Adobe Photoshop (Legacy Version) is provided "as is" without warranties of any kind, express or implied. While we strive to ensure the Application functions smoothly and securely, we are not responsible for any loss of data, system issues, or other damages arising from its use. The analytics data collected is anonymized and used solely to improve the Application, as outlined above. By using Panel Splitter, you acknowledge that you understand and accept these terms.

Since this version is no longer maintained, we strongly recommend transitioning to the UXP version for continued support and updates.