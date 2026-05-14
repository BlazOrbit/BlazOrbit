export async function copyText(text: string): Promise<void> {
    if (navigator.clipboard?.writeText) {
        try {
            // Aseguramos que el documento tiene el foco antes de llamar
            window.focus();
            await navigator.clipboard.writeText(text);
            return;
        } catch (err) {
            // To the pokedex
        }
    }
}

/**
 * Writes both text/plain and text/html payloads to the system clipboard so spreadsheet
 * targets (Excel, Sheets, Numbers) consume the HTML envelope while plain editors fall back
 * to the TSV. Requires the async ClipboardItem API — on insecure contexts or older browsers
 * we degrade to writeText with the plain payload.
 */
export async function copyRich(text: string, html: string): Promise<void> {
    if (navigator.clipboard?.write && typeof ClipboardItem !== "undefined") {
        try {
            window.focus();
            const item = new ClipboardItem({
                "text/plain": new Blob([text], { type: "text/plain" }),
                "text/html": new Blob([html], { type: "text/html" })
            });
            await navigator.clipboard.write([item]);
            return;
        } catch (err) {
            // Fall through to plain-text fallback below.
        }
    }

    if (navigator.clipboard?.writeText) {
        try {
            window.focus();
            await navigator.clipboard.writeText(text);
        } catch (err) {
            // Swallow — write failed (no permission, no focus, etc.).
        }
    }
}