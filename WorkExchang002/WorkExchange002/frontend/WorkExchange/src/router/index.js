import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/HomeView.vue'
import Layout from '@/components/Layout.vue'
import UserLayout from '@/components/UserLayout.vue'
import HostLayout from '@/components/HostLayout.vue'
import http from '@/components/api/http'
import AdLayout from '@/components/AdLayout.vue'

// 移除 RouteRecordRaw[] 型別定義

const routes = [

  {
    path: '/',
    name: 'home',
    component: HomeView,
  },
  {
    path: '/hostlist',
    name: 'hostlist',
    component: () => import('../views/Meatball/HostList.vue'),
  },
  {
    path: '/hostdetail/:id',
    name: 'hostdetail',
    component: () => import('../views/Meatball/HostDetail.vue'),
  },
  {
    path: '/Hotvacancy',
    name: 'Hotvacancy',
    component: () => import('../views/Meatball/HotVacancy.vue'),
  },
  {
    path: '/vacancy/:id',
    name: 'vacancy',
    component: () => import('../views/Meatball/Vacancy.vue'),
  },
  {
    path: '/AdIntro',
    name: 'AdIntro',
    component: () => import('../views/AdIndex.vue'),
  },
  {

    path: '/BeSponsorStepOne',
    name: 'BeSponsorStepOne',
    component: () => import('../views/BeSponsorStepOne.vue'),
  },
  {
    path: '/BeSponsorStepTwo',
    name: 'BeSponsorStepTwo',
    component: () => import('../views/BeSponsorStepTwo.vue'),
  },
  {
    path: '/BeSponsorStepThree',
    name: 'BeSponsorStepThree',
    component: () => import('../views/BeSponsorStepThree.vue'),
  },
  {
    path: '/AdPreview',
    name: 'AdPreview',
    component: () => import('../views/AdPreview.vue'),
  },
  {
    path: '/Upload',
    name: 'Upload',
    component: () => import('../views/Upload.vue'),
  },
  {
    path: '/Success',
    name: 'Success',
    component: () => import('../views/AdTransactionSuccess.vue'),
  },
  {
    path: '/Donation',
    name: 'Donation',
    component: () => import('../views/Donation.vue'),
  },
  {
    path: '/SponsorLogin',
    name: 'SponsorLogin',
    component: () => import('../components/SponsorLogin.vue'),
  },
  {
    path:'/Ad',
    component: AdLayout,
    children:[
      {
        path: '',
        redirect: '/Ad/AdHomePage',
      },
      {
        path: 'AdHomePage',
        name: 'AdHomePage',
        component: () => import('../views/AdHomePage.vue'),
      },
      {
        path: 'CreateNewAd',
        name: 'CreateNewAd',
        component: () => import('../views/CreateNewAd.vue'),
      },
      {
        path: 'AdHistory',
        name: 'AdHistory',
        component: () => import('../views/AdHistory.vue'),
      },
      {
        path: 'AdProfile',
        name: 'AdProfile',
        component: () => import('../views/AdProfile.vue'),
      },
    ]
  },
  {
    path: '/chat',
    name: 'chat',
    component: () => import('../views/chatView.vue'),
  },
  {
    path: '/FAQ',
    name: 'FAQ',
    component: () => import('../views/FAQ.vue'),
  },
  {
    path: '/TheReviews',
    name: 'TheReviews',
    component: () => import('../views/Baleen/TheReviews.vue'),
  },
  {
    path: '/TheReviewsDetail',
    name: 'TheReviewsDetail',
    component: () => import('../views/Baleen/TheReviewsDetail.vue'),
  },
  {
    path: '/TheReviewsDetail/:id',
    name: 'TheReviewsDetail',
    component: () => import('../views/Baleen/TheReviewsDetail.vue'),
  },
  {
    path: '/AU',
    name: 'AU',
    component: () => import('../views/About-why.vue'),
  },
  {
    path: '/UserProfile/:id',
    name: 'UserProfile',
    component: () => import('../views/Baleen/UserProfile.vue'),

  },
  {
    path: '/member',
    component: UserLayout,
    children: [
      {
        path: '',
        redirect: '/member/MemberView',
      },
      {
        path: 'MemberView',
        name: 'MemberView',
        component: () => import('../views/Baleen/MemberView.vue'),

      },
      {
        path: 'UserView',
        name: 'UserView',
        component: () => import('../views/Baleen/UserView.vue'),
      },
      {
        path: 'ReviewView',
        name: 'ReviewView',
        component: () => import('../views/Baleen/ReviewView.vue'),
      },
      {
        path: 'ReviewEdit/:id',
        name: 'ReviewEdit',
        component: () => import('../views/Baleen/ReviewEdit.vue'),
      },
      {
        path: 'PasswordEdit',
        name: 'PasswordEdit',
        component: () => import('../views/Baleen/PasswordEdit.vue'),
      },
      {
        path: 'UserCollect',
        name: 'UserCollect',
        component: () => import('../views/Baleen/UserCollect.vue'),
      },
      {
        path: 'UserProfile',
        name: 'UserProfile',
        component: () => import('../views/Baleen/UserProfile.vue'),
      },
    ],
  },
  {
    path: '/host',
    component: HostLayout,
    meta: { hideHeader: true },
    children: [
      {
        path: '',
        redirect: '/host/HostHomeView',
      },
      {
        path: 'HostHomeView',
        name: 'HostHomeView',
        component: () => import('../views/Meatball/HostHomeView.vue'),
      },
      {
        path: 'HostManage',
        name: 'HostManage',
        component: () => import('../views/Meatball/HostManage.vue'),
      },
      {
        path: 'HostPage',
        name: 'HostPage',
        component: () => import('../views/Meatball/HostPage.vue'),
      },
      {
        path: 'HostVacancy',
        name: 'HostVacancy',
        component: () => import('../views/Meatball/HostVacancy.vue'),
      },
      {
        path: 'HostEmployee',
        name: 'HostEmployee',
        component: () => import('../views/Meatball/HostEmployee.vue'),
      },
      {
        path: 'HostPassword',
        name: 'HostPassword',
        component: () => import('../views/Meatball/HostPassword.vue'),
      },
    ],
  },
  {
    path: '/MM',
    component: Layout,
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        redirect: '/MM/Dashboard',
      },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('../views/backend/MMindex.vue'),
      },
      {
        path: 'members',
        name: 'Members',
        component: () => import('../views/backend/members/MembersManagement.vue'),
      },
      {
        path: 'members/owners',
        name: 'OwnersList',
        component: () => import('../views/backend/members/OwnersManagement.vue'),
      },
      {
        path: 'members/workers',
        name: 'WorkersList',
        component: () => import('../views/backend/members/WorkersManagement.vue'),
      },
      {
        path: 'profile',
        name: 'Profile',
        component: () => import('../views/backend/ProfileManagement.vue'),
      },
      {
        path: 'owner-backend',
        name: 'owner-backend',
        component: () => import('../views/backend/OwnerBackendManagement.vue'),
      },
      {
        path: 'ads/plans',
        name: 'plans',
        component: () => import('../views/backend/ads/PlansManagement.vue'),
      },
      {
        path: 'ads/applications',
        name: 'applications',
        component: () => import('../views/backend/ads/ApplicationsManagement.vue'),
      },
      {
        path: 'ads/customers',
        name: 'customers',
        component: () => import('../views/backend/ads/CustomersManagement.vue'),
      },
      {
        path: 'TheBaleen',
        name: 'TheBaleen',
        component: () => import('../views/backend/TheBaleen.vue'),
      },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,

  // ✅ 新增這段就好
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return new Promise((resolve) => {
        setTimeout(() => {
          resolve(savedPosition)
        }, 300) // 給 DOM / 圖片一點時間
      })
    }

    return { top: 0 }
  },
})
router.beforeEach(async (to, from, next) => {
  // 如果目標路由需要驗證
  if (to.matched.some((record) => record.meta.requiresAuth)) {
    try {
      // 1. 務必加上 await
      const response = await http.get('/api/users/adminaccess');

      // 2. 只有當狀態碼是 200 時才放行
      if (response.status === 200) {
        console.log('驗證通過，放行進入')
        next() // 這裡才跳轉
      } else {
        throw new Error('身分驗證失敗')
      }
    } catch (error) {
      console.error('守衛攔截：', error.message)
      alert('請登入管理員帳號')
      next('/') // 驗證失敗，踢回登入頁
    }
  } else {
    // 不需要驗證的頁面直接放行
    next()
  }

})

export default router
