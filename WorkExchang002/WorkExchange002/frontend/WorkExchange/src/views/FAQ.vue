<script setup>
import '@/assets/css/faq.css'
</script>

<template>
  <div class="faq-page">
    <!-- Banner -->
    <div class="faq-banner-wrap">
      <img src="@/assets/images/faq-banner.jpg" alt="FAQ Banner" class="faq-banner" />
    </div>
    <h1 class="faq-title" style="display: flex; justify-content: center">一般常見問題</h1>
    <div class="accordion" id="faq-general">
      <!-- Q1 -->
      <div class="accordion-item">
        <h2 class="accordion-header" id="general-q1">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#general-a1"
            aria-expanded="false"
            aria-controls="general-a1"
          >
            甚麼是打工換宿?
          </button>
        </h2>
        <div id="general-a1" class="accordion-collapse collapse" aria-labelledby="general-q1" data-bs-parent="#faq-general">
          <div class="accordion-body">
            打工換宿是一種結合旅行與工作的模式，參與者（通常稱「小幫手」）用自己的勞務或技能（如清潔、接待、才藝教學）來交換免費的食宿，以此降低旅行成本，並能深度體驗當地生活。
          </div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="general-q2">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#general-a2"
            aria-expanded="false"
            aria-controls="general-a2"
          >
            打工換宿有薪水嗎?
          </button>
        </h2>
        <div id="general-a2" class="accordion-collapse collapse" aria-labelledby="general-q2" data-bs-parent="#faq-general">
          <div class="accordion-body">
            「打工換宿」(Work Exchange) 顧名思義就是以「打工」（勞務或專業技能）來抵「住宿」。
            本身就存在利益交換，因此基本上不會有額外「薪水」，但可能有「零用金」、交通補貼、供餐等其他福利，依雇主刊登內容為準。
          </div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="general-q3">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#general-a3"
            aria-expanded="false"
            aria-controls="general-a3"
          >
            註冊平台會員需要付費嗎?
          </button>
        </h2>
        <div id="general-a3" class="accordion-collapse collapse" aria-labelledby="general-q3" data-bs-parent="#faq-general">
          <div class="accordion-body">一般使用者註冊與瀏覽職缺、求職等皆為免費。</div>
        </div>
      </div>
    </div>
    <h1 class="faq-title" style="display: flex; justify-content: center">換宿者(求職者)常見問題</h1>
    <div class="accordion" id="faq-worker">
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-worker-q1">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-worker-a1"
            aria-expanded="false"
            aria-controls="faq-worker-a1"
          >
            打工換宿需要具備什麼資格？有年齡限制嗎？
          </button>
        </h2>
        <div id="faq-worker-a1" class="accordion-collapse collapse" aria-labelledby="faq-worker-q1" data-bs-parent="#faq-worker">
          <div class="accordion-body">
            基本上任何人都能打工換宿，在台灣通常會要求 18 歲以上，或未成年需要家人同意。
            如果是比較需要體力的工作，可能會有年齡上限限制。
            有些店家會希望小幫手有汽／機車駕照；離島至少要有機車駕照，才方便移動。
          </div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-worker-q2">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-worker-a2"
            aria-expanded="false"
            aria-controls="faq-worker-a2"
          >
            多久可以收到回覆?
          </button>
        </h2>
        <div id="faq-worker-a2" class="accordion-collapse collapse" aria-labelledby="faq-worker-q2" data-bs-parent="#faq-worker">
          <div class="accordion-body">回覆時間由雇主自行決定，請耐心等候，並留意平台訊息。</div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-worker-q3">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-worker-a3"
            aria-expanded="false"
            aria-controls="faq-worker-a3"
          >
            有沒有供餐?
          </button>
        </h2>
        <div id="faq-worker-a3" class="accordion-collapse collapse" aria-labelledby="faq-worker-q3" data-bs-parent="#faq-worker">
          <div class="accordion-body">
            是否供餐依實際職缺內容為準。 部分職缺可能提供餐食，實際供餐方式請於申請前與雇主確認。
          </div>
        </div>
      </div>
    </div>
    <h1 class="faq-title" style="display: flex; justify-content: center">雇主(商家)常見問題</h1>
    <div class="accordion" id="faq-employer">
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-employer-q1">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-employer-a1"
            aria-expanded="false"
            aria-controls="faq-employer-a1"
          >
            我要怎麼刊登徵才廣告?
          </button>
        </h2>
        <div id="faq-employer-a1" class="accordion-collapse collapse" aria-labelledby="faq-employer-q1" data-bs-parent="#faq-employer">
          <div class="accordion-body">雇主須先完成註冊與登入，即可於刊登。</div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-employer-q2">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-employer-a2"
            aria-expanded="false"
            aria-controls="faq-employer-a2"
          >
            我可以隨時關閉或編輯徵才廣告嗎?
          </button>
        </h2>
        <div id="faq-employer-a2" class="accordion-collapse collapse" aria-labelledby="faq-employer-q2" data-bs-parent="#faq-employer" >
          <div class="accordion-body">可以，雇主可隨時於後台編輯或關閉。</div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-employer-q3">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-employer-a3"
            aria-expanded="false"
            aria-controls="faq-employer-a3"
          >
            我可以同時刊登多個徵才廣告嗎?
          </button>
        </h2>
        <div id="faq-employer-a3" class="accordion-collapse collapse" aria-labelledby="faq-employer-q3" data-bs-parent="#faq-employer">
          <div class="accordion-body">可以，雇主可依需求刊登多筆職缺，並分別管理。</div>
        </div>
      </div>
    </div>
     <h1 class="faq-title" style="display: flex; justify-content: center">安心換宿指南</h1>
    <div class="accordion" id="faq-notice">
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-notice-q1">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-notice-a1"
            aria-expanded="false"
            aria-controls="faq-notice-a1"
          >
            WorkExchange 在媒合過程中扮演什麼角色？
          </button>
        </h2>
        <div id="faq-notice-a1" class="accordion-collapse collapse" aria-labelledby="faq-notice-q1" data-bs-parent="#faq-notice">
          <div class="accordion-body">本平台為開放式資訊橋樑，負責建立換宿者與商家間的聯繫管道。由於每筆計畫的工時、工作內容及福利均不盡相同，相關細節應由雙方在聯繫過程中達成共識。建議您在出發前詳盡確認各項約定，以確保雙方均享有良好的換宿體驗。</div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-notice-q2">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-notice-a2"
            aria-expanded="false"
            aria-controls="faq-notice-a2"
          >
            若抵達換宿現場發現與描述不符，該如何處理？
          </button>
        </h2>
        <div id="faq-notice-a2" class="accordion-collapse collapse" aria-labelledby="faq-notice-q2" data-bs-parent="#faq-notice">
          <div class="accordion-body">若遇實際情況與頁面描述有顯著落差，建議先與對方核對原先的約定。若雙方無法達成共識，或現場環境與說明嚴重不符、導致無法正常居住，請務必先保障自身安全並暫停換宿行程。若需反映資訊真實性，請將相關證明發送至頁尾信箱，我們會將此作為後續職缺審核與資訊維護的直接依據。</div>
        </div>
      </div>
      <div class="accordion-item">
        <h2 class="accordion-header" id="faq-notice-q3">
          <button
            class="accordion-button"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#faq-notice-a3"
            aria-expanded="false"
            aria-controls="faq-notice-a3"
          >
            如何保障個人的安全與財產權益？
          </button>
        </h2>
        <div id="faq-notice-a3" class="accordion-collapse collapse" aria-labelledby="faq-notice-q3" data-bs-parent="#faq-notice">
          <div class="accordion-body">媒合期間請提高警覺並審慎保護隱私資料。請注意，宿主不應要求提供身分證正本、銀行帳戶密碼或預繳保證金，若遇到此類要求請務必拒絕。任何私下簽署的書面協議，請務必詳閱條款內容並留存副本，以維護自身權益。</div>
        </div>
      </div>
    </div>
  </div>
</template>
