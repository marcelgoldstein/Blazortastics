function isEmbedded() {
    try {
        var result = window.self !== window.top;
        return result;
    }
    catch (e) {
        return true;
    }
}