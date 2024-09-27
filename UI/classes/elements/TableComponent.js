class TableComponent {
    constructor(container, columns = [], data = [], actionCallbacks = {}) {
        this.columns = columns; // Column headers
        this.data = data; // Table data
        this.actionCallbacks = actionCallbacks; // Callback functions for buttons
        // Create table structure
        this.tableContainer = this.createElement(container, "div", [
            "table_container",
        ]);
        this.searchInput = this.createElement(this.tableContainer, "input", [
            "player_search",
        ]);
        this.searchInput.placeholder = "Filter...";

        this.table = this.createElement(this.tableContainer, "table", [
            "custom_table",
        ]);
        this.tableHead = this.createElement(this.table, "thead");
        this.tableBody = this.createElement(this.table, "tbody");

        // Initial setup
        this.renderTableHead();
        this.renderTableBody(this.data);

        // Search functionality
        this.searchInput.addEventListener("input", () => this.handleSearch());

        this.rowsPerPage = 10; // Number of rows to display per page
        this.currentPage = 1; // Current page index
        this.totalPages = Math.ceil(this.data.length / this.rowsPerPage); // Total pages

        // Add pagination container
        this.paginationContainer = this.createElement(
            this.tableContainer,
            "div",
            ["pagination_container"],
        );

        this.renderPaginationControls(); // Render the pagination controls
    }

    // Add the new method here to update data dynamically
    updateTableData(newData) {
        this.data = newData; // Update the data
        this.renderTableBody(this.data); // Re-render the table body with updated data
        this.totalPages = Math.ceil(this.data.length / this.rowsPerPage); // Total pages
        this.renderPaginationControls();
    }

    // Render table header with sortable columns
    renderTableHead() {
        const row = this.createElement(this.tableHead, "tr");
        this.columns.forEach((col, index) => {
            const th = this.createElement(row, "th", ["sortable"]);
            th.innerText = col;
            th.addEventListener("click", () => this.sortByColumn(index)); // Sorting feature
        });
    }

    renderTableBody(data) {
        this.tableBody.innerHTML = ""; // Clear the table body

        // Calculate start and end index based on the current page
        const startIndex = (this.currentPage - 1) * this.rowsPerPage;
        const endIndex = startIndex + this.rowsPerPage;
        const paginatedData = data.slice(startIndex, endIndex); // Slice the data for the current page

        paginatedData.forEach((rowData, rowIndex) => {
            const row = this.createElement(this.tableBody, "tr");

            this.columns.forEach((col) => {
                const cell = this.createElement(row, "td");

                if (col === "Actions") {
                    // Render buttons with callbacks in "Actions" column
                    const magicButton = this.createElement(cell, "button", [
                        "dump-button",
                    ]);
                    magicButton.innerText = "Toggle Magic";
                    magicButton.addEventListener("click", () =>
                        this.handleAction("magic", rowData, rowIndex),
                    );

                    const inventoryButton = this.createElement(cell, "button", [
                        "dump-button",
                    ]);
                    inventoryButton.innerText = "Add to inventory";
                    inventoryButton.addEventListener("click", () =>
                        this.handleAction("inventory", rowData, rowIndex),
                    );
                    const playerButton = this.createElement(cell, "button", [
                        "dump-button",
                    ]);
                    playerButton.innerText = "Give to Player";
                    playerButton.addEventListener("click", () =>
                        this.handleAction("player", rowData, rowIndex),
                    );

                    const deleteButton = this.createElement(cell, "button", [
                        "dump-button",
                    ]);
                    deleteButton.innerText = "Delete blueprint";
                    deleteButton.addEventListener("click", () =>
                        this.handleAction("delete", rowData, rowIndex),
                    );
                } else {
                    cell.innerText = rowData[col] || "-"; // Fill each cell with data
                }
            });
        });
    }

    // Handle action button clicks (edit, delete, etc.)
    handleAction(action, rowData, rowIndex) {
        if (this.actionCallbacks[action]) {
            this.actionCallbacks[action](rowData, rowIndex);
        }
    }

    // Sort the table by column index
    sortByColumn(index) {
        const key = this.columns[index];
        const sortedData = [...this.data].sort((a, b) => {
            if (a[key] < b[key]) return -1;
            if (a[key] > b[key]) return 1;
            return 0;
        });
        this.renderTableBody(sortedData); // Re-render the table with sorted data
    }

    handleSearch() {
        const query = this.searchInput.value.toLowerCase();
        const filteredData = this.data.filter((row) =>
            this.columns.some((col) =>
                String(row[col]).toLowerCase().includes(query),
            ),
        );
        this.totalPages = Math.ceil(filteredData.length / this.rowsPerPage); // Update total pages based on filtered data
        this.currentPage = 1; // Reset to the first page
        this.renderTableBody(filteredData); // Re-render the table with filtered data
        this.renderPaginationControls(); // Re-render pagination controls
    }

    // Helper method to create elements
    createElement(parent, tag, classes = []) {
        const element = document.createElement(tag);
        if (classes.length) element.classList.add(...classes);
        parent.appendChild(element);
        return element;
    }

    // Add pagination controls
    renderPaginationControls() {
        this.paginationContainer.innerHTML = ""; // Clear previous pagination controls

        const prevButton = this.createElement(
            this.paginationContainer,
            "button",
            ["pagination-btn"],
        );
        prevButton.innerText = "Previous";
        prevButton.disabled = this.currentPage === 1;
        prevButton.addEventListener("click", () =>
            this.changePage(this.currentPage - 1),
        );

        for (let i = 1; i <= this.totalPages; i++) {
            const pageButton = this.createElement(
                this.paginationContainer,
                "button",
                ["pagination-btn"],
            );
            pageButton.innerText = i;
            if (i === this.currentPage) {
                pageButton.classList.add("active");
            }
            pageButton.addEventListener("click", () => this.changePage(i));
        }

        const nextButton = this.createElement(
            this.paginationContainer,
            "button",
            ["pagination-btn"],
        );
        nextButton.innerText = "Next";
        nextButton.disabled = this.currentPage === this.totalPages;
        nextButton.addEventListener("click", () =>
            this.changePage(this.currentPage + 1),
        );
    }

    // Method to change page
    changePage(pageNumber) {
        if (pageNumber < 1 || pageNumber > this.totalPages) return;
        this.currentPage = pageNumber;
        this.renderTableBody(this.data); // Re-render the table
        this.renderPaginationControls(); // Update the pagination controls
    }
}

export default TableComponent;
