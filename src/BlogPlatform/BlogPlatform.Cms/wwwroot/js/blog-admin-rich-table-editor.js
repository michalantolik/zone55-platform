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
        .map(row => (row?.length ? row : [normalizeTableCell({})]).map(normalizeTableCell));

    if (!block.rows.length) {
        block.rows = createEmptyTableRows(3, 3);
    }

    if (selectedTableCell.row >= block.rows.length) {
        selectedTableCell.row = block.rows.length - 1;
    }

    if (selectedTableCell.column >= block.rows[0].length) {
        selectedTableCell.column = block.rows[0].length - 1;
    }

    selectedTableCell.row = Math.max(0, selectedTableCell.row);
    selectedTableCell.column = Math.max(0, selectedTableCell.column);
}

function renderTableEditor(block) {
    normalizeTableRows(block);

    const host = document.getElementById("tableEditorHost");

    host.innerHTML = `
        <div class="table-block-modal-layout">
            <div class="table-ribbon" aria-label="Table editing toolbar">
                <div class="table-ribbon-group table-ribbon-group-primary">
                    <div class="table-ribbon-title">Table</div>
                    <button type="button" class="table-action-btn" onclick="addTableRowAfterSelected()">+ Row below</button>
                    <button type="button" class="table-action-btn" onclick="addTableColumnAfterSelected()">+ Column right</button>
                    <button type="button" class="table-action-btn danger" onclick="removeSelectedTableRow()">Delete row</button>
                    <button type="button" class="table-action-btn danger" onclick="removeSelectedTableColumn()">Delete column</button>
                </div>

                <div class="table-ribbon-group">
                    <div class="table-ribbon-title">Structure</div>
                    <label class="table-checkbox">
                        <input type="checkbox"
                               id="tableHasHeaderRow"
                               ${block.hasHeaderRow ? "checked" : ""}
                               onchange="updateTableOption('hasHeaderRow', this.checked)">
                        Header row
                    </label>
                    <label class="table-checkbox">
                        <input type="checkbox"
                               id="tableHasHeaderColumn"
                               ${block.hasHeaderColumn ? "checked" : ""}
                               onchange="updateTableOption('hasHeaderColumn', this.checked)">
                        Header column
                    </label>
                    <label class="table-checkbox">
                        <input type="checkbox"
                               id="tableAutoNumberRows"
                               ${block.autoNumberRows ? "checked" : ""}
                               onchange="updateTableOption('autoNumberRows', this.checked)">
                        Auto-number rows
                    </label>
                </div>

                <div class="table-ribbon-group">
                    <div class="table-ribbon-title">Default alignment</div>
                    <div class="table-ribbon-selects">
                        <select id="tableDefaultHorizontalAlignment" onchange="updateTableOption('defaultHorizontalAlignment', this.value)">
                            ${renderAlignmentOptions(block.defaultHorizontalAlignment || "left", ["left", "center", "right"])}
                        </select>
                        <select id="tableDefaultVerticalAlignment" onchange="updateTableOption('defaultVerticalAlignment', this.value)">
                            ${renderAlignmentOptions(block.defaultVerticalAlignment || "middle", ["top", "middle", "bottom"])}
                        </select>
                    </div>
                </div>
            </div>

            <div class="table-editor-workspace">
                <div class="table-editor-paper" role="region" aria-label="Editable table surface">
                    <div class="table-editor-hint">
                        Click any cell and type directly, just like in a document editor. Use Shift + Enter for a new line.
                    </div>
                    ${renderTablePreview(block)}
                </div>

                <aside class="table-cell-editor-panel" aria-label="Selected cell options">
                    ${renderSelectedCellEditor(block)}
                </aside>
            </div>
        </div>
    `;
}

function renderTablePreview(block) {
    return `
        <div class="table-preview-scroll">
            <table class="table-preview-grid">
                <tbody>
                    ${block.rows.map((row, rowIndex) => `
                        <tr>
                            ${row.map((cell, columnIndex) => renderEditableTableCell(block, cell, rowIndex, columnIndex)).join("")}
                        </tr>
                    `).join("")}
                </tbody>
            </table>
        </div>
    `;
}

