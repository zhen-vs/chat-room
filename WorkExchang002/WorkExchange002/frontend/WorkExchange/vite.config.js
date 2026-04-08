// vite.config.js
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import http from './src/components/api/http'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), vueDevTools()],
  base: '/', // 或者如果不確定，可以試試 './'

  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },

  // ← 這裡新增 server.proxy 設定（開發時用）
  server: {
    proxy: {
      // API 請求轉發（你原本的 Reviews API 就用這個）
      '/api': {
        target: http.defaults.baseURL,   // 你的後端地址
        changeOrigin: true,                 // 改變 origin 避免 CORS 問題
        secure: false,                      // 忽略後端自簽憑證錯誤（開發用）
      },

      // 重要：靜態檔案 uploads 也要代理
      '/uploads': {
        target: http.defaults.baseURL,
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
