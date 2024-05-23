// Plugins
import { registerPlugins } from './plugins'

// Components
import App from './App.vue'

// Composables
import { createApp } from 'vue'

import {connection as dockerContainersConnection} from "./api/signalr/docker-containers-signalr"

const app = createApp(App)

registerPlugins(app)

dockerContainersConnection.start()

app.mount('#app')
