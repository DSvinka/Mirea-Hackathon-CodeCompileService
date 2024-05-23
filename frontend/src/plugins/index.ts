import vuetify from './vuetify'
import pinia from '../stores'
import router from '../router'

import { loader as monacoEditorLoader } from '@guolao/vue-monaco-editor'

// Types
import type { App } from 'vue'

monacoEditorLoader.config({
  paths: {
    vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.43.0/min/vs',
  },
})

export function registerPlugins (app: App) {
  app
    .use(vuetify)
    .use(router)
    .use(pinia)
}
