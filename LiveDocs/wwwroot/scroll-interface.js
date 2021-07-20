export function getScroll() {
    return {
        X: window.pageXOffset || document.documentElement.scrollLeft || document.body.scrollLeft || 0,
        Y: window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0
    };
}

export function setScroll(left, top) {
    window.scrollTo(left, top);
}