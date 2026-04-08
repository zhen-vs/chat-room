<script setup>
import '@/assets/css/chatroom.css'
// 用生命週期在頁面開啟時用fech抓資料
import { ref, onMounted, onBeforeUnmount, computed } from 'vue'
//使用套件-要先安裝：npm i @microsoft/signalr
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
// 引入其他聊天室相關功能 JS
import { useChatStore } from '@/components/api/chatStore.js'
import { blockUserApi, unblockUserApi, useChatRooms } from '@/components/api/chat.js'

// 當前聊天室狀態（初始空陣列 抓到再變）
const currentUser = ref({ id: '', role: '', name: '' }) //真實登入者資訊
const currentRoom = ref(null) //當前選中房間
const currentRoomTitle = ref('') // 房間右側 header 要顯示的對象名字

//----------------------------------------
// 取TOKEN
const getToken = () => {
  return localStorage.getItem('token') || sessionStorage.getItem('token') || null
}

//-------------------------------------------

//正規化
const formatMessage = (raw) => {
  return {
    id: raw.id || raw.Id,
    text: raw.content || raw.Content,
    //優先用後端給的時間，沒有才用現在時間
    time: new Date(raw.createdAt || raw.CreatedAt).toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit',
    }), //用本地時間，時/分2位數

    //判斷消息是否為我發送
    // 確保先取出發送者 ID，再進行比較
    mine: String(raw.senderUserId || raw.SenderUserId) === String(currentUser.value.id),
    //接收後端未讀標記
    isFirstUnread: raw.isFirstUnread || raw.IsFirstUnread || false,
  }
}

// 左側房間列表、右側
// 訊息列表
const rooms = ref([])
const messages = ref([])

//紅點
const chatStore = useChatStore()

function syncUnreadDot() {
  const anyUnread = rooms.value.some((r) => (r.unread ?? 0) > 0)
  chatStore.setUnread(anyUnread)
}

const newMessage = ref('') //輸入框

//SignalR 連線物件
const connection = ref(null)

// --- 端點設定----
//chathub URL
const HUB_URL = 'https://localhost:7268/chathub'

// 聊天室專用：API Base（只影響這一頁）

const CHAT_API_BASE = 'https://localhost:7268' //要合併時改這一行

//====手機版 Panel 切換（Rooms / Chat）====
const isMobile = ref(false)
const mobilePanel = ref('list') // '左' | '右'

function updateIsMobile() {
  isMobile.value = window.matchMedia('(max-width: 991.98px)').matches

  if (!isMobile.value) {
    mobilePanel.value = 'chat'
  } else {
    if (!currentRoom.value) mobilePanel.value = 'list'
  }
}
function goBackToRoomList() {
  mobilePanel.value = 'list'
}
//================================

const API_ME = `${CHAT_API_BASE}/api/chat/me` //查目前登入者是誰
//================================

const API_ROOMS = `${CHAT_API_BASE}/api/chat/rooms`
const API_ROOM_MESSAGES = (roomId) => `${CHAT_API_BASE}/api/chat/rooms/${roomId}/messages`

//連線SignalR / 收推播 /加入房間
//在頁面載入時就先連
async function startSignalR() {
  const token = getToken()
  if (!token) return
  //-----
  connection.value = new HubConnectionBuilder()
    .withUrl(HUB_URL, {
      //每次 SignalR 需要 token 都重新取（避免 token 更新後還用舊的）
      accessTokenFactory: () => getToken() || '',
    })
    .withAutomaticReconnect() // 自動重連
    .configureLogging(LogLevel.Information)
    .build()

  // 收後端推播
  connection.value.on('ReceiveMessage', (msg) => {
    console.log('收到推播:', msg)
    // 防止重複 ID 進入訊息陣列
    const isDuplicate = messages.value.some((m) => m.id === (msg.id || msg.Id))

    // 判斷是否為我發送的
    const isMine = String(msg.senderUserId || msg.SenderUserId) === String(currentUser.value.id)

    // 更新左側房間列表（不論是誰發的都要更新預覽和順序）
    const room = rooms.value.find((r) => String(r.id) === String(msg.roomId || msg.RoomId))
    if (room) {
      room.lastMessagePreview = msg.content || msg.Content // 更新最後一句話

      // 如果不是我發的，且我目前沒在這個房間裡，才增加未讀數
      if (!isMine && currentRoom.value !== String(room.id)) {
        room.unread = (room.unread || 0) + 1 // 增加未讀點
        syncUnreadDot()
      }
    }

    // 如果訊息已經在畫面上了（自己送的），就不要再 push 到對話框
    if (isDuplicate || isMine) return // 這裡 return 只會擋掉右邊對話框的重複顯示
    // 使用正規化工具並推入陣列
    messages.value.push(formatMessage(msg))
  })

  // 啟動連線並加入錯誤捕捉
  try {
    await connection.value.start()
    console.log('SignalR connected:', connection.value.connectionId)
  } catch (err) {
    console.error('SignalR 連線啟動失敗:', err)
  }
}

