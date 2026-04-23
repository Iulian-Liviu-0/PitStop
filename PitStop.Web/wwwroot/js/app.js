window.copyToClipboard = (text) => navigator.clipboard.writeText(text).then(() => true).catch(() => false);
