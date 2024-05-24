// Composables
import { createRouter, createWebHistory } from 'vue-router/auto'
import { setupLayouts } from 'virtual:generated-layouts'
import { useAuthStore } from "../stores/auth";
import { storeToRefs } from "pinia";

const router = createRouter({
  history: createWebHistory(),
  extendRoutes: (routes) => [
    {
      path: '/editor',
      redirect: to => {
        const authStore = useAuthStore()
        const { isLoggedIn } = storeToRefs(authStore)

        return isLoggedIn.value ?
          { name: 'editor' } :
          { name: 'auth', query: to.query }
      },
    },
    ...setupLayouts(routes)
  ],
})

export default router
