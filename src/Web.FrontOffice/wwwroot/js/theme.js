// Función para aplicar el tema del tenant usando CSS variables
window.applyTenantTheme = function (primaryColor, secondaryColor, accentColor) {
    document.documentElement.style.setProperty('--tenant-primary-color', primaryColor);
    document.documentElement.style.setProperty('--tenant-secondary-color', secondaryColor);
    document.documentElement.style.setProperty('--tenant-accent-color', accentColor);
    
    // Guardar colores para re-aplicarlos
    window.tenantColors = { primaryColor, secondaryColor, accentColor };
    
    // Aplicar colores
    applyColorsToElements();
    
    // Observar cambios en el DOM para re-aplicar colores
    setupMutationObserver();
    
    console.log('✅ Tema aplicado:', { primaryColor, secondaryColor, accentColor });
}

function applyColorsToElements() {
    if (!window.tenantColors) return;
    
    const { primaryColor, secondaryColor } = window.tenantColors;
    
    // Aplicar color SOLO al footer (NO al sidebar)
    const footer = document.querySelector('.footer');
    if (footer) {
        footer.style.backgroundColor = primaryColor;
        footer.style.background = primaryColor;
    }
    
    // Aplicar color al top del navbar
    const topRow = document.querySelector('.top-row');
    if (topRow) {
        topRow.style.backgroundColor = secondaryColor;
        topRow.style.background = secondaryColor;
    }
}

function setupMutationObserver() {
    // Si ya existe el observer, no crear otro
    if (window.tenantThemeObserver) return;
    
    window.tenantThemeObserver = new MutationObserver(() => {
        applyColorsToElements();
    });
    
    // Observar cambios en el body
    window.tenantThemeObserver.observe(document.body, {
        childList: true,
        subtree: true
    });
}
