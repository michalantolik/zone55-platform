(function () {
    const supportedLanguages = new Set([
        'bash',
        'csharp',
        'css',
        'docker',
        'hcl',
        'html',
        'javascript',
        'json',
        'markup',
        'markdown',
        'powershell',
        'razor',
        'sql',
        'typescript',
        'xml',
        'yaml'
    ]);

    const aliases = {
        cs: 'csharp',
        cshtml: 'razor',
        html: 'markup',
        ps1: 'powershell',
        sh: 'bash',
        terraform: 'hcl',
        tf: 'hcl',
        yml: 'yaml'
    };

    function normalizeLanguage(rawLanguage) {
        const language = (rawLanguage || 'text').toLowerCase().replace('language-', '').trim();
        return aliases[language] || language;
    }

    function getGrammar(language) {
        const normalizedLanguage = normalizeLanguage(language);

        if (!supportedLanguages.has(normalizedLanguage)) {
            return null;
        }

        if (normalizedLanguage === 'html' || normalizedLanguage === 'xml' || normalizedLanguage === 'razor') {
            return Prism.languages.markup;
        }

        return Prism.languages[normalizedLanguage] || null;
    }

    function highlightCodeBlock(codeBlock) {
        if (!window.Prism || codeBlock.dataset.highlighted === 'true') {
            return;
        }

        const languageClass = Array.from(codeBlock.classList)
            .find(className => className.startsWith('language-'));
        const language = normalizeLanguage(languageClass);
        const grammar = getGrammar(language);

        if (!grammar) {
            codeBlock.dataset.highlighted = 'true';
            return;
        }

        codeBlock.querySelectorAll('.line-content').forEach(line => {
            const text = line.textContent || '';
            line.innerHTML = Prism.highlight(text, grammar, language);
        });

        codeBlock.dataset.highlighted = 'true';
    }

    function highlightAllCodeBlocks() {
        if (!window.Prism) {
            return;
        }

        document
            .querySelectorAll('.code-block code[class*="language-"]')
            .forEach(highlightCodeBlock);
    }

    window.blogCodeHighlighting = {
        highlightAll: highlightAllCodeBlocks
    };

    document.addEventListener('DOMContentLoaded', highlightAllCodeBlocks);

    const observer = new MutationObserver(() => {
        window.requestAnimationFrame(highlightAllCodeBlocks);
    });

    observer.observe(document.documentElement, {
        childList: true,
        subtree: true
    });
}());