function renderEditableTableCell(block, cell, rowIndex, columnIndex) {
    const isSelected = selectedTableCell.row === rowIndex && selectedTableCell.column === columnIndex;
    const isHeader = (block.hasHeaderRow && rowIndex === 0) || (block.hasHeaderColumn && columnIndex === 0);
    const numberLabel = block.autoNumberRows && columnIndex === 0 && rowIndex > 0
        ? `<span class="table-row-number">${rowIndex}</span>`
        : "";
    const horizontalAlignment = cell.horizontalAlignment || block.defaultHorizontalAlignment || "left";
    const verticalAlignment = cell.verticalAlignment || block.defaultVerticalAlignment || "middle";

    return `
        <td class="table-preview-cell ${isHeader ? "header" : ""} ${isSelected ? "selected" : ""}"
            style="text-align: ${escapeHtml(horizontalAlignment)}; vertical-align: ${escapeHtml(verticalAlignment)};"
            onclick="selectTableCell(${rowIndex}, ${columnIndex})">
            ${numberLabel}
            ${renderPreviewCellMedia(cell)}
            <div class="table-preview-text"
                 contenteditable="true"
                 spellcheck="true"
                 data-row="${rowIndex}"
                 data-column="${columnIndex}"
                 onfocus="selectTableCell(${rowIndex}, ${columnIndex})"
                 oninput="updateSelectedCellTextFromEditable(this)"
                 onkeydown="handleTableCellKeyDown(event)">${escapeHtml(cell.text || "")}</div>
        </td>
    `;
}

function renderPreviewCellMedia(cell) {
    return `
        ${cell.emoji
            ? `<div class="table-preview-emoji">${escapeHtml(cell.emoji)}</div>`
            : ""}
        ${cell.imageUrl
            ? `<img class="table-preview-image" src="${escapeHtml(cell.imageUrl)}" alt="${escapeHtml(cell.imageAlt || "")}" />`
            : ""}
    `;
}

