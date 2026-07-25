(() => {
    const stuckClass = "top-nav-is-stuck";

    const updateStickyState = () => {
        const sentinel = document.querySelector(".top-nav-sticky-sentinel");
        const topNav = document.querySelector(".top-nav-sticky");

        if (!sentinel || !topNav) {
            document.documentElement.classList.remove(stuckClass);
            return;
        }

        const sentinelTop = sentinel.getBoundingClientRect().top;
        document.documentElement.classList.toggle(stuckClass, sentinelTop < 0);
    };

    window.addEventListener("scroll", updateStickyState, { passive: true });
    window.addEventListener("resize", updateStickyState);

    updateStickyState();
})();
