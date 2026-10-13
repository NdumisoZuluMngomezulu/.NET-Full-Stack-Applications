import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // During development, forward API calls to the ASP.NET backend.
    proxy: { '/api': 'http://localhost:5080' },
  },
  build: {
    // `npm run build` drops the production site into the backend's wwwroot folder.
    outDir: '../backend/MovieTracker.Api/wwwroot',
    emptyOutDir: true,
  },
});
