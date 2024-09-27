import TableComponent from "../elements/TableComponent";
class AkimboBlueprintPanelTab {
	constructor(tabPane, that) {
		// Initialize player teleport panels
		this.createBlueprintPanelTab(tabPane, that);
	}

	// Create the Player Teleport panel
	createBlueprintPanelTab(tabPane, that) {
		const title = createElement(tabPane, "h6", ["panel_title"]);
		title.innerText = "Blueprint management";

		let blueprintManagementWrapper = createElement(tabPane, "div", [
			"container-full",
		]);

		let blueprintUploadBody = createElement(
			blueprintManagementWrapper,
			"div",
			["row-flex-center"],
		);

		// Input field to search for players
		let bpInputField = createElement(blueprintUploadBody, "input", [
			"player_search",
		]);
		bpInputField.type = "text";
		bpInputField.placeholder = "Import blueprint";
        
        // Checkbox for Single Use
		let magicUseWrapper = createElement(blueprintUploadBody, "div", [
			"checkbox-wrapper",
		]);
		let magicUseCheckbox = createElement(magicUseWrapper, "input", []);
		magicUseCheckbox.type = "checkbox";
		magicUseCheckbox.id = "magicUse";
		let magicUseLabel = createElement(magicUseWrapper, "label", []);
		magicUseLabel.innerText = "Make Magic";
		magicUseLabel.htmlFor = "magicUse";

		// Button to trigger the search action
		let bpAddButton = createElement(blueprintUploadBody, "button", [
			"dump-button",
		]);
		bpAddButton.innerText = "Import blueprint from URL";
		bpAddButton.addEventListener("click", () => {
			const searchstr = bpInputField.value.trim();
            const magicUse = magicUseCheckbox.checked
			if (searchstr) {
				that.logData(`Adding blueprint: ${searchstr}`);
				that.adminModInteraction.ImportBlueprint(searchstr,magicUse,that.selectedPlayer); // Placeholder for future function
			} else {
				that.logData("Please enter a URL to upload a blueprint.");
			}
		});

		let blueprintManagementBody = createElement(
			blueprintManagementWrapper,
			"div",
			["row-flex-center"],
		);

		// // Input field to search for players
		// let bpSearchField = createElement(blueprintManagementBody, "input", [
		// 	"player_search",
		// ]);
		// bpSearchField.type = "text";
		// bpSearchField.placeholder = "Search a blueprint";
		// //that.HTMLNodes.newPlayerInput = inputField;

		// // Button to trigger the search action
		// let bpSearchButton = createElement(blueprintManagementBody, "button", [
		// 	"dump-button",
		// ]);
		// bpSearchButton.innerText = "Search blueprint in BO";
		// bpSearchButton.addEventListener("click", () => {
		// 	const searchstr = that.HTMLNodes.bpSearchField.value.trim();
		// 	if (searchstr) {
		// 		that.logData(`Searching blueprint: ${searchstr}`);
		// 		//that.adminModInteraction.searchForPlayer(searchstr); // that function should trigger the search and setPlayers
		// 	} else {
		// 		that.logData("Please enter a name to search for a blueprint.");
		// 	}
		// });

		const tableColumns = [
			"Id",
			"Name",
			// "Created at",
			"Creator Id",
			"Creator Name",
			"free to deploy",
			// "max use",
			"Actions",
		];

		const actionCallbacks = {
			magic: (rowData, rowIndex) => {
				that.logData(`Edit action for row ${rowIndex} , data: ${JSON.stringify(rowData)}`);
				that.adminModInteraction.toggleMagicBlueprint(rowData.Id,!rowData['free to deploy']);
			},
			inventory: (rowData, rowIndex) => {
				that.logData(`Edit action for row ${rowIndex} , data: ${JSON.stringify(rowData)}`);
				that.adminModInteraction.giveBlueprint(rowData.Id,null);
			},
			player: (rowData, rowIndex) => {
				that.logData(`Edit action for row ${rowIndex} , data: ${JSON.stringify(rowData)}`);
				that.adminModInteraction.giveBlueprint(rowData.Id,that.selectedPlayer);
			},
			delete: (rowData, rowIndex) => {
				that.logData(`Edit action for row ${rowIndex} , data: ${JSON.stringify(rowData)}`);
				that.adminModInteraction.deleteBlueprint(rowData.Id);
			},
		};

		const tableData = [
			{
				Id: 1,
				name: "Project Alpha",
				"Created at": "2023-09-21",
				"creator Id": 2,
				"creator Name": "aphelia",
				"free to deploy": true,
				"max use": 5,
			},
			{
				Id: 2,
				name: "Project Beta",
				"Created at": "2023-09-18",
				"creator Id": 2,
				"creator Name": "aphelia",
				"free to deploy": false,
				"max use": 10,
			},
			{
				Id: 3,
				name: "Project Gamma",
				"Created at": "2023-09-15",
				"creator Id": 2,
				"creator Name": "aphelia",
				"free to deploy": true,
				"max use": 3,
			},
			// Add more rows as needed
		];
		this.table = new TableComponent(
			blueprintManagementWrapper,
			tableColumns,
			tableData,
			actionCallbacks, // Pass the action callbacks here
		);
	}
}

export default AkimboBlueprintPanelTab;
