import Vue from 'vue'
import App from './App.vue'
import router from './router'
import VueCodemirror from 'vue-codemirror'
import './registerServiceWorker'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap/dist/css/bootstrap-grid.css'
import 'bootstrap/dist/css/bootstrap-reboot.css'
import 'font-awesome/css/font-awesome.css'
import 'toastr/build/toastr.css'

// CodeMirror
import 'codemirror/lib/codemirror.css'
import 'codemirror/mode/yaml/yaml.js'

// Filters
import './filters/yaml'
import './filters/datetime'

import './functions/stringFormat'

Vue.config.productionTip = false

Vue.use(VueCodemirror)

new Vue({
  router,
  render: h => h(App)
}).$mount('#app')
