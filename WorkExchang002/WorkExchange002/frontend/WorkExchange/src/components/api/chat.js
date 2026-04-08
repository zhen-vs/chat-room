import { computed, ref } from 'vue'

//聊天室相關其他API/JS放這裡

//============黑名單=============
//執行封鎖
/**
 * @param {number} roomId targetUserId - 要封鎖的使用者 ID
 * @param {string} CHAT_API_BASE 後端 API 地址
 * @param {string} token 登入憑證
 */
export const blockUserApi = async (targetUserId, apiBase, token) => {
  if (!targetUserId) return false

  if (!confirm('確定要封鎖此用戶嗎？封鎖後將無法接收其訊息。')) return false

  try {
    const response = await fetch(`${apiBase}/api/blacklist/${targetUserId}`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    })

    return response.ok
  } catch (error) {
    console.error('Block user error:', error)
    return false
  }
}

//解封
export const unblockUserApi = async (targetUserId, apiBase, token) => {
  if (!targetUserId) return false

  try {
    const response = await fetch(`${apiBase}/api/blacklist/${targetUserId}`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })

    return response.ok
  } catch (error) {
    console.error('Unblock API error:', error)
    return false
  }
}

// 分正常房間 / 被封鎖房間
export function useChatRooms(roomsRef) {
  const showBlocked = ref(false)

  const activeRooms = computed(() => (roomsRef.value || []).filter((r) => !r.isBlocked))

  const blockedRooms = computed(() => (roomsRef.value || []).filter((r) => r.isBlocked))

  const blockedCount = computed(() => blockedRooms.value.length)

  const toggleBlocked = () => (showBlocked.value = !showBlocked.value)

  return { showBlocked, activeRooms, blockedRooms, blockedCount, toggleBlocked }
}
