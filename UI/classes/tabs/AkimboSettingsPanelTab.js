class AkimboSettingsPanelTab {
	constructor(tabPane, that) {
		// Initialize player teleport panels
		this.createSettingPanelTab(tabPane, that);
	}

	// Create the Player Teleport panel
	createSettingPanelTab(tabPane, that) {
		// Create a container for the debug panel content
		let settingsContainer = createElement(tabPane, "div", [
			"container-full",
		]);

		// Add a title for the debug panel
		let settingTitle = createElement(settingsContainer, "h2", [
			"sub-title",
		]);
		settingTitle.innerText = "Settings";
	}
}

export default AkimboSettingsPanelTab;
