<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'

import { useChatStore } from '@/components/api/chatStore.js'
import http from './api/http' // 確保 axios 配置了 withCredentials: true

const open = ref(false)
const router = useRouter()
const route = useRoute()

const chatPath = '/chat'

// 使用者資訊狀態
const userInfo = ref(null)

/**
 * 檢查身份驗證
 * Axios 攔截器會自動處理 Token，我們只需確認 API 是否通暢
 */
async function checkAuth() {
  try {
    const res = await http.get(`/api/users/access`)
    if (res.status === 200) {
      userInfo.value = {
        role: res.data.currentRoles
      }
      return true
    }
    return false
  } catch (err) {
    console.error("身份校驗失敗 (未登入或 Token 過期)")
    userInfo.value = null
    return false
  }
}

// 在 /chat 頁面時隱藏懸浮按鈕
const showChatFloat = computed(() => !route.path.startsWith('/chat'))

// 紅點狀態 --- 使用 Pinia Store
const chatStore = useChatStore()

/**
 * 刷新未讀狀態
 */
const refreshUnreadStatus = async () => {
  // 1. 先確認是否登入，未登入則直接清空紅點
  const isAuth = await checkAuth()
  if (!isAuth) {
    chatStore.setUnread(false)
    return
  }

  try {
    // 2. 使用 http (Axios) 抓取未讀狀態
    // Axios 自動處理 res.json()，結果在 res.data 中
    const res = await http.get(`/api/chat/unread-status`)

    // 3. 更新 Pinia 狀態
    chatStore.setUnread(res.data.hasAnyUnread)
  } catch (err) {
    console.error('紅點更新失敗:', err)
    // 發生錯誤（如 401）時隱藏紅點，避免誤導使用者
    chatStore.setUnread(false)
  }
}

let timer = null

onMounted(() => {
  // 初始化刷新
  refreshUnreadStatus()

  // 每 5 秒自動輪詢一次
  timer = setInterval(refreshUnreadStatus, 5000)

  // 當使用者回到網頁視窗時立即刷新
  window.addEventListener('focus', refreshUnreadStatus)
})

// 路由切換時刷新 (例如從別的頁面跳轉)
watch(
  () => route.fullPath,
  () => {
    refreshUnreadStatus()
  },
)

onUnmounted(() => {
  if (timer) clearInterval(timer)
  window.removeEventListener('focus', refreshUnreadStatus)
})

/**
 * 前往聊天室頁面
 */
async function goChat() {
  open.value = false

  // 按下按鈕時再次檢查登入狀態
  const isLogin = await checkAuth()
  if (!isLogin) {
    alert('請先登入系統以使用聊天室！')
    // 如果你有登入頁，可以在此導向登入頁
    // router.push('/login')
    return
  }
  router.push(chatPath)
}
</script>

<template>
  <transition name="fade">
    <div v-show="showChatFloat" class="chatfab" aria-label="chat-floating-entry">
      <button class="chatfab__btn" type="button" @click="goChat" aria-label="chat">
        <i class="fa-regular fa-comments"></i>
        <span v-if="chatStore.hasUnread" class="chatfab__badge"></span>
      </button>
    </div>
  </transition>
</template>

<style scoped>
/* 整個懸浮元件固定在右下角 */
.chatfab {
  position: fixed;
  right: 20px;
  bottom: 60px;
  z-index: 9999;
}

/* 浮動按鈕 */
.chatfab__btn {
  position: relative;
  width: 56px;
  height: 56px;
  border: none;
  border-radius: 50%;
  cursor: pointer;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
  font-size: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #2f6fed;
  color: #fff;
  transition: transform 0.2s ease;
}

.chatfab__btn:hover {
  transform: scale(1.05);
}

/* 紅點 */
.chatfab__badge {
  position: absolute;
  top: 2px;
  right: 2px;
  width: 14px;
  height: 14px;
  background-color: #ff4d4f;
  border-radius: 50%;
  border: 2px solid white;
}

/* 簡單的進場動畫 */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
