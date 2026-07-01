const react = require('@vitejs/plugin-react');\nmodule.exports = {\n  plugins: [react()],\n  server: { port: 5173, proxy: { '/api': { target: 'http://localhost:5000', changeOrigin: true } } }\n};\n
