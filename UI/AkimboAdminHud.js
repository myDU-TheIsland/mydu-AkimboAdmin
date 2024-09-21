import AkimboAdminFunctions from "./classes/functions/AkimboAdminFunctions";
import AkimboAdminModInteractions from "./classes/functions/AkimboAdminModInteractions";
import AkimboDebugPanelTab from './classes/tabs/AkimboDebugPanelTab';
import AkimboItemPanelTab from './classes/tabs/AkimboItemPanelTab';
import PlayerTeleportTab from './classes/tabs/PlayerTeleportTab';
import AkimboGeneralPanelTab from './classes/tabs/AkimboGeneralPanelTab';
import AkimboSettingsPanelTab from './classes/tabs/AkimboSettingsPanelTab';
// import './dependencies/jscolor.js'
class AkimboAdminHud extends MousePage {
	constructor() {
		super();
		// Create instances of function classes and use them
		this.adminFunctions = new AkimboAdminFunctions();
		this.adminModInteraction = new AkimboAdminModInteractions();

		// create initial HTML content
		this._createHTML();

		// make the logData function accesible
		this.logData = this.adminModInteraction.logData;

		// hide the initial HTML content for now
		this.wrapperNode.classList.add("hide");
        
        var that = this;
		// add an Icon to the notification bar
		this.notificationIcon = new NotificationIconComponent(
			"icon_repair_unit", //
			"akimboAdmin",
		);
		
		// add an event when clicked on icon to open the main panel
		this.notificationIcon.onClickEvent.subscribe(() => that.show(true));
		hudManager.addNotificationIcon(this.notificationIcon);
		this.notificationIcon.HTMLNodes.icon.innerText = "A";
    
        // assign hooks that can be called from c#
		engine.on("AkimboAdminHud.show", this.showIt, this);
		engine.on("AkimboAdminHud.setPlayers", this.setPlayers, this);

        // inject the css if accessible
		if (typeof injectedCss !== "undefined") {
			this.adminFunctions.applyInjectedCss(injectedCss);
		}
	}

	// callback function that is called when searching for players.
	setPlayers(players) {
		this.logData("received: " + players);
		const templ = JSON.parse(players);
		this.adminFunctions.populatePlayerList(templ.players, this);
	}

	// set visibility of the UI
	showIt(iviz) {
		if (iviz) {
			hudManager.toggleEnhancedMouse();
			this.show(true);
		} else this.show(false);
	}
	show(isVisible) {
		super.show(isVisible);
	}

	// handle visibility changes
	_onVisibilityChange() {
		super._onVisibilityChange();
		this.wrapperNode.classList.toggle("hide", !this.isVisible);
		if (!!this.isVisible) {
			inputCaptureManager.captureText(true, () => this._close());
		}
		if (!this.isVisible) {
			inputCaptureManager.captureText(false);
		}
	}

	// hide the UI on close
	_close() {
		this.show(false);
		hudManager.toggleEnhancedMouse();
	}

	// create initial UI layout
	_createHTML() {
		this.HTMLNodes = {};

		// Create the wrapper for the panel
		this.wrapperNode = createElement(document.body, "div", [
			"AkimboAdminHud_panel",
		]);

        // add a header for titles
		let header = createElement(this.wrapperNode, "div", [
			"AkimboAdminHud_header",
		]);

        // panel name
		this.HTMLNodes.panelTitle = createElement(header, "h5", [
			"panel_title",
		]);
		this.HTMLNodes.panelTitle.innerText = "Akimbo Admin HUD";

        // interacting with player
		this.HTMLNodes.selectedPlayerTitle = createElement(header, "h5", [
			"panel_title",
		]);
		this.HTMLNodes.selectedPlayerTitle.innerText =
			"Interacting with player: none";

		// close button to close the UI 
		this.HTMLNodes.closeIconButton = createElement(header, "button", [
			"close_button",
		]);
		this.HTMLNodes.closeIconButton.innerText = "Close";
		this.HTMLNodes.closeIconButton.addEventListener("click", () =>
			this._close(),
		);

        // create a tab wrapper
		let tabs = createElement(this.wrapperNode, "div", ["tab-nav"]);

		// add tabs to be used
		this.HTMLNodes.tab1 = createElement(tabs, "a", ["tab-link", "active"]);
		this.HTMLNodes.tab1.innerText = "General";
		this.HTMLNodes.tab1.href = "#tab1";
		this.HTMLNodes.tab2 = createElement(tabs, "a", ["tab-link"]);
		this.HTMLNodes.tab2.innerText = "Teleportation";
		this.HTMLNodes.tab2.href = "#tab2";
		this.HTMLNodes.tab3 = createElement(tabs, "a", ["tab-link"]);
		this.HTMLNodes.tab3.innerText = "Items";
		this.HTMLNodes.tab3.href = "#tab3";
		this.HTMLNodes.tab4 = createElement(tabs, "a", ["tab-link"]);
		this.HTMLNodes.tab4.innerText = "Debug";
		this.HTMLNodes.tab4.href = "#tab4";
		this.HTMLNodes.tab5 = createElement(tabs, "a", ["tab-link"]);
		this.HTMLNodes.tab5.innerText = "Settings";
		this.HTMLNodes.tab5.href = "#tab5";
        
        // make a wrapper for tab content
		let tabContent = createElement(this.wrapperNode, "div", [
			"tab-content",
		]);

		// assign each tab to the tab wrapper
		let tabPane1 = createElement(tabContent, "div", ["tab-pane", "active"]);
		tabPane1.id = "tab1";
		let tabPane2 = createElement(tabContent, "div", ["tab-pane"]);
		tabPane2.id = "tab2";
		let tabPane3 = createElement(tabContent, "div", ["tab-pane"]);
		tabPane3.id = "tab3";
		let tabPane4 = createElement(tabContent, "div", ["tab-pane"]);
		tabPane4.id = "tab4";
		let tabPane5 = createElement(tabContent, "div", ["tab-pane"]);
		tabPane5.id = "tab5";
        
        // create each initial tab content
        // Content of Tab 4 --> needs to load first for debugging
		this.DebugPanel = new AkimboDebugPanelTab(tabPane4,this);
		this.GeneralPanel = new AkimboGeneralPanelTab(tabPane1,this);
        this.TeleportPanel = new PlayerTeleportTab(tabPane2,this);
		this.ItemPanel = new AkimboItemPanelTab(tabPane3,this);
        this.SettingPanel = new AkimboSettingsPanelTab(tabPane5,this);

		// Add tab functionality
		this._addTabFunctionality();
	}

	_addTabFunctionality() {
		const tabs = document.querySelectorAll(".tab-link");
		const tabPanes = document.querySelectorAll(".tab-pane");

		tabs.forEach((tab) => {
			tab.addEventListener("click", (event) => {
				event.preventDefault();
				const targetId = tab.getAttribute("href").substring(1);

				// Remove active class from all tabs and tab panes
				tabs.forEach((t) => t.classList.remove("active"));
				tabPanes.forEach((pane) => pane.classList.remove("active"));

				// Add active class to the clicked tab and corresponding pane
				tab.classList.add("active");
				document.getElementById(targetId).classList.add("active");
			});
		});
	}

	dumpData() {
		// this.dumpData(document.baseURI);
		this.adminFunctions.outputCSSRules(tabPane,that);
		if (outputDiv && outputDiv.textContent.trim()) {
			this.dumpData(outputDiv.textContent);
		} else {
			console.log("No CSS data found.");
		}
	}
}
let akimboAdminHud = new AkimboAdminHud();
