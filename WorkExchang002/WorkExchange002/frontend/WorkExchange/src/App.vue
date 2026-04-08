<script setup>
import { ref, computed, provide, onMounted } from 'vue'
import { RouterView } from 'vue-router'
import ChatFab from './components/ChatFab.vue'
import TheHeader from './components/TheHeader.vue'
import TheFooter from './components/TheFooter.vue'
import AiChat from './components/Baleen/AiChat.vue'
import { BOrchestrator } from 'bootstrap-vue-next'
import router from './router'
import axios from 'axios'

axios.defaults.withCredentials = true
// ── 全域使用者狀態 ──
const token = ref(localStorage.getItem('token') || null)
const avatar = ref('/images/default-avatar.png')
const isLogin = ref(false)
const headerRef = ref(null) // 新增這行，用來拿到 Header 的 ref
// 登入成功處理（給 LoginRegister 或其他登入元件使用）
const handleLoginSuccess = (newToken, userAvatar = null) => {
  token.value = newToken
  localStorage.setItem('token', newToken)
  if (userAvatar) {
    avatar.value = userAvatar
  } else {
    // 如果後端沒給 avatar，可在此發 API 抓取
    // fetchAvatar()
  }
}

// 登出處理（給會員中心或其他地方呼叫）
const handleLogout = () => {
  token.value = null
  avatar.value = '/images/default-avatar.png'
  localStorage.removeItem('token')
  router.push('/') // 建議跳回首頁，避免留在會員頁
}

// 提供給全域使用（Header、會員中心 Sidebar、LoginRegister 等）
provide('userToken', token)
provide('userAvatar', avatar)
provide('isLogin', isLogin)
provide('onLoginSuccess', handleLoginSuccess)
provide('onLogout', handleLogout)
provide('headerRef', headerRef) // 新增這行
// 可選：頁面載入時，如果有 token 就自動抓 avatar
onMounted(() => {
  if (token.value) {
    // fetchAvatar() // 如果你有這個函式，可以在這裡呼叫
  }
})
</script>

<template>
  <TheHeader ref="headerRef" v-if="!$route.path.startsWith('/host/')" />
  <main class="main-content" :class="{ 'has-header': !$route.path.startsWith('/host') }">
    <RouterView />
  </main>
  <TheFooter v-if="!$route.path.startsWith('/host/')" />
  <ChatFab />
  <AiChat class="chat-float" />
  <BOrchestrator />
</template>

<style scoped>
.has-header {
  padding-top: 90px;
}

.chat-float {
  position: fixed;
  right: 20px;
  bottom: 130px;
  z-index: 9999;
}

body:has([class*="app-container"]) {
  padding-top: 0 !important;
}

body:has([class*="app-container"]) {
  padding-top: 0 !important;
}
</style>
