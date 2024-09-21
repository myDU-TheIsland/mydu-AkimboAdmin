class PlayerTeleportTab {
	constructor(tabPane, that) {
		// Initialize player teleport panels
		this.createPlayerTeleportPanel(tabPane, that);
	}

	// Create the Player Teleport panel
	createPlayerTeleportPanel(tabPane, that) {
		that.HTMLNodes.player_TP_Title = createElement(tabPane, "h5", [
			"panel_title",
		]);
		that.HTMLNodes.player_TP_Title.innerText = "Player Teleport panel";
		let player_tp_body = createElement(tabPane, "div", ["container-center"]);
		let tp_body = createElement(player_tp_body, "div", ["row-flex-center"]);
		// teleport to player button
		let player_tp_btn = (that.HTMLNodes.player_tp_btn = createElement(
			tp_body,
			"button",
			["dump-button"],
		));
		player_tp_btn.innerText = "Teleport To Player";
		that.HTMLNodes.player_tp_btn.addEventListener("click", () => {
			that.logData(`Selected Player: ${that.selectedPlayer}`);

			if (that.selectedPlayer !== "") {
				that.logData(`Teleporting to player: ${that.selectedPlayer}`);
				that.adminModInteraction.teleportToPlayer(that.selectedPlayer);
				setTimeout(() => {
					that._close();
				}, 2000); // 5000 milliseconds = 5 seconds
			} else {
				that.logData("No player selected.");
			}
		});

		// teleport player here button
		let player_fp_btn = (that.HTMLNodes.player_fp_btn = createElement(
			tp_body,
			"button",
			["dump-button"],
		));
		player_fp_btn.innerText = "Teleport Player Here";
		that.HTMLNodes.player_fp_btn.addEventListener("click", () => {
			that.logData(`Selected Player: ${that.selectedPlayer}`);
			if (that.selectedPlayer !== "") {
				that.logData(`Teleporting player here: ${that.selectedPlayer}`);
				that.adminModInteraction.teleportPlayerHere(
					that.selectedPlayer,
				);
				setTimeout(() => {
					that._close();
				}, 2000); // 5000 milliseconds = 5 seconds
			} else {
				that.logData("No player selected.");
			}
		});

		that.HTMLNodes.player_TP_Title = createElement(tabPane, "h5", [
			"panel_title",
		]);
		that.HTMLNodes.player_TP_Title.innerText = "Teleport location panel";
		let button_tp_body = createElement(tabPane, "div", ["container-center"]);
		// Container for teleport buttons
		that.HTMLNodes.teleportButtons = createElement(button_tp_body, "div", [
			"buttons_container",
		]);
		// Create or select the <p> element where combinations will be shown
		const teleportTags2 = JSON.parse(teleportTags); // Example teleport tags
		that.selectedButton = null; // Track the currently selected button

		teleportTags2.forEach((tag) => {
			let button = createElement(
				that.HTMLNodes.teleportButtons,
				"button",
				["teleport_button"],
			);

			// Set the button text to the PublicName
			button.innerText = tag;

			button.addEventListener("click", () => {
				// Remove selected class from previously selected button
				if (that.selectedButton) {
					that.selectedButton.classList.remove("selected");
				}

				// Add selected class to the newly selected button
				button.classList.add("selected");
				that.selectedButton = button;

				// Store the selected tag (the whole object, not just the name)
				that.selectedTag = tag;
			});
		});
		let start_tp_body = createElement(button_tp_body, "div", [
			"row-flex-center",
		]);
		// Confirm button
		let confbtn = (that.HTMLNodes.confirmButton = createElement(
			start_tp_body,
			"button",
			["dump-button"],
		));
		confbtn.innerText = "Teleport To Location";
		that.HTMLNodes.confirmButton.addEventListener("click", () => {
			if (that.selectedTag) {
				// Trigger the teleport action
				that.adminModInteraction.sendTeleportLocation(that.selectedTag);
				that.logData(`Teleporting to ${that.selectedTag}`);
				setTimeout(() => {
					that._close();
				}, 5000); // 5000 milliseconds = 5 seconds
			} else {
				that.logData("No destination selected.");
			}
		});
		that.HTMLNodes.add_tp_location = createElement(tabPane, "h5", [
			"panel_title",
		]);
		that.HTMLNodes.add_tp_location.innerText = "Add a teleport location";
		let location_tp_body = createElement(tabPane, "div", [
			"container-center",
		]);
		let inputLocation_body = createElement(location_tp_body, "div", [
			"row-flex-center",
		]);
		// Input field to add teleport location
		let addTpField = createElement(inputLocation_body, "input", [
			"player_search",
		]);
		addTpField.type = "text";
		addTpField.placeholder = "location name";
		that.HTMLNodes.addTpInput = addTpField;

		// Button to trigger the search action
		let addLocationButton = createElement(inputLocation_body, "button", [
			"dump-button",
		]);
		addLocationButton.innerText = "add";
		addLocationButton.addEventListener("click", () => {
			const location = that.HTMLNodes.addTpInput.value.trim();
			if (location) {
				that.logData(`Adding location: ${location}`);
				that.adminModInteraction.addTeleportDestination(location); // that function should trigger the search and setPlayers

				// Add the new location to the array
				teleportTags2.push(location);

				// Create a new button for the added location
				let button = createElement(
					that.HTMLNodes.teleportButtons,
					"button",
					["teleport_button"],
				);
				button.innerText = location;

				// Add click event to the new button
				button.addEventListener("click", () => {
					if (that.selectedButton) {
						that.selectedButton.classList.remove("selected");
					}

					button.classList.add("selected");
					that.selectedButton = button;

					// Store the selected tag (in that case the new location)
					that.selectedTag = location;
				});

				// Clear the input field after adding the location
				that.HTMLNodes.addTpInput.value = "";
			} else {
				that.logData("Please enter a location name.");
			}
		});
		that.HTMLNodes.player_POS_Title = createElement(tabPane, "h5", [
			"panel_title",
		]);
		that.HTMLNodes.player_POS_Title.innerText = "Teleport To Custom Pos:";

		// Container for custom location teleport
		let pos_body = createElement(tabPane, "div", ["container-center"]);
		let inputpos_body = createElement(pos_body, "div", ["row-flex-center"]);

		// Input field for custom location
		let inputposField = createElement(inputpos_body, "input", [
			"player_search",
		]);
		inputposField.type = "text";
		inputposField.placeholder = "Enter custom POS:";
		that.HTMLNodes.customPos = inputposField;
		// Button to trigger the teleport pos action
		let posButton = createElement(inputpos_body, "button", ["dump-button"]);
		posButton.innerText = "Teleport to POS";
		posButton.addEventListener("click", () => {
			const posstr = that.HTMLNodes.customPos.value.trim();
			if (posstr) {
				that.logData(`Searching for pos: ${posstr}`);
				that.adminModInteraction.teleportToCoordinates(posstr); // that function should trigger the search and setPlayers
			} else {
				that.logData("Please enter a POS to teleport.");
			}
		});
		let playerposbtn = createElement(inputpos_body, "button", [
			"dump-button",
		]);
		playerposbtn.innerText = "Teleport player to POS";
		playerposbtn.addEventListener("click", () => {
			const playerposstr = that.HTMLNodes.customPos.value.trim();
			if (playerposstr) {
				that.logData(`Searching for pos: ${playerposstr}`);
				that.adminModInteraction.teleportPlayerToCoordinates(
					playerposstr,
				); // that function should trigger the search and setPlayers
			} else {
				that.logData("Please enter a player name to search.");
			}
		});
		let teleport_body = createElement(tabPane, "div", ["row-flex-center"]);
	}
}

export default PlayerTeleportTab;
