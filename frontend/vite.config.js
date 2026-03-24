import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5272',
        changeOrigin: true,
        secure: false
      },
      '/images': {
        target: 'http://localhost:5272',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
