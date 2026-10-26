window.enfilade = {
    visible: () => document.visibilityState === "visible",
    followMark: nav => {
        const mark = nav.querySelector(".mark");
        const place = () => {
            const link = nav.querySelector("a.active");
            mark.classList.toggle("placed", Boolean(link));
            if (!link) {
                return;
            }
            mark.style.setProperty("--x", `${link.offsetLeft}px`);
            mark.style.setProperty("--y", `${link.offsetTop}px`);
            mark.style.setProperty("--w", `${link.offsetWidth}px`);
            mark.style.setProperty("--h", `${link.offsetHeight}px`);
            if (nav.scrollWidth > nav.clientWidth) {
                nav.scrollTo({ left: link.offsetLeft - (nav.clientWidth - link.offsetWidth) / 2, behavior: "smooth" });
            }
        };
        place();
        requestAnimationFrame(() => mark.classList.add("moving"));
        new MutationObserver(place).observe(nav, { subtree: true, attributeFilter: ["class"] });
        new ResizeObserver(place).observe(nav);
        nav.addEventListener("transitionend", place);
        document.fonts.ready.then(place);
    },
};

(() => {
    const modal = document.getElementById("components-reconnect-modal");

    const retryWhenVisible = async () => {
        if (document.visibilityState === "visible") {
            await retry();
        }
    };

    const retry = async () => {
        document.removeEventListener("visibilitychange", retryWhenVisible);
        try {
            if (await Blazor.reconnect() || await Blazor.resumeCircuit()) {
                modal.close();
            } else {
                location.reload();
            }
        } catch {
            document.addEventListener("visibilitychange", retryWhenVisible);
        }
    };

    const resume = async () => {
        try {
            if (!await Blazor.resumeCircuit()) {
                location.reload();
            }
        } catch {
            modal.classList.replace("components-reconnect-paused", "components-reconnect-resume-failed");
        }
    };

    modal?.addEventListener("components-reconnect-state-changed", ({ detail }) => {
        if (detail.state === "show") {
            modal.showModal();
        } else if (detail.state === "hide") {
            modal.close();
        } else if (detail.state === "failed") {
            document.addEventListener("visibilitychange", retryWhenVisible);
        } else if (detail.state === "rejected") {
            location.reload();
        }
    });

    modal?.addEventListener("cancel", event => event.preventDefault());

    document.addEventListener("click", ({ target }) => {
        const action = target.closest?.("[data-link]")?.dataset.link;
        if (action === "retry") {
            retry();
        } else if (action === "resume") {
            resume();
        } else if (action === "reload") {
            location.reload();
        }
    });
})();
