window.animationInterop = {
    applyAnimation: function(elementSelector, animationClass) {
        const elements = document.querySelectorAll(elementSelector);
        if (elements.length > 0) {
            const element = elements[elements.length - 1];
            element.classList.add(animationClass);
            setTimeout(() => element.classList.remove(animationClass), 1000);
            return true;
        }
        return false;
    },
    
    injectStyle: function(css) {
        const style = document.createElement('style');
        style.textContent = css;
        style.setAttribute('data-modern-router-animation', 'true');
        document.head.appendChild(style);
        setTimeout(() => style.remove(), 2000); // Give more time for animations
        return true;
    }
};