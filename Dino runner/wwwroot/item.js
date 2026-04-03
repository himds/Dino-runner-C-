// items.js

export const gameState = {
    hasDoubleJump: false,
    speedReduction: 1,
    hasShield: false,
    itemsLoaded: false
};

/**
 * Charge les objets achetés depuis l’API et active les effets.
 */
export async function loadOwnedItems(userId, api) {
    try {
        const userItems = await api(`/api/shop/user/${userId}`);
        const itemIds = userItems.map(i => i.shopItemId);

        // IDs : 1 double saut, 2 ralentissement, 3 bouclier
        gameState.hasDoubleJump = itemIds.includes(1);
        gameState.speedReduction = itemIds.includes(2) ? 0.7 : 1;
        gameState.hasShield = itemIds.includes(3);

        gameState.itemsLoaded = true;
        return { success: true };
    } catch (e) {
        console.error('Échec de synchronisation :', e);
        gameState.itemsLoaded = false;
        return { success: false, error: e.message };
    }
}

/**
 * Consomme le bouclier (une collision absorbée).
 */
export function consumeShield() {
    if (gameState.hasShield) {
        gameState.hasShield = false;
        return true;
    }
    return false;
}

/**
 * Texte des objets actifs pour l’UI.
 */
export function getActiveItemsText() {
    if (!gameState.itemsLoaded) return 'Synchronisation...';
    const list = [];
    if (gameState.hasDoubleJump) list.push('Double saut');
    if (gameState.speedReduction < 1) list.push('Ralentissement');
    if (gameState.hasShield) list.push('Bouclier');
    return list.length ? list.join(' / ') : 'Aucun objet';
}