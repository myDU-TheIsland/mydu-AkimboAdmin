class AkimboSettingsPanelTab {
	constructor(tabPane, that) {
		// Initialize player teleport panels
		this.createSettingPanelTab(tabPane, that);
	}

	// Create the Player Teleport panel
	createSettingPanelTab(tabPane, that) {
		// Create a container for the settings panel content
		let settingsContainer = createElement(tabPane, "div", ["container-full"]);

		// Add a title for the settings panel
		let settingTitle = createElement(settingsContainer, "h2", ["sub-title"]);
		settingTitle.innerText = "Settings";

		// // Create a checkbox to toggle visibility
		// let toggleContainer = createElement(settingsContainer, "div", ["toggle-container"]);
		// let checkboxLabel = createElement(toggleContainer, "label", []);
		// checkboxLabel.innerText = "Toggle Settings Visibility";

		// let checkbox = createElement(toggleContainer, "input", []);
		// checkbox.setAttribute("type", "checkbox");
		// checkbox.checked = false;  // Default to unchecked, meaning not visible

		// // Create the godmode toggle
		// let godmode_toggle = createElement(tabPane, "kbd");
		// godmode_toggle.setAttribute("data-key", "TOGGLE_GOD_MODE");
		// godmode_toggle.innerHTML = "G"; // Key representation

        // this.createInputKeyCheckbox(settingsContainer, "TOGGLE_GOD_MODE", "Toggle God Mode", "Shift + G");
		// // Function to create additional checkboxes
		// this.createInputKeyCheckbox(settingsContainer, "GOD_MODE_SPEED_UP", "God Mode Speed Up", "Shift + Z");
		// this.createInputKeyCheckbox(settingsContainer, "GOD_MODE_SPEED", "God Mode Speed", "Shift + X");
	}

	// Create a checkbox for input keys
	createInputKeyCheckbox(container, key, labelText, defaultKey) {
		let keyToggleContainer = createElement(container, "div", ["key-toggle-container"]);
		let keyCheckboxLabel = createElement(keyToggleContainer, "label", []);
		keyCheckboxLabel.innerText = labelText;

		let keyCheckbox = createElement(keyToggleContainer, "input", []);
		keyCheckbox.setAttribute("type", "checkbox");
		keyCheckbox.checked = false;  // Default to unchecked

		let keyToggle = createElement(keyToggleContainer, "kbd");
		keyToggle.setAttribute("data-key", key);
		keyToggle.innerHTML = defaultKey; // Key representation

		// Add event listener to handle checkbox changes
		keyCheckbox.addEventListener("change", function() {
			if (keyCheckbox.checked) {
				inputManager.addNodeToNodeInputList(keyToggle);
			} else {
				inputManager.removeNodeFromInputList(keyToggle);
			}
		});
	}
}

export default AkimboSettingsPanelTab;