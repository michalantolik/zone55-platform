let selectedTableCell = {
    row: 0,
    column: 0
};

const tableStyleOptions = [
    {
        value: "dense-engineering",
        title: "Dense Engineering Table",
        description: "Compact technical table with clear borders, header bar and strong readability."
    },
    {
        value: "minimal-reference",
        title: "Minimal Reference Table",
        description: "Very light reference layout with thin separators and no heavy table frame."
    }
];

function normalizeTableStyle(value) {
    return value === "minimal-reference"
        ? "minimal-reference"
        : "dense-engineering";
}

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

function parseTableRowsValue(rows) {
    if (Array.isArray(rows)) {
        return rows;
    }

    if (typeof rows !== "string") {
        return [];
    }

    const trimmedRows = rows.trim();

    if (!trimmedRows) {
        return [];
    }

    try {
        const parsedRows = JSON.parse(trimmedRows);

        return Array.isArray(parsedRows)
            ? parsedRows
            : [];
    }
    catch {
        return [];
    }
}

function normalizeTableRow(row) {
    if (Array.isArray(row)) {
        return row.length
            ? row.map(normalizeTableCell)
            : [normalizeTableCell({})];
    }

    if (typeof row === "string") {
        return [normalizeTableCell(row)];
    }

    if (Array.isArray(row?.cells)) {
        return row.cells.length
            ? row.cells.map(normalizeTableCell)
            : [normalizeTableCell({})];
    }

    return [normalizeTableCell(row || {})];
}

function normalizeTableRows(block) {
    const sourceRows = parseTableRowsValue(block.rows);

    block.rows = (sourceRows.length ? sourceRows : createEmptyTableRows(3, 3))
        .map(normalizeTableRow);

    if (!block.rows.length || !Array.isArray(block.rows[0]) || !block.rows[0].length) {
        block.rows = createEmptyTableRows(3, 3);
    }

    const firstRowColumnCount = block.rows[0].length;

    block.rows = block.rows.map(row => {
        const normalizedRow = Array.isArray(row) && row.length
            ? row
            : [normalizeTableCell({})];

        while (normalizedRow.length < firstRowColumnCount) {
            normalizedRow.push(normalizeTableCell({}));
        }

        return normalizedRow;
    });

    if (selectedTableCell.row >= block.rows.length) {
        selectedTableCell.row = block.rows.length - 1;
    }

    if (selectedTableCell.column >= block.rows[0].length) {
        selectedTableCell.column = block.rows[0].length - 1;
    }

    selectedTableCell.row = Math.max(0, selectedTableCell.row);
    selectedTableCell.column = Math.max(0, selectedTableCell.column);
}

