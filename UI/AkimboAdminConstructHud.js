/* Specific for all Constructs */
import AkimboAdminFunctions from "./classes/functions/AkimboAdminFunctions";
import AkimboAdminModInteractions from "./classes/functions/AkimboAdminModInteractions";
class AkimboAdminConstructHud extends MousePage {
	constructor() {
		super();
		// Create instances and use them
		this.adminFunctions = new AkimboAdminFunctions();
		this.adminModInteraction = new AkimboAdminModInteractions();
		this._createHTML();
		this.logData = this.adminModInteraction.logData;
		this.wrapperNode.classList.add("hide");
		engine.on("AkimboAdminConstructHud.show", this.showIt, this);
		engine.on(
			"AkimboAdminConstructHud.setConstructInfo",
			this.setConstructInfo,
			this,
		);
		if (typeof injectedCss !== "undefined") {
			this.adminFunctions.applyInjectedCss(injectedCss);
		}
	}
	setConstructInfo(info) {
		var cInfo = JSON.parse(info);
		this.HTMLNodes.ConstructName.innerText = "Construct Name: " + cInfo.Name;
		this.HTMLNodes.ConstructId.innerText = "Construct Id: " + cInfo.Id;
		this.HTMLNodes.OwnerName.innerText = "Construct Owner: "+ cInfo.Owner.name;
		this.cInfo = cInfo;
		this.logData(cInfo);
		this.logData(this.cInfo.Id);
	}
	showIt(iviz) {
		if (iviz) {
			hudManager.toggleEnhancedMouse();
			this.show(true);
		} else this.show(false);
	}
	show(isVisible) {
		super.show(isVisible);
	}
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
		this.HTMLNodes.panelTitle.innerText = "Akimbo Construct HUD";
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
		this.HTMLNodes.ConstructName = createElement(constructheader, "h5", [
			"construct-sub-title",
		]);
		this.HTMLNodes.ConstructId = createElement(constructheader, "h5", [
			"construct-sub-title",
		]);
		this.HTMLNodes.OwnerName = createElement(constructheader, "h5", [
			"construct-sub-title",
		]);
		this.HTMLNodes.ConstructName.innerText =
			"Construct: <Name Come Here Now !>";

		let constructManagementBody = createElement(
			this.wrapperNode,
			"div",
			["row-flex-center"],
		);
		// Button to trigger the teleport pos action
		let abButton = createElement(constructManagementBody, "button", [
			"dump-button",
		]);
		abButton.innerText = "Disown construct";
		abButton.addEventListener("click", () => {
			//that.logData(`Taking over owned territory`);
		    this.adminModInteraction.disownConstruct(this.cInfo.Id);
		});
		let toButton = createElement(constructManagementBody, "button", [
			"dump-button",
		]);
		toButton.innerText = "Take over construct";
		toButton.addEventListener("click", () => {
			// that.logData(`Taking over owned territory`);
			this.adminModInteraction.takeOverConstruct(this.cInfo.Id);
		});
		let reButton = createElement(constructManagementBody, "button", [
			"dump-button",
		]);
		reButton.innerText = "Repair construct";
		reButton.addEventListener("click", () => {
			// that.logData(`Taking over owned territory`);
			this.adminModInteraction.repairConstruct(this.cInfo.Id);
		});
		let drmButton = createElement(constructManagementBody, "button", [
			"dump-button",
		]);
		drmButton.innerText = "Remove DRM protection";
		drmButton.addEventListener("click", () => {
			// that.logData(`Taking over owned territory`);
			this.adminModInteraction.removeDRMproctection(this.cInfo.Id);
		});
	}
}
let akimboAdminConstructHud = new AkimboAdminConstructHud();
