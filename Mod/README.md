# Mod AkimboAdmin Source Files Guide

## Overview
This document provides instructions for setting up and building the source files of the Mod AkimboAdmin. Before you can successfully build the source, you'll need to add specific DLL dependencies from the MyDU Mod Toolkit examples.

## Prerequisites
Make sure you have the following DLLs available before building the source:

- `backend.dll`
- `backend.PubSub.dll`
- `BotLib.dll`
- `Interfaces.dll`
- `Router.dll`
- `Router.Orleans.dll`
- `NQUtils.dll`

## Steps to Prepare the Source Files

1. **Download the Source Files:**
   - Clone the repository or download the source files as a ZIP archive from the GitHub repository.

2. **Locate the Dependency DLLs:**
   - Navigate to the MyDU Mod Toolkit examples directory where the required DLLs are located.

3. **Copy the DLLs:**
   - Copy the following DLL files into the `bin` or `libs` folder of the Mod AkimboAdmin source project:
     - `backend.dll`
     - `backend.PubSub.dll`
     - `BotLib.dll`
     - `Interfaces.dll`
     - `Router.dll`
     - `Router.Orleans.dll`
     - `NQUtils.dll`

4. **Build the Source:**
   - Open the solution file in your preferred C# IDE (e.g., Visual Studio).
   - Ensure that all required DLLs are referenced correctly in the project.
   - Build the project to compile the Mod AkimboAdmin.

5. **Resolve Any Issues:**
   - If you encounter any missing reference errors, double-check that all the required DLLs are present and correctly referenced in the project.

## Output Location
After running the build commands, the compiled files will be located in: `../dist/ModAkimboAdmin/`

## Conclusion
Once you have added the necessary DLLs and built the source, you can proceed to implement and test your modifications. If you have any questions or need further assistance, feel free to check the project's documentation or reach out to the community.
