export default class AkimboAdminFunctions {
	applyInjectedCss(cssContent) {
		let style = document.createElement("style");
		style.type = "text/css";
		style.innerHTML = cssContent;
		document.head.appendChild(style);
	}

	populatePlayerList(players, that) {
		that.logData("Starting to populate selection menu");
		const dropdownList = that.HTMLNodes.playerSelectList;
		dropdownList.innerHTML = ""; // Clear previous options
		if (players.length > 0) {
			that.logData("Starting to populate selection menu");
			players.forEach((player) => {
				that.logData(
					`Adding option for player: ${player.displayName}, ID: ${player.playerId}`,
				);

				// Create a new list item for the dropdown
				let listItem = document.createElement("li");
				listItem.classList.add("dropdown_item");
				listItem.innerText = player.displayName;
				listItem.dataset.playerId = player.playerId;

				// Add click event to select the player when an item is clicked
				listItem.addEventListener("click", () => {
					that.HTMLNodes.playerSelectDropdown.innerText =
						player.displayName;
					that.HTMLNodes.selectedPlayerTitle.innerText =
						"Interacting with player: " + player.displayName;
					dropdownList.classList.add("hide");
					that.selectedPlayer = player.playerId;
					that.logData(that.selectedPlayer);
					that.logData(
						`Selected player: ${player.displayName} (ID: ${player.playerId})`,
					);
				});

				// Append the list item to the dropdown list
				dropdownList.appendChild(listItem);
			});
			dropdownList.classList.toggle("hide");
			// Log the dropdown content after population
			that.logData(that.HTMLNodes.playerSelectList.innerHTML);
		} else {
			that.logData("No players found in the list.");
		}
	}
	
	outputCSSRules(tabPane, that) {
		// // Assuming tabPane is a reference to the correct tab container
		// const debugOutput = tabPane.querySelector(".debug-output");
		// if (!debugOutput) {
		//   console.error("Debug output element not found.");
		//   return;
		// }
		// let scriptList = "";
		// // Get all script tags in the document
		// const scripts = document.querySelectorAll("script");
		// scripts.forEach((script, index) => {
		//   if (script.src) {
		//     // For external scripts, list the src attribute
		//     scriptList += `Script ${index + 1}: ${script.src}\n`;
		//   } else {
		//     // For inline scripts, indicate as inline
		//     scriptList += `Script ${index + 1}: [Inline Script]\n`;
		//   }
		// });
		// // Update the debug output element with the script list
		// debugOutput.textContent = scriptList;
	}

	dumpScriptContentByName(tabPane, scriptName, that) {
		let jsText = "";

		// Log that the function has started
		that._logToDebugPanel(`Dumping content for script: ${scriptName}\n`);

		// Get all script tags in the document
		const scripts = document.querySelectorAll("script");

		// Log the number of scripts found
		that._logToDebugPanel(
			`Number of script tags found: ${scripts.length}\n`,
		);

		const targetScript = Array.from(scripts).find(
			(script) => script.src === scriptName,
		);

		if (targetScript) {
			that._logToDebugPanel(`Target script found: ${targetScript.src}\n`);
			// For external scripts, fetch the content using fetch API
			fetch(targetScript.src)
				.then((response) => {
					if (!response.ok) {
						that._logToDebugPanel(
							`Network response was not ok: ${response.statusText}`,
						);
					}
					return response.text();
				})
				.then((text) => {
					jsText += `/* ${targetScript.src} */\n\n${text}\n\n`;
					// Update the debug output element with the fetched content
					that._logToDebugPanel(`Content fetched successfully.\n`);
					that._logToDebugPanel(jsText);
				})
				.catch((error) => {
					that._logToDebugPanel(
						`Error fetching script content: ${error.message}\n`,
					);
				});
		} else {
			that._logToDebugPanel("Script not found or is an inline script.\n");
			that._logToDebugPanel("Searching for inline scripts...\n");
			// Check for inline scripts
			const inlineScript = Array.from(scripts).find((script) =>
				script.textContent.includes(scriptName),
			);

			if (inlineScript) {
				that._logToDebugPanel(
					"Inline script found. Displaying content...\n",
				);
				jsText += `/* Inline script */\n\n${inlineScript.textContent}\n\n`;
				that._logToDebugPanel(jsText);
			} else {
				that._logToDebugPanel("Inline script not found.\n");
			}
		}
	}
}
