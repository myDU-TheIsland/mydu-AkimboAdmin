class DropDownMenu {
    constructor(container, options = [], placeholder = "Select an Option") {
        // Create dropdown structure
        this.selectBody = this.createElement(container, "div", ["custom_select_body"]);
        this.selectDropdown = this.createElement(this.selectBody, "div", ["custom_dropdown"]);
        this.selectDropdown.innerText = placeholder; // Custom placeholder

        // Dropdown list (hidden by default)
        this.dropdownList = this.createElement(this.selectBody, "ul", ["akimbo_dropdown_list", "hide"]);
        this.populateOptions(options); // Add options to the list

        // Toggle dropdown visibility on click
        this.selectDropdown.addEventListener("click", () => {
            this.dropdownList.classList.toggle("hide");
        });
    }

    // Method to add options dynamically
    populateOptions(options) {
        this.dropdownList.innerHTML = ""; // Clear any existing items

        options.forEach(option => {
            const listItem = this.createElement(this.dropdownList, "li", ["dropdown_item"]);
            listItem.innerText = option;
            listItem.addEventListener("click", () => {
                this.selectDropdown.innerText = option; // Update selected value
                this.dropdownList.classList.add("hide"); // Hide the dropdown
            });
        });
    }

    // Helper method to create elements
    createElement(parent, tag, classes = []) {
        const element = document.createElement(tag);
        if (classes.length) element.classList.add(...classes);
        parent.appendChild(element);
        return element;
    }
}

export default DropDownMenu;
