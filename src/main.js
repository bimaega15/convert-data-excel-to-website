import { createApp } from 'vue'
import App from './App.vue'
import bootstrap from './plugins/bootstrap'
import './assets/dashboard.css'

const app = createApp(App)

app.use(bootstrap)
app.mount('#app')