// 加入房間群組
async function joinRoom(roomId) {
  //防呆：確保連線存在且狀態為已連線
  if (!connection.value || connection.value.state !== 'Connected') {
    console.warn('SignalR 尚未連線，無法加入房間')
    return
  }
  await connection.value.invoke('joinRoom', String(roomId))
}

// ---接口----

//取得目前網頁登入者

async function fetchUserInfo() {
  const token = getToken()

  if (!token) {
    alert('請先登入系統以使用聊天室！')
    return
  }
  //===============裡面等真實登入做完解開=======================
  try {
    const response = await fetch(API_ME, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`, // JWT 核心：帶上這行
        'Content-Type': 'application/json',
      },
    })
    //如果後端回傳未登入
    if (!response.ok) {
      alert('登入已過期，請重新登入！')
      return
    }
    const user = await response.json()

    currentUser.value = {
      id: user.id,
      role: user.role, //後端回傳 'Host' 或 'User'
      name: user.name,
    }
    // 拿到身分再抓聊天列表
    await fetchRooms()
  } catch (error) {
    //連不到API會進這邊
    console.error('API Error:', error)
    alert('系統連線失敗，請檢查網路連線！')
  }
  //===============裡面等真實登入做完解開=======================
}

// // 取得：聊天室列表
const fetchRooms = async () => {
  const token = getToken()
  console.log('發送請求使用的 Token:', token)
  try {
    const response = await fetch(API_ROOMS, {
      headers: { Authorization: `Bearer ${token}` },
    })
    if (!response.ok) {
      console.error('API 回傳狀態:', response.status) // 檢查是 401 還是 500
      alert('抓取聊天室列表失敗!')
      return
    }

    const data = await response.json()
    console.log('Vue 收到房間資料:', data)
    rooms.value = data
    syncUnreadDot()
  } catch (error) {
    console.log(error)
    alert('抓取聊天室列表失敗')
  }
}
const selectRoom = async (r) => {
  currentRoom.value = String(r.id)
  // header 顯示對象名字（讓右邊 header 也一致）
  currentRoomTitle.value =
    currentUser.value.role === 'Host'
      ? r.userName
      : `${r.hostCompanyName ? r.hostCompanyName + ' - ' : ''}${r.hostName}`

  // 加入 SignalR 群組（才能收到這個房間的即時訊息）
  await joinRoom(currentRoom.value)

  // 載入歷史訊息
  await fetchMessages(currentRoom.value)

  // 手機：選房間後切到聊天 panel
  if (isMobile.value) mobilePanel.value = 'chat'

  // 處理未讀訊息邏輯
  if (r.unread > 0) {
    try {
      const token = getToken()
      // 呼叫後端 API 把該房間設為已讀
      const response = await fetch(`${CHAT_API_BASE}/api/chat/rooms/${r.id}/read`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${token}` },
      })
      // 前端視覺立即歸零，紅點瞬間消失
      r.unread = 0
      syncUnreadDot()
      await response.json()
    } catch (error) {
      console.error('更新已讀狀態失敗:', error)
    }
  }
}
//取得某房間歷史訊息
const fetchMessages = async (roomId) => {
  const token = getToken()
  try {
    const response = await fetch(API_ROOM_MESSAGES(roomId), {
      method: 'GET',
      headers: { Authorization: `Bearer ${token}` },
    })

    if (!response.ok) {
      alert('抓取訊息失敗')
      return
    }

    //訊息資料
    const data = await response.json()

    // 交給 formatMessage 處理
    messages.value = data.map((m) => formatMessage(m))
  } catch (error) {
    console.log(error)
    alert('抓取訊息失敗')
  }
}
//送出訊息

