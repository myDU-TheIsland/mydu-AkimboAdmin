class AkimboGeneralPanelTab {
	constructor(tabPane, that) {
		// Initialize player teleport panels
		this.createAkimboGeneralPanelTab(tabPane, that);
	}

	// Create the Player Teleport panel
	createAkimboGeneralPanelTab(tabPane, that) {
		const title = createElement(tabPane, "h6", ["panel_title"]);
		title.innerText = "General";
		const info = createElement(tabPane, "p", ["panel_title"]);
		info.innerText = "Interact with player";
		let player_interact_body = createElement(tabPane, "div", [
			"container-center",
		]);
		let input_body = createElement(player_interact_body, "div", [
			"row-flex-center",
		]);

		// Input field to search for players
		let inputField = createElement(input_body, "input", ["player_search"]);
		inputField.type = "text";
		inputField.placeholder = "Search Player";
		that.HTMLNodes.newPlayerInput = inputField;

		// Button to trigger the search action
		let searchButton = createElement(input_body, "button", ["dump-button"]);
		searchButton.innerText = "Search...";
		searchButton.addEventListener("click", () => {
			const searchstr = that.HTMLNodes.newPlayerInput.value.trim();
			if (searchstr) {
				that.logData(`Searching for player: ${searchstr}`);
				that.logData(
					"dropdownlist" + that.HTMLNodes.playerSelectDropdown,
				);
				that.adminModInteraction.searchForPlayer(searchstr); // that function should trigger the search and setPlayers
			} else {
				that.logData("Please enter a player name to search.");
			}
		});

		// Custom select dropdown for displaying search results
		let select_body = createElement(player_interact_body, "div", [
			"custom_select_body",
		]);
		let selectDropdown = createElement(select_body, "div", [
			"custom_dropdown",
		]);
		that.HTMLNodes.playerSelectDropdown = selectDropdown;

		that.HTMLNodes.playerSelectDropdown.innerText = "Select a Player";
		// The dropdown list (hidden by default, shown on click)
		let dropdownList = createElement(select_body, "ul", [
			"akimbo_dropdown_list",
			"hide",
		]);

		that.HTMLNodes.playerSelectList = dropdownList;
		selectDropdown.addEventListener("click", () => {
			dropdownList.classList.toggle("hide");
		});
		let interact_reset_body = createElement(player_interact_body, "div", [
			"row-flex-center",
		]);
		// teleport to player button
		let interact_reset_btn = (that.HTMLNodes.interact_reset_btn =
			createElement(interact_reset_body, "button", ["dump-button"]));
		interact_reset_btn.innerText = "Reset Interaction";
		that.HTMLNodes.interact_reset_btn.addEventListener("click", () => {
			that.selectedPlayer = "";
			that.HTMLNodes.selectedPlayerTitle.innerText =
				"Interacting with player: none";
		});

		let territoryManagementWrapper = createElement(tabPane, "div", [
			"container-center",
		]);

		that.HTMLNodes.TerritoryManagement_Title = createElement(
			territoryManagementWrapper,
			"h5",
			["panel_title"],
		);
		that.HTMLNodes.TerritoryManagement_Title.innerText =
			"Territory Management";
		let territoryManagementBody = createElement(
			territoryManagementWrapper,
			"div",
			["row-flex-center"],
		);
		// Button to trigger the teleport pos action
		let OTButton = createElement(territoryManagementBody, "button", [
			"dump-button",
		]);
		OTButton.innerText = "Take over owned Territory";
		OTButton.addEventListener("click", () => {
			that.logData(`Taking over owned territory`);
			that.adminModInteraction.ClaimOwnedTerritory();
		});
		let UOTButton = createElement(territoryManagementBody, "button", [
			"dump-button",
		]);
		UOTButton.innerText = "Take over unowned Territory";
		UOTButton.addEventListener("click", () => {
			that.logData(`Taking over owned territory`);
			that.adminModInteraction.ClaimUnownedTerritory();
		});
	}
}

export default AkimboGeneralPanelTab;
