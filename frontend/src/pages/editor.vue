<script setup lang="ts">
import { VueMonacoEditor } from '@guolao/vue-monaco-editor'
import { ref, shallowRef } from 'vue'


let items = [
  {
    title: 'Python',
    value: 1,
  },
  {
    title: 'C#',
    value: 2,
  },
  {
    title: 'C++',
    value: 3,
  },
];

const MONACO_EDITOR_OPTIONS = {
  automaticLayout: true,
  formatOnType: true,
  formatOnPaste: true,

}

const code = ref('// some code...')
const editorRef = shallowRef()
const handleMount = editor => (editorRef.value = editor)

// your action
function formatCode() {
  editorRef.value?.getAction('editor.action.formatDocument').run()
}

</script>

<template>
  <v-theme-provider theme="dark">
    <v-app-bar
      height="40"
      color="teal-darken-4"
      flat
      image="https://picsum.photos/1920/1080?random"
    >
      <div class="flex-fill d-flex align-center mx-5">
        <span class="justify-start">
          <v-app-bar-title>Компилятор</v-app-bar-title>
        </span>

        <span class="mx-auto justify-center">
          <v-btn icon disabled>
            <v-icon>mdi-stop</v-icon>
          </v-btn>

          <v-btn icon disabled>
            <v-icon>mdi-play</v-icon>
          </v-btn>
        </span>
      </div>
    </v-app-bar>

    <v-card variant="flat" style="margin-top: -5px; width: 100%; height: 101%">
      <v-row class="mx-2 mt-3">
        <v-col cols="2">
          <v-card border>
            <v-card-title>Язык</v-card-title>
            <v-divider/>

            <v-list :items="items"></v-list>
          </v-card>
        </v-col>

        <v-col cols="10">
          <v-row style="height: 71vh">
            <v-card style="height: 100%; width: 100%" border>
              <VueMonacoEditor
                v-model:value="code"
                theme="vs-dark"
                :options="MONACO_EDITOR_OPTIONS"
                @mount="handleMount"
                language="python"
              />
            </v-card>
          </v-row>

          <v-row style="height: 20vh">
            <v-card style="height: 100%; width: 100%" border>
              <v-card-title>Логи</v-card-title>
              <v-divider/>
              <div class="ma-2" style="max-height: 100%; overflow-y: scroll">
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
                LOGS <br/>
              </div>
            </v-card>
          </v-row>
        </v-col>
      </v-row>
    </v-card>
  </v-theme-provider>
</template>

<style scoped lang="sass">

</style>
