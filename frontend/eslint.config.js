import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import tseslint from 'typescript-eslint';
import importPlugin from 'eslint-plugin-import';
import simpleImportSort from 'eslint-plugin-simple-import-sort';
import unusedImports from 'eslint-plugin-unused-imports';

export default tseslint.config(
  { ignores: ['dist'] },
  {
    extends: [
      js.configs.recommended,
      ...tseslint.configs.recommended,
      // вместо строк — добавляй правила через plugins и rules
    ],
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      'import': importPlugin,
      'simple-import-sort': simpleImportSort,
      'unused-imports': unusedImports,  // Добавлен плагин
    },
    settings: {
      'import/resolver': {
        typescript: {},
      },
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'no-console': ['warn', { allow: ['error'] }],
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
      
      // Отключаем стандартные правила неиспользуемых переменных
      '@typescript-eslint/no-unused-vars': 'off',
      'no-unused-vars': 'off',
      
      // Включаем правило для неиспользуемых импортов
      'unused-imports/no-unused-imports': 'error',  // <-- Основное правило
      'simple-import-sort/imports': [
        'error',
        {
          groups: [
            // 1. Встроенные модули node (если используешь)
            ['^node:'],
            // 2. Внешние пакеты
            ['^react', '^@?\\w'],
            // 3. Алиасы
            ['^@/'],
            // 4. Относительные импорты
            ['^\\.'],
            // 5. Стили (css/scss/less) — ВСЕГДА В КОНЦЕ!
            ['^.+\\.s?css$'],
          ],
        },
      ],
      'simple-import-sort/exports': 'error',
      'import/no-unresolved': 'error',
      'indent': ['warn', 'tab'], // Используем табы вместо пробелов
      'no-mixed-spaces-and-tabs': 'error' // Запрещаем смешивание
    },
  }
);
