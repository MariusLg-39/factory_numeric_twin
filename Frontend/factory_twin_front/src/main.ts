import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'

import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Button from 'primevue/button'
import Message from 'primevue/message'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.use(PrimeVue, {
  theme: {
    preset: Aura
  }
})

//Composant globaux (merci d'importer les composants PrimeVue ici et non dans les composants)
app.component('PrimeCard', Card)
app.component('PrimeInputText', InputText)
app.component('PrimePassword', Password)
app.component('PrimeButton', Button)
app.component('PrimeMessage', Message)

app.mount('#app')
