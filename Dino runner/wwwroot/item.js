// items.js

export const gameState = {
    hasDoubleJump: false,
    speedReduction: 1,
    hasShield: false,
    itemsLoaded: false
};

/**
 * 从 API 加载并直接激活所有已购道具
 */
export async function loadOwnedItems(userId, api) {
    try {
        const userItems = await api(`/api/shop/user/${userId}`);
        const itemIds = userItems.map(i => i.shopItemId);

        // 映射 ID：1-双倍跳跃, 2-减速, 3-护盾
        gameState.hasDoubleJump = itemIds.includes(1);
        gameState.speedReduction = itemIds.includes(2) ? 0.7 : 1;
        gameState.hasShield = itemIds.includes(3);

        gameState.itemsLoaded = true;
        return { success: true };
    } catch (e) {
        console.error("同步失败:", e);
        gameState.itemsLoaded = false;
        return { success: false, error: e.message };
    }
}

/**
 * 消耗护盾
 */
export function consumeShield() {
    if (gameState.hasShield) {
        gameState.hasShield = false;
        return true;
    }
    return false;
}

/**
 * 获取状态文字（可选，如果你还需要在 UI 显示的话）
 */
export function getActiveItemsText() {
    if (!gameState.itemsLoaded) return 'Synchronisation...';
    const list = [];
    if (gameState.hasDoubleJump) list.push('Double Saut');
    if (gameState.speedReduction < 1) list.push('Vitesse Réduite');
    if (gameState.hasShield) list.push('Bouclier');
    return list.length ? list.join(' / ') : 'Aucun objet';
}