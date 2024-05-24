<script lang="ts" setup>
import {useRouter} from "vue-router";

const router = useRouter()
</script>


<template>
  <div class="background">
    <div class="header">
      <img class="main" src="@/assets/logo.webp" alt="Adeptus ALTuches"/>
    </div>

    <div class="border"></div>

    <div class="box">
      <div class="KomLink">
        <a href=""><img class="logo" src="@/assets/kompilatorum.webp" alt="NoHomo"></a>
        <h1>Империалис компиляторум</h1>
        <div class="dabl"> Сквозь варп для каждого возжелавшего учиться подчинять дух машины!</div>
        <br><br>
        <a @click="router.push('/editor')">Нажмите для начала работы</a>
        <br><br><br><br><br><br><br><br><br><br>
        <q>(Запрещено для использования разработки искусственного интеллекта, замеченные за этим будут переданы инквизиция)</q>
      </div>
    </div>

    <div class="border"></div>

    <div class="box">
      <div class="history">
        <div class="Imperium">
          <img src="@/assets/Imperium2.webp" alt="">
        </div>
        <h1>История Адептус Альтушес</h1>
        <div class="dabl"> Шли тысячелетия, пока Адептус Механикус вели свою молитву, пока однажды, некоторым техножрецам не снизошла ересь о возможности подчинения духа машины. "В тёмную эру технологий люди именно так и использовали свою технику! Поэтому и покорили обширные территории галакити" – слышалось от еретиков. Начал образовываться новый култь: Адептус Программатус. Марс не стал терпеть еретизма и разрозилась война. Сначала мы проигрывали, но когда дух машины подчинился, Империуму пришлось признать наше место. Мы чтим каждого, кто приложил руку к формированию и защите Адептус Программатус, но особое место в нашем культе занял верховный техножрецы Ричард Столман и Линус Торвальдс, основатели Адептус Опенсорсус и Адептус Линукс. Уже от них исходим и мы, Адептус Альтушес (Альталес, Альтус).<br><br><br>Да благославлён будет каждый это читающий светом Императора!<br><br><br><br><br><br></div>
      </div>
    </div>

    <div class="border"></div>

    <div class="final"></div>
  </div>
</template>

<style scoped lang="scss">
.background{
  display: block;
  background-color: #000000;
  margin: 0 auto;
}
.box{
  width: auto;
  font-family: "Andale Mono";
  background-color: #FBF6F0;
  padding: 20px 30px 0 30px;
  min-height: 20em;
  height: auto;
}
.box .KomLink{
  display: block;
  max-width: 920px;
}
.KomLink img{
  display: block;
  position: relative;
  width: 25em;
  float: right;
}
.KomLink h1{
  font-size: 4em;
}
.dabl{
  font-size: 1.4em;
}
@font-face {
  font-family: 'Andale Mono';
  src: url('../assets/Andale Mono.ttf') format('truetype');
  font-weight: 700;
  font-style: normal;
}
.history{
  text-align: justify;
  max-width: 920px;
  font-family: "Andale Mono";
  background-color: #FBF6F0;
  padding: 0 30px 0 30px;
}
.history h1{
  font-size: 3em;
  text-align: center;
}
.header{
  position: relative;
  width: 100%;
  height: auto;
  overflow: hidden;
}
.header img {
  display: block;
  width: 100%;
  height: auto;
}
.history img{
  width: 100%;
}
.border{
  height: 20px;
}
.final{
  background-color: #08006A;
  height: 8em;
}
</style>
