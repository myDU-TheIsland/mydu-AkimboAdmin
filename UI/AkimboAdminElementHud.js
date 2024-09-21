import AkimboAdminFunctions from "./classes/functions/AkimboAdminFunctions";
import AkimboAdminModInteractions from "./classes/functions/AkimboAdminModInteractions";
import DropDownMenu from "./classes/elements/DropDownMenu";
class AkimboAdminElementHud extends MousePage {
	constructor() {
		super();
		// Create instances and use them
		this.adminFunctions = new AkimboAdminFunctions();
		this.adminModInteraction = new AkimboAdminModInteractions();
		this._createHTML();
		this.logData = this.adminModInteraction.logData;
		this.showDispensers = false;
		this.showTeleports = false;
		this.wrapperNode.classList.add("hide");
		engine.on("AkimboAdminElementHud.show", this.showIt, this);
		engine.on(
			"AkimboAdminElementHud.setElementInfo",
			this.setElementInfo,
			this,
		);

		// Apply injected CSS if available
		if (typeof injectedCss !== "undefined") {
			this.adminFunctions.applyInjectedCss(injectedCss);
		}
	}

	setElementInfo(info) {
		const cInfo = JSON.parse(info);
		this.HTMLNodes.ConstructId.innerText =
			"Construct Id: " + cInfo.ConstructId;
		this.HTMLNodes.ElementId.innerText = "Element Id: " + cInfo.Id;
		this.cInfo = cInfo;

		// Toggle visibility of teleport-related elements
		const teleportEnabled = !!cInfo.Teleporters; // Check if teleporters exist

		// Show/hide teleport configuration and button bodies based on teleportEnabled flag
		this.HTMLNodes.teleportConfigurationBody.classList.toggle(
			"hide",
			!teleportEnabled,
		);
		this.HTMLNodes.teleportButtonBody.classList.toggle(
			"hide",
			!teleportEnabled,
		);

		// Handle dispensers
		this.badButton.classList.toggle("hide", !cInfo.Dispensers);
		this.brdButton.classList.toggle("hide", !cInfo.Dispensers);

		// Log the teleporters if available
		if (cInfo.Teleporters) {
			this.logData("teleporters enabled");
			this.HTMLNodes.TPD.populateOptions(cInfo.locations);
		}

		// Log dispensers if enabled
		if (cInfo.Dispensers) {
			this.logData("dispensers enabled");
		}

		this.logData(this.cInfo.Id + "," + this.cInfo.ConstructId);
	}

	showIt(iviz) {
		if (iviz) {
			hudManager.toggleEnhancedMouse();
			this.show(true);
		} else {
			this.show(false);
		}
	}

	show(isVisible) {
		super.show(isVisible);
	}

	_onVisibilityChange() {
		super._onVisibilityChange();
		this.wrapperNode.classList.toggle("hide", !this.isVisible);
		if (!!this.isVisible) {
			inputCaptureManager.captureText(true, () => this._close());
		} else {
			inputCaptureManager.captureText(false);
		}
	}

	_close() {
		this.show(false);
		hudManager.toggleEnhancedMouse();
	}

	_createHTML() {
		this.HTMLNodes = {};

		// Create the wrapper for the panel
		this.wrapperNode = createElement(document.body, "div", [
			"AkimboAdminConstructHud_panel",
		]);

		let header = createElement(this.wrapperNode, "div", [
			"AkimboAdminConstructHud_header",
		]);
		this.HTMLNodes.panelTitle = createElement(header, "h5", [
			"panel_title",
		]);
		this.HTMLNodes.panelTitle.innerText = "Akimbo Element HUD";
		this.HTMLNodes.closeIconButton = createElement(header, "button", [
			"closeConstructPanel_button",
		]);
		this.HTMLNodes.closeIconButton.innerText = "Close";
		this.HTMLNodes.closeIconButton.addEventListener("click", () =>
			this._close(),
		);

		let constructheader = createElement(this.wrapperNode, "div", [
			"ConstructRow-flex-center",
		]);
		this.HTMLNodes.ElementId = createElement(constructheader, "h5", [
			"construct-sub-title",
		]);
		this.HTMLNodes.ConstructId = createElement(constructheader, "h5", [
			"construct-sub-title",
		]);
		this.HTMLNodes.OwnerName = createElement(constructheader, "h5", [
			"construct-sub-title",
		]);

		let constructManagementBody = createElement(this.wrapperNode, "div", [
			"row-flex-center",
		]);

		// Repair button
		let repButton = createElement(constructManagementBody, "button", [
			"dump-button",
		]);
		repButton.innerText = "Repair Element";
		repButton.addEventListener("click", () => {
			this.adminModInteraction.repairElement(
				this.cInfo.Id,
				this.cInfo.ConstructId,
			);
		});

		// Dispenser buttons, initially hidden
		this.badButton = createElement(constructManagementBody, "button", [
			"dump-button",
			"hide",
		]);
		this.badButton.innerText = "Bypass dispenser";
		this.badButton.addEventListener("click", () => {
			this.adminModInteraction.BypassDispenser(
				this.cInfo.Id,
				this.cInfo.ConstructId,
			);
		});

		this.brdButton = createElement(constructManagementBody, "button", [
			"dump-button",
			"hide",
		]);
		this.brdButton.innerText = "Reset dispenser";
		this.brdButton.addEventListener("click", () => {
			this.adminModInteraction.ResetDispenser(
				this.cInfo.Id,
				this.cInfo.ConstructId,
			);
		});

		// Teleport configuration body, hidden by default
		this.HTMLNodes.teleportConfigurationBody = createElement(
			this.wrapperNode,
			"div",
			[
				"container-full",
				"hide", // Hidden by default
			],
		);
		this.HTMLNodes.TPD_Title = createElement(
			this.HTMLNodes.teleportConfigurationBody,
			"h5",
			["panel_title"],
		);
		this.HTMLNodes.TPD_Title.innerText = "Teleport Configuration";
		this.HTMLNodes.TPD = new DropDownMenu(
			this.HTMLNodes.teleportConfigurationBody,
			[],
			"Select a TP location",
		);

		// Teleport button body, hidden by default
		this.HTMLNodes.teleportButtonBody = createElement(
			this.wrapperNode,
			"div",
			[
				"ConstructRow-flex-center",
				"hide", // Hidden by default
			],
		);
		this.TPDButton = createElement(
			this.HTMLNodes.teleportButtonBody,
			"button",
			["dump-button"],
		);
		this.TPDButton.innerText = "Set as Teleport Destination";
		this.TPDButton.addEventListener("click", () => {
			// this.logData(this.HTMLNodes.TPD.selectDropdown.innerText);
			var tag = this.HTMLNodes.TPD.selectDropdown.innerText;
			this.logData(tag);
			this.adminModInteraction.configureTeleporter(this.cInfo.Id,this.cInfo.ConstructId,tag,"destination");
		});
		this.TPTButton = createElement(
			this.HTMLNodes.teleportButtonBody,
			"button",
			["dump-button"],
		);
		this.TPTButton.innerText = "Set Target To Teleport";
		this.TPTButton.addEventListener("click", () => {
			// this.logData(this.HTMLNodes.TPD.selectDropdown.innerText);
			var tag = this.HTMLNodes.TPD.selectDropdown.innerText;
			this.logData(tag);
			this.adminModInteraction.configureTeleporter(this.cInfo.Id,this.cInfo.ConstructId,tag,"target");
		});
	}
}

// Instantiate the HUD
let akimboAdminElementHud = new AkimboAdminElementHud();
