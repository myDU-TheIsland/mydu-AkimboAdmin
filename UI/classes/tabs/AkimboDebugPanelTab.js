class AkimboDebugPanelTab {
	constructor(tabPane,that) {
		// Create a container for the debug panel content
		console.log(tabPane,that)
		let debugContainer = createElement(tabPane, "div", ["container-full"]);
		that.HTMLNodes.debugContainer = debugContainer;

		// Add a title for the debug panel
		let debugTitle = createElement(debugContainer, "h2", ["sub-title"]);
		debugTitle.innerText = "Debug Input";
		that.HTMLNodes.debugTitle = debugTitle;

		let codeMirrorContainer = createElement(debugContainer, "div", ["debug-input"]);
		that.HTMLNodes.codeMirrorContainer = codeMirrorContainer;

		// Initialize CodeMirror editor
		let editor = CodeMirror(codeMirrorContainer, {
			lineNumbers: true,
			mode: "javascript",
			theme: "monokai",
			lineWrapping: true,
			autoRefresh: true,
		});
		that.HTMLNodes.editor = editor;

		// Create buttons for running JS, clearing input, and copying input
		this.createButton(debugContainer, "Run JS", ["dump-button"], () => this.runJS(that));
		this.createButton(debugContainer, "Clear", ["dump-button"], () => editor.setValue(""));
		this.createButton(debugContainer, "Copy JS Input", ["dump-button"], () => this.copyToClipboard(editor.getValue()));
        that._logToDebugPanel = function (message) {
			// Get current content
			let currentContent = that.HTMLNodes.debugOutput.getValue();

			// Append new message with a newline
			let updatedContent = currentContent + message + "\n";

			// Set the updated content back to the editor
			that.HTMLNodes.debugOutput.setValue(updatedContent);

			// Optionally, scroll to the bottom if you want to view the latest logs
			that.HTMLNodes.debugOutput.scrollTo(null, that.HTMLNodes.debugOutput.getScrollInfo().height);
		};
		// Debug script section
		this.createScriptSection(debugContainer,that);

		// Debug output section
		this.createDebugOutputSection(debugContainer,that);
        
		// Initialize log function
		that._logToDebugPanel("Debug panel initialized.");
		
	}

	// Helper to create buttons
	createButton(parent, text, classList, callback) {
		let button = createElement(parent, "button", classList);
		button.innerText = text;
		button.addEventListener("click", callback);
		return button;
	}

	// Runs JavaScript code from the CodeMirror editor
	runJS(that) {
		const code = that.HTMLNodes.editor.getValue().trim();
		if (code) {
			try {
				const result = eval(code);
				this.handleResult(result,that);
			} catch (error) {
				that._logToDebugPanel(`Error: ${error.message}`);
			}
		} else {
			that._logToDebugPanel("Please enter JavaScript code to evaluate.");
		}
	}

	// Handle different types of JS evaluation results
	handleResult(result,that) {
		if (typeof result === "object" && result !== null) {
			if (result instanceof Map) {
				that._logToDebugPanel(`Result: ${JSON.stringify(this.mapToObject(result), null, 2)}`);
			} else if (Array.isArray(result)) {
				that._logToDebugPanel(`Result: [\n  ${result.join(",\n  ")}\n]`);
			} else {
				that._logToDebugPanel(`Result: ${JSON.stringify(result, null, 2)}`);
			}
		} else {
			that._logToDebugPanel(`Result: ${result}`);
		}
	}

	// Converts a Map to an object for logging
	mapToObject(map) {
		const obj = {};
		map.forEach((value, key) => {
			obj[key] = value;
		});
		return obj;
	}

	// Logs to the debug panel
	_logToDebugPanel(message) {
		let currentContent = that.HTMLNodes.debugOutput.getValue();
		let updatedContent = currentContent + message + "\n";
		that.HTMLNodes.debugOutput.setValue(updatedContent);
		that.HTMLNodes.debugOutput.scrollTo(null, that.HTMLNodes.debugOutput.getScrollInfo().height);
	}


	// Copy text to clipboard
	copyToClipboard(text) {
		if (navigator.clipboard) {
			navigator.clipboard.writeText(text).then(() => {
				//that._logToDebugPanel("Debug output copied to clipboard.");
			}).catch(err => {
				//that._logToDebugPanel("Failed to copy text: " + err);
			});
		} else {
			const textarea = document.createElement("textarea");
			textarea.value = text;
			document.body.appendChild(textarea);
			textarea.select();
			try {
				document.execCommand("copy");
				//that._logToDebugPanel("Debug output copied to clipboard.",that);
			} catch (err) {
				//that._logToDebugPanel("Failed to copy text: " + err,that);
			}
			textarea.classList.add("hide");
			document.body.removeChild(textarea);
		}
	}

	// Creates the debug script section
	createScriptSection(container,that) {
		let debugScriptTitle = createElement(container, "h2", ["sub-title"]);
		debugScriptTitle.innerText = "Fetch a loaded script";
		that.HTMLNodes.debugScriptTitle = debugScriptTitle;

		let scriptInput = createElement(container, "input", ["script-input"]);
		scriptInput.setAttribute("type", "text");
		scriptInput.setAttribute("placeholder", "Enter script name or URL");
		that.HTMLNodes.scriptInput = scriptInput;

		this.createButton(container, "Dump Script Content", ["dump-button"], () => {
			const scriptName = scriptInput.value.trim();
			if (scriptName) {
				that._logToDebugPanel(`Fetching content for script: ${scriptName}`);
				that.adminFunctions.dumpScriptContentByName(scriptInput, scriptName, that);
			} else {
				that._logToDebugPanel("Please enter a script name or URL.");
			}
		});
	}

	// Creates the debug output section
	createDebugOutputSection(container,that) {
		let debugOuputTitle = createElement(container, "h2", ["sub-title"]);
		debugOuputTitle.innerText = "Debug Output";
		that.HTMLNodes.debugOuputTitle = debugOuputTitle;

		let debugcodeMirrorContainer = createElement(container, "div", ["debug-input"]);
		that.HTMLNodes.debugcodeMirrorContainer = debugcodeMirrorContainer;

		let debugOutput = CodeMirror(debugcodeMirrorContainer, {
			lineNumbers: true,
			mode: "javascript",
			theme: "monokai",
			lineWrapping: true,
			autoRefresh: true,
		});
		that.HTMLNodes.debugOutput = debugOutput;

		// Clear and copy debug output buttons
		this.createButton(container, "Clear Debug Output", ["dump-button"], () => debugOutput.setValue(""));
		this.createButton(container, "Copy Debug Output", ["dump-button"], () => this.copyToClipboard(debugOutput.getValue()));
	}
}

export default AkimboDebugPanelTab;
