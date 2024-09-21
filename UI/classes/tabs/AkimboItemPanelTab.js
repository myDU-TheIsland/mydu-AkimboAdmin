class AkimboItemPanelTab {
	constructor(tabPane,that) {
     this.createTabContent(tabPane,that);
	}
	createTabContent(tabPane,that) {
        // Create and append the title
        const title = createElement(tabPane, "h6", ["panel_title"]);
        title.innerText = "Items";

        // Create and append the input field and button for search
        const itemContainer = createElement(tabPane, "div", ["container-center"]);

        // Create the search input and button
        const searchDiv = createElement(itemContainer, "div", ["row-flex-center"]);
        const itemSearchInput = createElement(searchDiv, "input", ["player_search"]);
        itemSearchInput.type = "text";
        itemSearchInput.placeholder = "Search for items...";

        const searchItemButton = createElement(searchDiv, "button", ["dump-button"]);
        searchItemButton.innerText = "Search Item";

        // Create the "Give All Items" and "Give All Items to Player" buttons
        const giveAllDiv = createElement(itemContainer, "div", ["row-flex-center"]);
        const giveAllItemsButton = createElement(giveAllDiv, "button", ["dump-button"]);
        giveAllItemsButton.innerText = "All Items To nanoPack";

        const giveAllItemsToPlayerButton = createElement(giveAllDiv, "button", ["dump-button"]);
        giveAllItemsToPlayerButton.innerText = "Give All Items To Player";

        // Event listeners for the buttons
        giveAllItemsButton.addEventListener("click", () => {
            that.logData(`Adding all items to inventory : ${that.selectedPlayer}`);
            that.adminModInteraction.fillInventory(that.selectedPlayer);
        });

        giveAllItemsToPlayerButton.addEventListener("click", () => {
            that.logData(`Giving all items to Player: ${that.selectedPlayer}`);
            if (that.selectedPlayer !== "") {
                that.adminModInteraction.fillPlayerInventory(that.selectedPlayer);
            } else {
                that.logData("No player selected.");
            }
        });

        // Create the "Clear All" buttons
        const clearAllDiv = createElement(itemContainer, "div", ["row-flex-center"]);
        const clearAllItemsButton = createElement(clearAllDiv, "button", ["dump-button"]);
        clearAllItemsButton.innerText = "Clear nanopack";

        const clearAllItemsToPlayerButton = createElement(clearAllDiv, "button", ["dump-button"]);
        clearAllItemsToPlayerButton.innerText = "Clear player nanopack";

        // Event listeners for the "Clear" buttons
        clearAllItemsButton.addEventListener("click", () => {
            that.logData(`Clearing inventory`);
            that.adminModInteraction.clearInventory(that.selectedPlayer);
        });

        clearAllItemsToPlayerButton.addEventListener("click", () => {
            if (that.selectedPlayer !== "") {
                that.logData(`Clearing Inventory of player: ${that.selectedPlayer}`);
                that.adminModInteraction.clearPlayerInventory(that.selectedPlayer);
            } else {
                that.logData("No player selected.");
            }
        });

        // Create a div to display the search results
        const resultContainer = createElement(tabPane, "div", ["row-flex-center"]);

        // Event listener for searching items
        searchItemButton.addEventListener("click", async () => {
            // Clear previous results
            resultContainer.innerHTML = "";

            // Retrieve item IDs based on the search query
            const typeIds = await itemBank.getTypeIdListByName(itemSearchInput.value);

            // Fetch and display item definitions
            for (const id of typeIds) {
                const itemDefinition = await itemBank.getItemDefinition(id);

                // Skip hidden items
                if (itemDefinition.isHidden || !itemBank.isItem(id)) {
                    continue;
                }

                // Create a div for each item definition
                const itemDiv = createElement(resultContainer, "div", ["item-container"]);

                // Display item name
                const itemTitle = createElement(itemDiv, "h6", ["panel_title"]);
                itemTitle.innerText = itemDefinition.fullName;

                // Display scale if available
                if (itemDefinition.scale) {
                    const itemScale = createElement(itemDiv, "p", ["panel_title"]);
                    itemScale.innerText = `Scale: ${itemDefinition.scale}`;
                }

                const parentTypeId = createElement(itemDiv, "p", ["panel_title"]);
                parentTypeId.innerText = `ParentTypeId: ${itemDefinition.parentTypeId}`;

                // Display item icon if available
                if (itemDefinition.iconFilename) {
                    const iconDiv = createElement(itemDiv, "div", ["item-icon"]);
                    const iconImage = createElement(iconDiv, "img", ["icon-image"]);
                    iconImage.src = `/data/${itemDefinition.iconFilename}`; // Adjust path as needed
                    iconImage.alt = itemDefinition.name;
                    iconImage.style.maxWidth = "100px";
                    iconImage.style.maxHeight = "100px";
                }

                // Create amount input and buttons for adding/giving items
                const amountDiv = createElement(itemDiv, "div", ["row-flex-center"]);
                const amountInput = createElement(amountDiv, "input", ["player_search"]);
                amountInput.type = "number";
                amountInput.min = "1";
                amountInput.placeholder = "Amount";

                const itemBtnDiv = createElement(itemDiv, "div", ["row-flex-center"]);
                const giveButton = createElement(itemBtnDiv, "button", ["dump-button"]);
                giveButton.innerText = "Add to NanoPack";

                // Event listener for giving the item to NanoPack
                giveButton.addEventListener("click", async () => {
                    const amount = parseInt(amountInput.value, 10);
                    that.logData(`Adding Item To inventory: ${itemDefinition.name}, ${amount}`);
                    that.adminModInteraction.AddItemToInventory(
                        that.selectedPlayer,
                        itemDefinition.typeId,
                        itemDefinition.isMaterial ? amount * (1 << 24) : amount,
                    );
                    amountInput.value = "";
                });

                const giveToPlayerButton = createElement(itemBtnDiv, "button", ["dump-button"]);
                giveToPlayerButton.innerText = "Give To Player";

                // Event listener for giving the item to the selected player
                giveToPlayerButton.addEventListener("click", async () => {
                    const amount = parseInt(amountInput.value, 10);
                    if (isNaN(amount) || amount <= 0) {
                        alert("Please enter a valid amount.");
                        return;
                    }
                    if (that.selectedPlayer !== "") {
                        that.logData(`Giving Item To player: ${that.selectedPlayer}, ${itemDefinition.name}, ${amount}`);
                        that.adminModInteraction.AddItemToPlayerInventory(
                            that.selectedPlayer,
                            itemDefinition.typeId,
                            itemDefinition.isMaterial ? amount * (1 << 24) : amount,
                        );
                    } else {
                        that.logData("No player selected.");
                    }
                    amountInput.value = "";
                });
            }
        });
    }
}
export default AkimboItemPanelTab;