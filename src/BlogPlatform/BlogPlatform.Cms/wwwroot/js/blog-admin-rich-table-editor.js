let selectedTableCell = {
    row: 0,
    column: 0
};

function createEmptyTableRows(rowCount, columnCount) {
    return Array.from({ length: rowCount }, (_, rowIndex) =>
        Array.from({ length: columnCount }, (_, columnIndex) => ({
            text: rowIndex === 0 ? `Header ${columnIndex + 1}` : "",
            emoji: "",
            imageUrl: "",
            imageAlt: "",
            horizontalAlignment: "",
            verticalAlignment: ""
        }))
    );
}

function normalizeTableCell(cell) {
    if (typeof cell === "string") {
        return {
            text: cell,
            emoji: "",
            imageUrl: "",
            imageAlt: "",
            horizontalAlignment: "",
            verticalAlignment: ""
        };
    }

    return {
        text: cell?.text || "",
        emoji: cell?.emoji || "",
        imageUrl: cell?.imageUrl || "",
        imageAlt: cell?.imageAlt || "",
        horizontalAlignment: cell?.horizontalAlignment || "",
        verticalAlignment: cell?.verticalAlignment || ""
    };
}

function normalizeTableRows(block) {
    block.rows = (block.rows?.length ? block.rows : createEmptyTableRows(3, 3))
        .map(row => row.map(normalizeTableCell));

    if (selectedTableCell.row >= block.rows.length) {
        selectedTableCell.row = block.rows.length - 1;
    }

    if (selectedTableCell.column >= block.rows[0].length) {
        selectedTableCell.column = block.rows[0].length - 1;
    }
}

function renderTableEditor(block) {
    normalizeTableRows(block);

    const host = document.getElementById("tableEditorHost");

    host.innerHTML = `
        <div class="table-block-modal-layout">

            <div class="table-block-topbar">

                <div class="table-block-options">

                    <label class="table-checkbox">
                        <input type="checkbox"
                               id="tableHasHeaderRow"
                               ${block.hasHeaderRow ? "checked" : ""}>
                        Header row
                    </label>

                    <label class="table-checkbox">
                        <input type="checkbox"
                               id="tableHasHeaderColumn"
                               ${block.hasHeaderColumn ? "checked" : ""}>
                        Header column
                    </label>

                    <label class="table-checkbox">
                        <input type="checkbox"
                               id="tableAutoNumberRows"
                               ${block.autoNumberRows ? "checked" : ""}>
                        Auto-number rows
                    </label>

                </div>

                <div class="table-block-toolbar">

                    <button type="button" class="table-action-btn" onclick="addTableRow()">+ Row</button>
                    <button type="button" class="table-action-btn" onclick="addTableColumn()">+ Column</button>
                    <button type="button" class="table-action-btn danger" onclick="removeTableRow()">- Row</button>
                    <button type="button" class="table-action-btn danger" onclick="removeTableColumn()">- Column</button>

                </div>

            </div>

            <div class="table-block-content">

                <div class="table-preview-panel">
                    ${renderTablePreview(block)}
                </div>

                <div class="table-cell-editor-panel">
                    ${renderSelectedCellEditor(block)}
                </div>

            </div>

        </div>
    `;
}

function renderTablePreview(block) {
    return `
        <table class="table-preview-grid">
            <tbody>
                ${block.rows.map((row, rowIndex) => `
                    <tr>
                        ${row.map((cell, columnIndex) => {
        const isSelected =
            selectedTableCell.row === rowIndex &&
            selectedTableCell.column === columnIndex;

        return `
                                <td class="table-preview-cell ${isSelected ? "selected" : ""}"
                                    onclick="selectTableCell(${rowIndex}, ${columnIndex})">
                                    ${renderPreviewCellContent(cell)}
                                </td>
                            `;
    }).join("")}
                    </tr>
                `).join("")}
            </tbody>
        </table>
    `;
}

function renderPreviewCellContent(cell) {
    return `
        <div class="table-preview-cell-content">

            ${cell.emoji
            ? `<div class="table-preview-emoji">${escapeHtml(cell.emoji)}</div>`
            : ""}

            ${cell.imageUrl
            ? `<img class="table-preview-image" src="${escapeHtml(cell.imageUrl)}" alt="${escapeHtml(cell.imageAlt || "")}" />`
            : ""}

            <div class="table-preview-text">
                ${escapeHtml(cell.text || "Empty")}
            </div>

        </div>
    `;
}

