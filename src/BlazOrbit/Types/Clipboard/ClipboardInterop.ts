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