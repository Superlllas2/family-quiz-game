import { defineConfig } from 'vitest/config';

export default defineConfig({
  // JSX through Vite's own transformer, no plugin. Vite 8 transforms with Oxc and ignores the
  // `esbuild` option, so the Preact JSX runtime is set here.
  oxc: { jsx: { runtime: 'automatic', importSource: 'preact' } },
  server: {
    // Reachable from phones on the same WiFi during `npm run dev`.
    host: true,
    // The page imports ../content/characters.json at build time; allow the repo root (relative to phone/).
    fs: { allow: ['..'] },
  },
  build: {
    outDir: 'dist',
    emptyOutDir: true,
  },
  test: {
    environment: 'node',
    include: ['src/**/*.test.ts'],
  },
});
