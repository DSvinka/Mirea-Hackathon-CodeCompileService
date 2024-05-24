export const validationSchema = {
  name (value) {
    if (value?.length >= 2) return true

    return 'Имя должно состоять как минимум из 2 символов.'
  },
  phone (value) {
    if (value?.length > 9 && /[0-9-]+/.test(value)) return true

    return 'Номер телефона должен состоять как минимум из 9 цифр..'
  },
  email (value) {
    if (/^[a-z.-]+@[a-z.-]+\.[a-z]+$/i.test(value)) return true

    return 'Должен быть действительный адрес электронной почты.'
  },
  select (value) {
    if (value) return true

    return 'Выберите элемент.'
  },
  checkbox (value) {
    if (value === '1') return true

    return 'Пункт необходимо отметить.'
  },
}
