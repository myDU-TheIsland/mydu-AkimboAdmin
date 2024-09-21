# Mod AkimboAdmin UI

## Overview
This folder contains the user interface files for the Mod AkimboAdmin. You will need to compile these files using Webpack and CleanCSS before they can be used in the mod.

## Prerequisites
Make sure you have Node.js and npm installed on your machine. You will also need to install Webpack and CleanCSS.

## Installation

1. **Install Dependencies:**
   Run `npm install` in the UI folder to install the required packages.

2. **Build the UI Files:**
   - Compile the JavaScript files using Webpack by running `npx webpack`.
   - Compile the CSS files using CleanCSS with the command `cleancss -o ../dist/ModAkimboAdmin/AkimboAdminHud.css ./styles/AkimboAdminHud.css`.

   This will place the compiled files in the `dist` folder of the main root directory.

## Output Location
After running the build commands, the compiled files will be located in: `../dist/ModAkimboAdmin/`

## Conclusion
Once you have built the UI files, you can proceed to use them with the Mod AkimboAdmin. If you have any questions or encounter issues, please refer to the project documentation or reach out for assistance.