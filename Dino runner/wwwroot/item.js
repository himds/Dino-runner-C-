// items.js

// 道具状态对象
export const gameState = {
    hasDoubleJump: false,
    speedReduction: 1,
    hasShield: false,
    itemsLoaded: false
};

/**
 * 从 API 加载用户拥有的道具并更新状态
 * @param {number} userId 
 * @param {function} api 这里的 api 是你在 HTML 中定义的 fetch 封装
 */
export async function loadOwnedItems(userId, api) {
    try {
        const userItems = await api(`/api/shop/user/${userId}`);
        const itemIds = userItems.map(i => i.shopItemId);

        // 逻辑硬编码映射：1-双倍跳跃, 2-减速, 3-护盾
        gameState.hasDoubleJump = itemIds.includes(1);
        gameState.speedReduction = itemIds.includes(2) ? 0.7 : 1;
        gameState.hasShield = itemIds.includes(3);
        gameState.itemsLoaded = true;

        return { success: true, state: gameState };
    } catch (e) {
        gameState.hasDoubleJump = false;
        gameState.speedReduction = 1;
        gameState.hasShield = false;
        gameState.itemsLoaded = false;

        console.error("道具同步失败:", e);
        return { success: false, error: e.message };
    }
}

/**
 * 获取当前激活道具的文字描述
 */
export function getActiveItemsText() {
    if (!gameState.itemsLoaded) return '同步失败，默认模式';

    const activeList = [];
    if (gameState.hasDoubleJump) activeList.push('双倍跳跃');
    if (gameState.speedReduction < 1) activeList.push('减速');
    if (gameState.hasShield) activeList.push('护盾');

    return activeList.length ? activeList.join(' / ') : '未购买道具';
}

/**
 * 消耗护盾的触发函数
 */
export function consumeShield() {
    if (gameState.hasShield) {
        gameState.hasShield = false;
        return true;
    }
    return false;
}