<script setup lang="ts">
import { ref } from 'vue'
import { useField, useForm } from 'vee-validate'
import {validationSchema} from "../plugins/validation";
import {useAuthStore} from "../stores/auth";

const { handleSubmit, handleReset } = useForm({
  validationSchema: validationSchema
})

const authStore = useAuthStore();
const router = useRouter();

const email = useField('email')
const password = useField('password')

const submit = handleSubmit(values => {
  authStore.login({email: values.email, password: values.password})
    .then(() => router.push("/editor"))
});
</script>

<template>
  <div class="d-flex align-content-center justify-content-center" style="width: 100%; height: 100%;">
    <v-card>
      <v-card-title>Авторизация</v-card-title>

      <v-form>
        <v-text-field
          v-model="email.value.value"
          :counter="10"
          :error-messages="email.errorMessage.value"
          label="Email"
        ></v-text-field>

        <v-text-field
          v-model="password.value.value"
          :counter="10"
          :error-messages="password.errorMessage.value"
          label="Password"
        ></v-text-field>

        <v-btn @click="submit">Войти</v-btn>
      </v-form>
    </v-card>
  </div>
</template>

<style scoped lang="sass">

</style>