function escapeHtml(value) {
    return (value || "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

function normalizeMarkdownSource(value) {
    return (value || "")
        .replace(/\r\n/g, "\n")
        .replace(/\r/g, "\n")
        .replace(/\u00a0/g, " ");
}

function plainTextLooksLikeMarkdown(value) {
    const text = value || "";

    return /(^|\n)\s{0,3}#{1,6}\s/.test(text) ||
        /(^|\n)\s*[-*+]\s+/.test(text) ||
        /(^|\n)\s*\d+\.\s+/.test(text) ||
        /\*\*[^*]+\*\*/.test(text) ||
        /__[^_]+__/.test(text) ||
        /`[^`]+`/.test(text) ||
        /\[[^\]]+\]\([^)]+\)/.test(text) ||
        /(^|\n)\s*>/.test(text) ||
        /(^|\n)\s*\|.+\|/.test(text) ||
        /```/.test(text);
}

function clipboardEventToMarkdown(event) {
    const plainText = normalizeMarkdownSource(event.clipboardData?.getData("text/plain") || "");
    const html = event.clipboardData?.getData("text/html") || "";

    if (!html || plainTextLooksLikeMarkdown(plainText)) {
        return plainText;
    }

    const markdown = normalizeMarkdownSource(convertHtmlToMarkdown(html));

    return markdown || plainText;
}

function convertHtmlToMarkdown(html) {
    const documentParser = new DOMParser();
    const document = documentParser.parseFromString(html, "text/html");

    return Array.from(document.body.childNodes)
        .map(node => convertHtmlNodeToMarkdown(node, 0))
        .join("")
        .replace(/\n{3,}/g, "\n\n")
        .trim();
}

function convertHtmlNodeToMarkdown(node, listDepth) {
    if (node.nodeType === Node.TEXT_NODE) {
        return node.textContent || "";
    }

    if (node.nodeType !== Node.ELEMENT_NODE) {
        return "";
    }

    const element = node;
    const tagName = element.tagName.toLowerCase();
    const childrenMarkdown = () =>
        Array.from(element.childNodes)
            .map(child => convertHtmlNodeToMarkdown(child, listDepth))
            .join("");

    switch (tagName) {
        case "h1":
            return `# ${childrenMarkdown().trim()}\n\n`;
        case "h2":
            return `## ${childrenMarkdown().trim()}\n\n`;
        case "h3":
            return `### ${childrenMarkdown().trim()}\n\n`;
        case "h4":
            return `#### ${childrenMarkdown().trim()}\n\n`;
        case "h5":
            return `##### ${childrenMarkdown().trim()}\n\n`;
        case "h6":
            return `###### ${childrenMarkdown().trim()}\n\n`;
        case "strong":
        case "b":
            return `**${childrenMarkdown().trim()}**`;
        case "em":
        case "i":
            return `*${childrenMarkdown().trim()}*`;
        case "code":
            return `\`${element.textContent || ""}\``;
        case "pre":
            return `\n\`\`\`\n${(element.textContent || "").replace(/\n$/, "")}\n\`\`\`\n\n`;
        case "a": {
            const href = element.getAttribute("href");
            const text = childrenMarkdown().trim();

            return href
                ? `[${text}](${href})`
                : text;
        }
        case "br":
            return "\n";
        case "p":
        case "div":
            return `${childrenMarkdown().trim()}\n\n`;
        case "blockquote":
            return childrenMarkdown()
                .trim()
                .split("\n")
                .map(line => `> ${line}`)
                .join("\n") + "\n\n";
        case "ul":
            return Array.from(element.children)
                .map(child => convertHtmlNodeToMarkdown(child, listDepth + 1))
                .join("") + "\n";
        case "ol":
            return Array.from(element.children)
                .map((child, index) => convertHtmlListItemToMarkdown(child, listDepth + 1, index + 1))
                .join("") + "\n";
        case "li":
            return convertHtmlListItemToMarkdown(element, listDepth, null);
        case "table":
            return convertHtmlTableToMarkdown(element);
        case "thead":
        case "tbody":
        case "tr":
        case "th":
        case "td":
            return childrenMarkdown();
        default:
            return childrenMarkdown();
    }
}

function convertHtmlListItemToMarkdown(element, listDepth, orderedNumber) {
    const indent = "  ".repeat(Math.max(0, listDepth - 1));
    const marker = orderedNumber ? `${orderedNumber}.` : "-";
    const content = Array.from(element.childNodes)
        .map(child => convertHtmlNodeToMarkdown(child, listDepth))
        .join("")
        .trim()
        .replace(/\n/g, `\n${indent}  `);

    return `${indent}${marker} ${content}\n`;
}

function convertHtmlTableToMarkdown(tableElement) {
    const rows = Array.from(tableElement.querySelectorAll("tr"))
        .map(row => Array.from(row.children)
            .filter(cell => ["th", "td"].includes(cell.tagName.toLowerCase()))
            .map(cell => convertHtmlNodeToMarkdown(cell, 0)
                .replace(/\n+/g, " ")
                .replace(/\|/g, "\\|")
                .trim()))
        .filter(row => row.length > 0);

    if (!rows.length) {
        return "";
    }

    const columnCount = Math.max(...rows.map(row => row.length));
    const normalizedRows = rows.map(row => [
        ...row,
        ...Array.from({ length: columnCount - row.length }, () => "")
    ]);

    const header = normalizedRows[0];
    const separator = Array.from({ length: columnCount }, () => "---");
    const body = normalizedRows.slice(1);

    return [
        `| ${header.join(" | ")} |`,
        `| ${separator.join(" | ")} |`,
        ...body.map(row => `| ${row.join(" | ")} |`)
    ].join("\n") + "\n\n";
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

                <div class="table-ribbon-group table-style-group">
                    <div class="table-ribbon-title">Article style</div>
                    <div class="table-style-picker">
                        ${tableStyleOptions.map(option => `
                            <label class="table-style-card ${normalizeTableStyle(block.tableStyle) === option.value ? "selected" : ""}">
                                <input type="radio"
                                       name="tableStyle"
                                       value="${option.value}"
                                       ${normalizeTableStyle(block.tableStyle) === option.value ? "checked" : ""}
                                       onchange="updateTableOption('tableStyle', this.value)">
                                <span>
                                    <strong>${option.title}</strong>
                                    <small>${option.description}</small>
                                </span>
                            </label>
                        `).join("")}
                    </div>
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
                        Paste Markdown source directly. Rich copied text is converted to clean Markdown source, then rendered in article view.
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
            <textarea class="table-preview-text table-markdown-source"
                      spellcheck="true"
                      data-row="${rowIndex}"
                      data-column="${columnIndex}"
                      aria-label="Markdown source for row ${rowIndex + 1}, column ${columnIndex + 1}"
                      placeholder="Markdown source..."
                      onfocus="selectTableCell(${rowIndex}, ${columnIndex})"
                      oninput="updateSelectedCellTextFromEditable(this)"
                      onpaste="pasteMarkdownIntoTableCell(event, this)"
                      onkeydown="handleTableCellKeyDown(event)">${escapeHtml(cell.text || "")}</textarea>
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

            <div class="table-side-label-row">
                <label class="table-side-label" for="tableCellText">Markdown source</label>
                <span class="table-side-hint">Paste keeps clean Markdown source</span>
            </div>

            <textarea id="tableCellText"
                      class="table-editor-textarea table-markdown-source"
                      spellcheck="true"
                      placeholder="Paste or write Markdown source..."
                      oninput="updateSelectedCellText(this.value)"
                      onpaste="pasteMarkdownIntoTableCell(event, this)">${escapeHtml(cell.text || "")}</textarea>

            <div class="table-markdown-preview-card">
                <div class="table-side-label-row">
                    <div class="table-side-label">Rendered preview</div>
                    <span class="table-side-hint">Approximate editor preview</span>
                </div>
                <div id="tableCellMarkdownPreview" class="table-markdown-preview">
                    ${renderMarkdownPreview(cell.text || "")}
                </div>
            </div>

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

function renderMarkdownPreview(markdown) {
    let html = escapeHtml(markdown || "");

    html = html.replace(/^###### (.*)$/gm, "<h6>$1</h6>");
    html = html.replace(/^##### (.*)$/gm, "<h5>$1</h5>");
    html = html.replace(/^#### (.*)$/gm, "<h4>$1</h4>");
    html = html.replace(/^### (.*)$/gm, "<h3>$1</h3>");
    html = html.replace(/^## (.*)$/gm, "<h2>$1</h2>");
    html = html.replace(/^# (.*)$/gm, "<h2>$1</h2>");
    html = html.replace(/^&gt; (.*)$/gm, "<blockquote>$1</blockquote>");
    html = html.replace(/`([^`]+)`/g, "<code>$1</code>");
    html = html.replace(/\*\*([^*]+)\*\*/g, "<strong>$1</strong>");
    html = html.replace(/(?<!\*)\*([^*]+)\*(?!\*)/g, "<em>$1</em>");
    html = html.replace(/\[([^\]]+)\]\((https?:\/\/[^)]+|\/[^)]+)\)/g, `<a href="$2" target="_blank" rel="noopener noreferrer">$1</a>`);
    html = html.replace(/\n/g, "<br>");

    return html;
}

function refreshSelectedCellPreview(value) {
    const preview = document.getElementById("tableCellMarkdownPreview");

    if (preview) {
        preview.innerHTML = renderMarkdownPreview(value || "");
    }
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

function updateTableOption(propertyName, value) {
    const block = bodyBlocks[editedBlockIndex];

    if (!block || getBlockType(block) !== "table") {
        return;
    }

    block[propertyName] = propertyName === "tableStyle"
        ? normalizeTableStyle(value)
        : value;

    renderTableEditor(block);
    onEditorChanged();
}

function updateSelectedCellText(value) {
    const block = bodyBlocks[editedBlockIndex];

    block.rows[selectedTableCell.row][selectedTableCell.column].text = value;

    const editable = document.querySelector(`[data-row="${selectedTableCell.row}"][data-column="${selectedTableCell.column}"]`);

    if (editable && editable.value !== value) {
        editable.value = value;
    }

    refreshSelectedCellPreview(value);
}

function updateSelectedCellTextFromEditable(element) {
    const row = Number(element.dataset.row);
    const column = Number(element.dataset.column);
    const block = bodyBlocks[editedBlockIndex];

    selectedTableCell = { row, column };
    block.rows[row][column].text = element.value;

    const textarea = document.getElementById("tableCellText");

    if (textarea && textarea !== element && textarea.value !== element.value) {
        textarea.value = element.value;
    }

    refreshSelectedCellPreview(element.value);
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
            Paste Markdown source directly. Rich copied text is converted to clean Markdown source, then rendered in article view.
        </div>
        ${renderTablePreview(block)}
    `;

    selectTableCell(selectedTableCell.row, selectedTableCell.column);
}

function handleTableCellKeyDown(event) {
    if (event.key === "Enter" && event.ctrlKey) {
        event.preventDefault();
        moveToNextTableCell();
    }
}

function pasteMarkdownIntoTableCell(event, element) {
    event.preventDefault();

    const markdown = clipboardEventToMarkdown(event);
    const selectionStart = element.selectionStart ?? element.value.length;
    const selectionEnd = element.selectionEnd ?? selectionStart;
    const beforeSelection = element.value.substring(0, selectionStart);
    const afterSelection = element.value.substring(selectionEnd);
    const nextValue = `${beforeSelection}${markdown}${afterSelection}`;
    const nextCaretPosition = selectionStart + markdown.length;

    element.value = nextValue;
    element.selectionStart = nextCaretPosition;
    element.selectionEnd = nextCaretPosition;

    if (element.id === "tableCellText") {
        updateSelectedCellText(nextValue);
        return;
    }

    updateSelectedCellTextFromEditable(element);
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
        block.tableStyle = normalizeTableStyle(
            document.querySelector('input[name="tableStyle"]:checked')?.value);
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
