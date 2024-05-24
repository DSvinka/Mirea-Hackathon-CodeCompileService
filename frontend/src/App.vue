<template>
  <v-app>
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script lang="ts" setup>
import {setupSignalR} from "./api/signalr/docker-containers-signalr"

setupSignalR()
</script>