const sendMessage = async () => {
  if (!newMessage.value.trim() || !currentRoom.value) return

  const token = getToken()

  const text = newMessage.value.trim() //每次送都重新抓
  // 先把要送的文字存起來，作在清空輸入框前

  try {
    // 用 POST API 把訊息存進資料庫，然後再用 SignalR 把訊息「廣播」（這樣聊天室才有歷史紀錄）
    const response = await fetch(API_ROOM_MESSAGES(currentRoom.value), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },

      body: JSON.stringify({ content: text }),
    })

    //如後端回傳錯誤
    if (!response.ok) {
      alert('送出失敗')
      return
    }

    //成功
    if (response.ok) {
      const savedMessage = await response.json()
      //交給 formatMessage 處理
      messages.value.push(formatMessage(savedMessage))
      // 清空輸入框
      newMessage.value = ''
    }

    //如網路斷線或瀏覽器錯誤
  } catch (error) {
    console.log(error)
    alert('送出失敗，請檢查網路連線')
  }
}

// 生命週期：進頁面先執行身分檢查
onMounted(async () => {
  // 0) 先判斷是否手機 + 監聽縮放
  updateIsMobile()
  window.addEventListener('resize', updateIsMobile)
  // 1) 連 SignalR（先連上，之後選房間再 join）
  await startSignalR()
  // 2) 再抓登入者 + rooms

  await fetchUserInfo()
})

onBeforeUnmount(async () => {
  window.removeEventListener('resize', updateIsMobile)
  if (connection.value) await connection.value.stop()
})

// 封鎖處理
const handleBlock = async () => {
  console.log('11111')
  if (!currentRoom.value) return

  //先從房間清單中找出目前這間房的資料
  const room = rooms.value.find((r) => String(r.id) === currentRoom.value)
  if (!room) return
  //判斷對方User ID
  const targetUserId = currentUser.value.role === 'Host' ? room.userId : room.hostId

  const success = await blockUserApi(targetUserId, CHAT_API_BASE, getToken())
  if (success) {
    alert('已將該用戶加入黑名單')
    room.isBlocked = true
  } else {
    alert('封鎖失敗')
  }
}
//解除封鎖
const handleUnblock = async () => {
  const room = rooms.value.find((r) => String(r.id) === currentRoom.value)
  if (!room) return

  const targetUserId = currentUser.value.role === 'Host' ? room.userId : room.hostId

  const success = await unblockUserApi(targetUserId, CHAT_API_BASE, getToken())

  if (success) {
    alert('已解除封鎖')
    room.isBlocked = false //遮罩消失
  } else {
    alert('解除封鎖失敗，請稍後再試')
  }
}
const currentRoomIsBlocked = computed(() => {
  if (!currentRoom.value) return false
  const room = rooms.value.find((r) => String(r.id) === String(currentRoom.value))
  return !!room?.isBlocked
})
// 封鎖置底/展開/收合
const { showBlocked, activeRooms, blockedRooms, blockedCount, toggleBlocked } = useChatRooms(rooms)
</script>