function renderSelectedCellEditor(block) {
    const cell = block.rows[selectedTableCell.row][selectedTableCell.column];

    return `
        <div class="table-cell-editor">
            <div>
                <div class="table-cell-kicker">Selected cell</div>
                <div class="table-cell-title">Row ${selectedTableCell.row + 1}, Column ${selectedTableCell.column + 1}</div>
            </div>

            <div class="table-cell-toolbar">
                <button type="button" class="table-action-btn compact" onclick="pickEmojiForSelectedCell()">Emoji</button>
                <button type="button" class="table-action-btn compact" onclick="uploadImageForSelectedCell()">Image file</button>
                <button type="button" class="table-action-btn compact" onclick="setImageUrlForSelectedCell()">Image URL</button>
                <button type="button" class="table-action-btn compact danger" onclick="clearSelectedCellMedia()">Clear media</button>
            </div>

            ${cell.imageUrl
            ? `<img class="table-editor-image-preview" src="${escapeHtml(cell.imageUrl)}" alt="${escapeHtml(cell.imageAlt || "")}" />`
            : `<div class="table-empty-media">No media in this cell</div>`}

            <label class="table-side-label" for="tableCellText">Cell text</label>
            <textarea id="tableCellText"
                      class="table-editor-textarea"
                      placeholder="Write table cell content..."
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
            <option value="${value}" ${selected === value ? "selected" : ""}>${value}</option>
        `).join("")}
    `;
}

function selectTableCell(row, column) {
    selectedTableCell = { row, column };

    const block = bodyBlocks[editedBlockIndex];
    normalizeTableRows(block);

    document.querySelectorAll(".table-preview-cell").forEach(cell => cell.classList.remove("selected"));

    const selectedCell = document.querySelector(`[data-row="${row}"][data-column="${column}"]`)?.closest(".table-preview-cell");
    selectedCell?.classList.add("selected");

    const editorPanel = document.querySelector(".table-cell-editor-panel");

    if (editorPanel) {
        editorPanel.innerHTML = renderSelectedCellEditor(block);
    }
}

function updateTableOption(optionName, value) {
    const block = bodyBlocks[editedBlockIndex];

    block[optionName] = value;

    refreshOnlyTablePreview(block);
}

function updateSelectedCellText(value) {
    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].text = value;

    const editable = document.querySelector(`[data-row="${selectedTableCell.row}"][data-column="${selectedTableCell.column}"]`);

    if (editable && editable.innerText !== value) {
        editable.innerText = value;
    }
}

function updateSelectedCellTextFromEditable(element) {
    const row = Number(element.dataset.row);
    const column = Number(element.dataset.column);
    const block = bodyBlocks[editedBlockIndex];

    selectedTableCell = { row, column };
    block.rows[row][column].text = element.innerText;

    const textarea = document.getElementById("tableCellText");

    if (textarea && textarea.value !== element.innerText) {
        textarea.value = element.innerText;
    }
}

function updateSelectedCellHorizontalAlignment(value) {
    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].horizontalAlignment = value;

    refreshOnlyTablePreview(block);
}

function updateSelectedCellVerticalAlignment(value) {
    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].verticalAlignment = value;

    refreshOnlyTablePreview(block);
}

function refreshOnlyTablePreview(block) {
    normalizeTableRows(block);

    const paper = document.querySelector(".table-editor-paper");

    if (!paper) {
        return;
    }

    paper.innerHTML = `
        <div class="table-editor-hint">
            Click any cell and type directly, just like in a document editor. Use Shift + Enter for a new line.
        </div>
        ${renderTablePreview(block)}
    `;

    selectTableCell(selectedTableCell.row, selectedTableCell.column);
}

function handleTableCellKeyDown(event) {
    if (event.key === "Enter" && !event.shiftKey) {
        event.preventDefault();
        moveToNextTableCell();
    }
}

function moveToNextTableCell() {
    const block = bodyBlocks[editedBlockIndex];
    const columnCount = block.rows[0].length;
    let nextRow = selectedTableCell.row;
    let nextColumn = selectedTableCell.column + 1;

    if (nextColumn >= columnCount) {
        nextColumn = 0;
        nextRow += 1;
    }

    if (nextRow >= block.rows.length) {
        addTableRowAfterSelected(false);
        nextRow = block.rows.length - 1;
        nextColumn = 0;
    }

    selectTableCell(nextRow, nextColumn);
    document.querySelector(`[data-row="${nextRow}"][data-column="${nextColumn}"]`)?.focus();
}

function pickEmojiForSelectedCell() {
    const emoji = prompt("Add emoji:", "📌");

    if (!emoji) {
        return;
    }

    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].emoji = emoji;

    refreshOnlyTablePreview(block);
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

function addTableRowAfterSelected(renderAfterAdd = true) {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    const columnCount = block.rows[0].length;
    const newRow = Array.from({ length: columnCount }, () => normalizeTableCell({}));

    block.rows.splice(selectedTableCell.row + 1, 0, newRow);
    selectedTableCell.row += 1;
    selectedTableCell.column = 0;

    if (renderAfterAdd) {
        renderTableEditor(block);
    }
}

function addTableColumnAfterSelected() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    block.rows.forEach(row => row.splice(selectedTableCell.column + 1, 0, normalizeTableCell({})));
    selectedTableCell.column += 1;

    renderTableEditor(block);
}

function removeSelectedTableRow() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    if (block.rows.length <= 1) {
        return;
    }

    block.rows.splice(selectedTableCell.row, 1);
    selectedTableCell.row = Math.min(selectedTableCell.row, block.rows.length - 1);

    renderTableEditor(block);
}

function removeSelectedTableColumn() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    if (block.rows[0].length <= 1) {
        return;
    }

    block.rows.forEach(row => row.splice(selectedTableCell.column, 1));
    selectedTableCell.column = Math.min(selectedTableCell.column, block.rows[0].length - 1);

    renderTableEditor(block);
}

function addTableRow() {
    addTableRowAfterSelected();
}

function addTableColumn() {
    addTableColumnAfterSelected();
}

function removeTableRow() {
    removeSelectedTableRow();
}

function removeTableColumn() {
    removeSelectedTableColumn();
}

function readTableEditorRows() {
    const block = bodyBlocks[editedBlockIndex];

    normalizeTableRows(block);

    return block.rows;
}

(function wireRichTableEditorIntoArticleBlockModal() {
    const originalOpenBlockEditor = window.openBlockEditor;
    const originalCloseBlockEditor = window.closeBlockEditor;
    const originalSaveBlockEditor = window.saveBlockEditor;

    function getBlockEditModalElements() {
        const modal = document.getElementById("blockEditModal");
        const card = modal?.querySelector(".modal");
        const textarea = document.getElementById("blockEditText");
        const label = document.getElementById("blockEditTextLabel");
        let host = document.getElementById("tableEditorHost");

        if (!host && textarea) {
            host = document.createElement("div");
            host.id = "tableEditorHost";
            host.className = "table-editor-host";
            textarea.insertAdjacentElement("afterend", host);
        }

        return { modal, card, textarea, label, host };
    }

    function resetTableModalState() {
        const { card, textarea, label, host } = getBlockEditModalElements();

        card?.classList.remove("table-block-modal");
        textarea?.classList.remove("hidden");
        label?.classList.remove("hidden");

        if (host) {
            host.classList.remove("active");
            host.innerHTML = "";
        }
    }

    window.openBlockEditor = function openBlockEditorWithRichTableSupport(index) {
        const block = window.bodyBlocks?.[index] ?? bodyBlocks?.[index];

        if (!block || getBlockType(block) !== "table") {
            resetTableModalState();
            originalOpenBlockEditor(index);
            return;
        }

        editedBlockIndex = index;
        selectedTableCell = { row: 0, column: 0 };

        const { modal, card, textarea, label, host } = getBlockEditModalElements();

        document.getElementById("blockEditTitle").innerText = `Edit ${getBlockLabel(block)}`;
        document.getElementById("diagramTitleField").classList.remove("active");
        document.getElementById("diagramTitleBarField").classList.remove("active");

        card?.classList.add("table-block-modal");
        textarea?.classList.add("hidden");
        label?.classList.add("hidden");
        host?.classList.add("active");

        if (textarea) {
            textarea.value = "";
        }

        renderTableEditor(block);

        if (modal) {
            modal.style.display = "flex";
        }
    };

    window.closeBlockEditor = function closeBlockEditorWithRichTableSupport() {
        resetTableModalState();
        originalCloseBlockEditor();
    };

    window.saveBlockEditor = function saveBlockEditorWithRichTableSupport() {
        if (editedBlockIndex === null) {
            return;
        }

        const block = bodyBlocks[editedBlockIndex];

        if (!block || getBlockType(block) !== "table") {
            originalSaveBlockEditor();
            return;
        }

        block.rows = readTableEditorRows();
        block.hasHeaderRow = document.getElementById("tableHasHeaderRow")?.checked === true;
        block.hasHeaderColumn = document.getElementById("tableHasHeaderColumn")?.checked === true;
        block.autoNumberRows = document.getElementById("tableAutoNumberRows")?.checked === true;
        block.defaultHorizontalAlignment = document.getElementById("tableDefaultHorizontalAlignment")?.value || "left";
        block.defaultVerticalAlignment = document.getElementById("tableDefaultVerticalAlignment")?.value || "middle";

        delete block.text;
        delete block.diagram;
        delete block.diagramTitle;
        delete block.mermaid;
        delete block.plantUml;
        delete block.plantuml;
        delete block.code;
        delete block.language;
        delete block.fileName;
        delete block.calloutType;
        delete block.kind;

        syncBodyMarkdownFromBlocks();
        renderBodyBlocks();
        window.closeBlockEditor();
        onEditorChanged();
    };
})();
