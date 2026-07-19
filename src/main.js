import { createApp } from 'vue'
import App from './App.vue'
import bootstrap from './plugins/bootstrap'
import router from './router'
import './assets/dashboard.css'

createApp(App).use(bootstrap).use(router).mount('#app')
