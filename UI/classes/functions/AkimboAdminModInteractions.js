export default class AkimboAdminModInteractions {
	disownConstruct(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				2004,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	takeOverConstruct(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				2008,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	repairConstruct(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				2009,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	removeDRMproctection(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				2010,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	repairElement(id, constructId) {
		if (id !== false) {
			this.logData(id + "," + constructId);
			CPPMod.sendModAction(
				"AkimboAdmin",
				2005,
				[],
				JSON.stringify({ id: id, constructId: constructId }),
			);
		}
	}
	BypassDispenser(id, constructId) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				2006,
				[],
				JSON.stringify({ id: id, constructId: constructId }),
			);
		}
	}
	ResetDispenser(id, constructId) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				2007,
				[],
				JSON.stringify({ id: id, constructId: constructId }),
			);
		}
	}
	configureTeleporter(id, constructId, teleportName, type) {
		this.logData(id + " " + constructId + " " + teleportName + " " + type);
		if (teleportName !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				2012,
				[],
				JSON.stringify({
					id: id,
					constructId: constructId,
					teleportName: teleportName,
					type: type,
				}),
			);
		}
	}
	addTeleportDestination(teleportName) {
		if (teleportName !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3001,
				[],
				JSON.stringify({ teleportName }),
			);
		}
	}
	ClaimOwnedTerritory(tag) {
		if (tag !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3002,
				[],
				JSON.stringify({ tag }),
			);
		}
	}
	ClaimUnownedTerritory(tag) {
		if (tag !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3003,
				[],
				JSON.stringify({ tag }),
			);
		}
	}
	searchForPlayer(name) {
		if (name !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3004,
				[],
				JSON.stringify({ name }),
			);
		}
	}
	teleportToPlayer(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3005,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	teleportPlayerHere(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3006,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	teleportToCoordinates(pos) {
		if (pos !== false) {
			//akimboAdminHud.HTMLNodes.customPos.value = "";
			CPPMod.sendModAction(
				"AkimboAdmin",
				3007,
				[],
				JSON.stringify({ pos }),
			);
		}
	}
	teleportPlayerToCoordinates(pos) {
		if (pos !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3008,
				[],
				JSON.stringify({ pos }),
			);
		}
	}
	sendTeleportLocation(tag) {
		if (tag !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3009,
				[],
				JSON.stringify({ tag }),
			);
		}
	}
	fillInventory(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3010,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	fillPlayerInventory(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3011,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	clearInventory(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3012,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	clearPlayerInventory(id) {
		if (id !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3013,
				[],
				JSON.stringify({ id }),
			);
		}
	}
	AddItemToInventory(id, itemId, quantity) {
		if (id !== false && itemId !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3014,
				[],
				JSON.stringify({ id: id, itemId: itemId, quantity: quantity }),
			);
		}
	}
	AddItemToPlayerInventory(id, itemId, quantity) {
		if (id !== false && itemId !== false) {
			CPPMod.sendModAction(
				"AkimboAdmin",
				3015,
				[],
				JSON.stringify({ id: id, itemId: itemId, quantity: quantity }),
			);
		}
	}

	logData(data) {
		if (data !== false) {
			//this._logToDebugPanel(data);
			CPPMod.sendModAction(
				"AkimboAdmin",
				888,
				[],
				JSON.stringify({ data: data }),
			);
		}
	}
}