function renderSelectedCellEditor(block) {
    const cell = block.rows[selectedTableCell.row][selectedTableCell.column];

    return `
        <div class="table-cell-editor">

            <div class="table-cell-title">
                Editing Cell R${selectedTableCell.row + 1} · C${selectedTableCell.column + 1}
            </div>

            <div class="table-cell-toolbar">

                <button type="button" class="table-action-btn" onclick="pickEmojiForSelectedCell()">
                    Emoji
                </button>

                <button type="button" class="table-action-btn" onclick="uploadImageForSelectedCell()">
                    Image file
                </button>

                <button type="button" class="table-action-btn" onclick="setImageUrlForSelectedCell()">
                    Image URL
                </button>

                <button type="button" class="table-action-btn danger" onclick="clearSelectedCellMedia()">
                    Clear media
                </button>

            </div>

            ${cell.imageUrl
            ? `<img class="table-editor-image-preview" src="${escapeHtml(cell.imageUrl)}" alt="${escapeHtml(cell.imageAlt || "")}" />`
            : ""}

            <textarea id="tableCellText"
                      class="table-editor-textarea"
                      placeholder="Cell content..."
                      oninput="updateSelectedCellText(this.value)">${escapeHtml(cell.text || "")}</textarea>

            <div class="table-alignments">

                <div class="table-alignment-group">
                    <label>Horizontal</label>
                    <select id="tableCellHorizontalAlignment"
                            onchange="updateSelectedCellHorizontalAlignment(this.value)">
                        ${renderAlignmentOptions(cell.horizontalAlignment || "", ["left", "center", "right"])}
                    </select>
                </div>

                <div class="table-alignment-group">
                    <label>Vertical</label>
                    <select id="tableCellVerticalAlignment"
                            onchange="updateSelectedCellVerticalAlignment(this.value)">
                        ${renderAlignmentOptions(cell.verticalAlignment || "", ["top", "middle", "bottom"])}
                    </select>
                </div>

            </div>

        </div>
    `;
}

function renderAlignmentOptions(selected, values) {
    return `
        <option value="">Default</option>
        ${values.map(value => `
            <option value="${value}" ${selected === value ? "selected" : ""}>
                ${value}
            </option>
        `).join("")}
    `;
}

function selectTableCell(row, column) {
    selectedTableCell = {
        row,
        column
    };

    renderTableEditor(bodyBlocks[editedBlockIndex]);
}

function updateSelectedCellText(value) {
    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].text = value;

    refreshOnlyTablePreview(block);
}

function updateSelectedCellHorizontalAlignment(value) {
    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].horizontalAlignment = value;
}

function updateSelectedCellVerticalAlignment(value) {
    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].verticalAlignment = value;
}

function refreshOnlyTablePreview(block) {
    const previewPanel = document.querySelector(".table-preview-panel");

    if (!previewPanel) {
        return;
    }

    previewPanel.innerHTML = renderTablePreview(block);
}

function pickEmojiForSelectedCell() {
    const emoji = prompt("Add emoji:", "📌");

    if (!emoji) {
        return;
    }

    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].emoji = emoji;

    renderTableEditor(block);
}

function setImageUrlForSelectedCell() {
    const url = prompt("Paste image URL:");

    if (!url) {
        return;
    }

    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].imageUrl = url;
    block.rows[selectedTableCell.row][selectedTableCell.column].imageAlt = "";

    renderTableEditor(block);
}

function uploadImageForSelectedCell() {
    const input = document.createElement("input");

    input.type = "file";
    input.accept = "image/*";

    input.onchange = () => {
        const file = input.files?.[0];

        if (!file) {
            return;
        }

        const reader = new FileReader();

        reader.onload = e => {
            const block = bodyBlocks[editedBlockIndex];

            block.rows[selectedTableCell.row][selectedTableCell.column].imageUrl = e.target.result;
            block.rows[selectedTableCell.row][selectedTableCell.column].imageAlt = file.name;

            renderTableEditor(block);
        };

        reader.readAsDataURL(file);
    };

    input.click();
}

function clearSelectedCellMedia() {
    const block = bodyBlocks[editedBlockIndex];
    const cell = block.rows[selectedTableCell.row][selectedTableCell.column];

    cell.emoji = "";
    cell.imageUrl = "";
    cell.imageAlt = "";

    renderTableEditor(block);
}

function addTableRow() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    const columnCount = block.rows[0].length;

    block.rows.push(
        Array.from({ length: columnCount }, () => normalizeTableCell({}))
    );

    renderTableEditor(block);
}

function addTableColumn() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    block.rows.forEach(row => row.push(normalizeTableCell({})));

    renderTableEditor(block);
}

function removeTableRow() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    if (block.rows.length <= 1) {
        return;
    }

    block.rows.pop();

    renderTableEditor(block);
}

function removeTableColumn() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    if (block.rows[0].length <= 1) {
        return;
    }

    block.rows.forEach(row => row.pop());

    renderTableEditor(block);
}

function readTableEditorRows() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    return block.rows;
}
