import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // Use relative URLs so the production build can load from an Electron app
  // folder without a web server.
  base: './',
})
