<script setup lang="ts">
import { VueMonacoEditor } from '@guolao/vue-monaco-editor'
import { ref, shallowRef } from 'vue'
import {useDockerStore} from "../stores/docker"
import {Docker, getAuthHeader} from "../api"
import {CreateDockerContainerRequest, DeleteDockerContainerRequest} from "../api/models/docker-container-models";

const MONACO_EDITOR_OPTIONS = {
  automaticLayout: true,
  formatOnType: true,
  formatOnPaste: true,
}

const dockerStore = useDockerStore();

const code = ref('// some code...')
const imageId = ref(null)

const editorRef = shallowRef()
const handleMount = editor => (editorRef.value = editor)

function codeStart() {
  const model: CreateDockerContainerRequest = {
    connectionId: dockerStore.getConnectionId,
    imageId: dockerStore.currentImageIndex,

    programCode: code.value
  }

  dockerStore.enableLoading();
  Docker.post("/containers/create", model, {headers: getAuthHeader()})
    .then(res => dockerStore.disableLoading())
}

function codeStop() {
  const model: DeleteDockerContainerRequest = {
    connectionId: dockerStore.getConnectionId,
    containerId: dockerStore.getCurrentContainerIndex
  }

  dockerStore.enableLoading();
  Docker.post("/containers/delete", model, {headers: getAuthHeader()})
    .then(res => dockerStore.disableLoading())
}

function onImageChange(event) {
  dockerStore.setCurrentImageIndex(event.target.value)
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
          <v-btn icon :disabled="dockerStore.getIsLoading && dockerStore.getCurrentContainerIndex != null" @click="codeStop">
            <v-icon>mdi-stop</v-icon>
          </v-btn>

          <v-btn icon :disabled="dockerStore.getIsLoading && !dockerStore.getCurrentContainerIndex != null" @click="codeStart">
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

            <v-list :disabled="dockerStore.getIsLoading && dockerStore.getCurrentContainerIndex != null" @change="onImageChange" v-model="imageId" :items="dockerStore.getImages" item-title="displayName" item-value="id"></v-list>
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
              <div v-if="dockerStore.getCurrentContainerIndex" class="ma-2" style="max-height: 100%; overflow-y: scroll">
                {{dockerStore.getContainers[dockerStore.getCurrentContainerIndex].logs}}
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