<template>
  <div class="chat-layout container-fluid py-3">
    <div class="chat-shell card shadow-sm border-0">
      <div class="row g-0">
        <!-- 左：聊天列表 -->
        <aside
          class="col-12 col-lg-4 col-xl-3 chat-list border-end d-flex flex-column overflow-x-hidden"
          style="height: 100%"
          v-if="!isMobile || mobilePanel === 'list'"
        >
          <div class="p-3 border-bottom">
            <div class="d-flex align-items-center justify-content-between">
              <strong class="fs-6">訊息{{ currentUser.name }}</strong>

              <span class="badge text-bg-light border">{{ currentUser.role }}</span>
            </div>
            <div class="mt-2">
              <input class="form-control form-control-sm" placeholder="搜尋聯絡人…" />
            </div>
          </div>

          <div
            class="list-group list-group-flush chat-list-scroll flex-grow-1"
            style="overflow-y: auto; max-width: 100%"
          >
            <div
              v-for="r in activeRooms"
              :key="String(r.id)"
              class="list-group-item list-group-item-action chat-item d-flex min-w-0"
              :class="{
                active: currentRoom === String(r.id),
                'room-blocked': r.isBlocked,
              }"
              @click="selectRoom(r)"
            >
              <div class="d-flex align-items-start gap-2 min-w-0 flex-grow-1 w-100">
                <div class="avatar bg-primary-subtle text-primary-emphasis flex-shrink-0">
                  <i class="fa-regular fa-user"></i>
                </div>

                <div class="flex-grow-1 w-100 min-w-0">
                  <div class="d-flex align-items-center w-100 gap-2">
                    <!-- 粗體：對方名字 -->
                    <div class="fw-semibold text-truncate flex-grow-1 min-w-0">
                      {{
                        currentUser.role === 'Host'
                          ? r.userName
                          : `${r.hostCompanyName ? r.hostCompanyName + ' - ' : ''}${r.hostName}`
                      }}
                    </div>
                  </div>
                  <!-- 小字：最後一句話 preview（沒有就顯示空字串） -->
                  <div class="d-flex align-items-center mt-1 w-100">
                    <div class="flex-grow-1 min-w-0 overflow-hidden">
                      <small class="text-muted text-truncate d-block">
                        {{ r.lastMessagePreview || '' }}</small
                      >
                    </div>
                    <!-- 右邊：未讀 badge（固定在最右） -->
                    <span
                      v-if="r.unread"
                      class="badge rounded-pill text-bg-primary ms-auto flex-shrink-0"
                    >
                      {{ r.unread }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 置底封鎖 -->
          <div class="blocked-section border-top bg-light">
            <div
              @click="toggleBlocked"
              class="p-3 d-flex justify-content-between align-items-center cursor-pointer hover-bg-gray"
              style="cursor: pointer"
            >
              <span class="text-muted fw-bold small">
                <i class="fa-solid fa-user-slash me-2"></i>已封鎖的對話 ({{ blockedCount }})
              </span>
              <i class="fa-solid" :class="showBlocked ? 'fa-chevron-down' : 'fa-chevron-up'"></i>
            </div>

            <div
              v-if="showBlocked"
              class="blocked-list-dropdown"
              style="max-height: 200px; overflow-y: auto"
            >
              <div
                v-for="r in blockedRooms"
                :key="String(r.id)"
                class="list-group-item list-group-item-action chat-item bg-white border-bottom"
                @click="selectRoom(r)"
              >
                <div class="d-flex align-items-center gap-2 px-3 py-2">
                  <div class="avatar-sm bg-secondary-subtle text-secondary small flex-shrink-0">
                    <i class="fa-solid fa-lock"></i>
                  </div>
                  <div class="small text-truncate flex-grow-1">
                    {{ currentUser.role === 'Host' ? r.userName : r.hostName }}
                  </div>
                  <span class="badge rounded-pill bg-secondary text-white">已封鎖</span>
                </div>
              </div>
              <div v-if="blockedCount === 0" class="p-3 text-center text-muted small">
                暫無封鎖對象
              </div>
            </div>
          </div>
        </aside>

        <!-- 右：聊天視窗 -->
        <section
          class="col-12 col-lg-8 col-xl-9 d-flex flex-column position-relative"
          v-if="!isMobile || mobilePanel === 'chat'"
        >
          <!-- 沒選房間時 -->
          <div
            v-if="!currentRoom"
            class="h-100 d-flex flex-column justify-content-center align-items-center text-muted"
          >
            <i class="fa-regular fa-comments fs-1 mb-3"></i>
            <p>請選擇對話開始聊天</p>
          </div>
          <template v-else>
            <!-- 如果有封鎖-灰色遮罩 -->
            <div v-if="currentRoomIsBlocked" class="blocked-mask-overlay">
              <div
                class="text-center p-5 bg-white rounded-4 shadow-lg border"
                style="max-width: 400px"
              >
                <div class="mb-3">
                  <span class="fa-stack fa-2x">
                    <i class="fa-solid fa-circle fa-stack-2x text-light"></i>
                    <i class="fa-solid fa-user-slash fa-stack-1x text-secondary"></i>
                  </span>
                </div>
                <h4 class="fw-bold text-dark">對話已鎖定</h4>
                <p class="text-muted mb-4">您已將此用戶加入黑名單，如需重新對話請先解除封鎖。</p>
                <button
                  @click="handleUnblock"
                  class="btn btn-outline-primary px-4 py-2 fw-bold shadow-sm"
                >
                  <i class="fa-solid fa-user-check me-2"></i>解除黑名單封鎖
                </button>
              </div>
            </div>
            <!-- 選到房間後 -->

            <!-- header -->
            <div class="chat-header px-3 py-2 border-bottom bg-white">
              <!-- 手機返回列：只在手機顯示 -->
              <div v-if="isMobile" class="chat-mobile-topbar">
                <button class="btn btn-link p-0" type="button" @click="goBackToRoomList">
                  <i class="fa-solid fa-chevron-left me-2"></i>返回
                </button>
              </div>
              <!-- ----------------- -->
              <div class="d-flex align-items-center justify-content-between">
                <div class="d-flex align-items-center gap-2">
                  <div class="avatar bg-success-subtle text-success-emphasis">
                    <i class="fa-regular fa-comments"></i>
                  </div>
                  <div class="min-w-0">
                    <div class="fw-bold text-truncate">{{ currentRoomTitle }}</div>
                    <small class="text-muted"
                      >訊息送出後將無法收回或刪除，請確認內容後再傳送。</small
                    >
                  </div>
                </div>

                <div class="d-flex gap-2">
                  <button
                    class="btn btn-sm btn-outline-secondary dropdown-toggle-split"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                  >
                    <i class="fa-solid fa-ellipsis"></i>
                  </button>
                  <ul class="dropdown-menu dropdown-menu-end">
                    <li v-if="!currentRoomIsBlocked">
                      <button
                        type="button"
                        class="dropdown-item py-2 item-danger-hover"
                        @click.prevent="handleBlock"
                      >
                        <i class="fa-solid fa-user-slash me-2"></i>將此用戶加入黑名單
                      </button>
                    </li>



                  </ul>
                </div>
              </div>
            </div>

            <!-- messages -->
            <div class="chat-body px-3 py-3">
              <div class="d-flex flex-column gap-2">
                <template v-for="m in messages" :key="m.id">
                  <div v-if="m.isFirstUnread" class="new-msg-divider">
                    <span>新訊息</span>
                  </div>

                  <div class="msg-row" :class="{ 'msg-row--mine': m.mine }">
                    <div class="msg-bubble">
                      <div class="msg-text">{{ m.text }}</div>
                      <div class="msg-meta">{{ m.time }}</div>
                    </div>
                  </div>
                </template>
              </div>
            </div>
            <!-- input -->
            <div class="chat-footer border-top bg-white px-3 py-2">
              <div class="input-group">
                <input
                  v-model="newMessage"
                  @keyup.enter="sendMessage"
                  type="text"
                  class="form-control"
                  placeholder="輸入訊息…"
                />
                <button @click="sendMessage" class="btn btn-primary" type="button">
                  <i class="fa-solid fa-paper-plane me-1"></i>送出
                </button>
              </div>
              <small class="text-muted d-block mt-1">
                請勿在訊息中分享個資（電話/身分證/地址）
              </small>
            </div>
          </template>
        </section>
      </div>
    </div>
  </div>
</template>

<style scoped></style>
