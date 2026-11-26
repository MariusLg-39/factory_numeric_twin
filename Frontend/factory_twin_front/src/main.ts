import './assets/main.css'
import 'primeicons/primeicons.css'

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
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Dialog from 'primevue/dialog'
import Select from 'primevue/select'
import InputNumber from 'primevue/inputnumber'
import Tag from 'primevue/tag'
import Slider from 'primevue/slider'
import SelectButton from 'primevue/selectbutton'
import Tooltip from 'primevue/tooltip'

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
app.component('PrimeDataTable', DataTable)
app.component('PrimeColumn', Column)
app.component('PrimeDialog', Dialog)
app.component('PrimeSelect', Select)
app.component('PrimeInputNumber', InputNumber)
app.component('PrimeTag', Tag)
app.component('PrimeSlider', Slider)
app.component('PrimeSelectButton', SelectButton)

app.directive('tooltip', Tooltip)

app.mount('#app')
