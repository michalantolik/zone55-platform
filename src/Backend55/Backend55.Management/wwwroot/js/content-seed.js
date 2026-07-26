window.learnKitSeedFiles = (() => {
    const handles = new Map();

    return {
        choose: async (fileName, pickerId) => {
            const token = crypto.randomUUID();

            if (!window.showSaveFilePicker) {
                handles.set(token, null);
                return token;
            }

            try {
                const handle = await window.showSaveFilePicker({
                    id: pickerId,
                    suggestedName: fileName,
                    types: [{
                        description: "JSON seed file",
                        accept: { "application/json": [".json"] }
                    }]
                });
                handles.set(token, handle);
                return token;
            } catch (error) {
                if (error?.name === "AbortError") {
                    return null;
                }

                throw error;
            }
        },
        write: async (token, fileName, content) => {
            if (!handles.has(token)) {
                throw new Error("The selected seed file handle is no longer available.");
            }

            const handle = handles.get(token);
            handles.delete(token);
            const blob = new Blob([content], { type: "application/json" });

            if (handle) {
                const writable = await handle.createWritable();
                await writable.write(blob);
                await writable.close();
                return;
            }

            const url = URL.createObjectURL(blob);

            try {
                const link = document.createElement("a");
                link.href = url;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                link.remove();
            } finally {
                URL.revokeObjectURL(url);
            }
        },
        cancel: token => {
            if (token) {
                handles.delete(token);
            }
        }
    };
})();
