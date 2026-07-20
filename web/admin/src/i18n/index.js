import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'

import { storageKeys } from '../config/storageKeys'
import { en } from './locales/en'
import { vi } from './locales/vi'

export const supportedLanguages = Object.freeze(['en', 'vi'])

const savedLanguage = localStorage.getItem(storageKeys.language)
const initialLanguage = supportedLanguages.includes(savedLanguage) ? savedLanguage : 'en'

localStorage.setItem(storageKeys.language, initialLanguage)

i18n
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      vi: { translation: vi },
    },
    lng: initialLanguage,
    fallbackLng: 'en',
    supportedLngs: supportedLanguages,
    interpolation: { escapeValue: false },
  })

i18n.on('languageChanged', (language) => {
  localStorage.setItem(storageKeys.language, language)
  document.documentElement.lang = language
})

document.documentElement.lang = initialLanguage

export default i18n
