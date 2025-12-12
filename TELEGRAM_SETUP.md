# Инструкция по настройке Telegram бота

## 1. Создание аккаунта админа или менеджера

### Через веб-интерфейс (требуется существующий админ):
1. Войдите в систему как администратор
2. Перейдите в раздел "Пользователи" (Admin → Users)
3. Нажмите кнопку "Создать админа/менеджера"
4. Заполните форму:
   - **Telegram ID** - ваш Telegram ID (можно узнать у бота @userinfobot)
   - **Логин** - логин для входа в систему
   - **Пароль** - пароль для входа
   - **Роль** - выберите "Администратор" или "Менеджер"
5. Нажмите "Создать пользователя"

### Через базу данных (если нет админа):
Выполните SQL запрос в базе данных PostgreSQL:

```sql
INSERT INTO "Users" ("TelegramId", "Username", "PasswordHash", "Role", "IsConfirmed", "CreatedAt")
VALUES (
    123456789,  -- Замените на ваш Telegram ID
    'admin',    -- Замените на желаемый логин
    '$2a$11$...',  -- Замените на хеш пароля (используйте BCrypt)
    'Admin',    -- Или 'Manager' для менеджера
    true,
    NOW()
);
```

Для генерации BCrypt хеша пароля используйте онлайн-генератор или .NET код:
```csharp
BCrypt.Net.BCrypt.HashPassword("ваш_пароль")
```

## 2. Настройка Telegram Webhook

### Вариант 1: Через API эндпоинт (рекомендуется)

После запуска приложения, выполните запрос:

```bash
POST https://ваш-домен/api/telegrambot/setup-webhook?url=https://ваш-домен/api/telegrambot/webhook
```

Или через браузер:
```
https://ваш-домен/api/telegrambot/setup-webhook?url=https://ваш-домен/api/telegrambot/webhook
```

### Вариант 2: Вручную через Telegram API

Выполните GET запрос:
```
https://api.telegram.org/bot<ВАШ_BOT_TOKEN>/setWebhook?url=https://ваш-домен/api/telegrambot/webhook
```

Замените:
- `<ВАШ_BOT_TOKEN>` на токен из `appsettings.json` (Telegram:BotToken)
- `https://ваш-домен` на URL вашего приложения

### Проверка webhook

Проверить статус webhook можно через:
```
https://api.telegram.org/bot<ВАШ_BOT_TOKEN>/getWebhookInfo
```

## 3. Проверка работы бота

1. Найдите вашего бота в Telegram по ссылке из `appsettings.json` (Telegram:BotUrl)
2. Отправьте команду `/start` - бот должен ответить приветствием
3. Если вы зарегистрированы как студент, отправьте любое сообщение - бот подтвердит регистрацию

## 4. Важные моменты

- **Webhook URL должен быть HTTPS** (Telegram требует безопасное соединение)
- **Бот должен быть доступен публично** (не localhost)
- **Webhook endpoint** (`/api/telegrambot/webhook`) доступен без авторизации
- **Для локальной разработки** используйте ngrok или аналогичные сервисы для туннелирования

## 5. Локальная разработка с ngrok

1. Установите ngrok: https://ngrok.com/
2. Запустите приложение локально (например, на порту 5000)
3. В другом терминале запустите: `ngrok http 5000`
4. Скопируйте HTTPS URL из ngrok (например, `https://abc123.ngrok.io`)
5. Настройте webhook:
```
https://api.telegram.org/bot<ВАШ_BOT_TOKEN>/setWebhook?url=https://abc123.ngrok.io/api/telegrambot/webhook
```

## 6. Устранение проблем

### Бот не отвечает:
- Проверьте, что webhook настроен правильно
- Проверьте логи приложения на наличие ошибок
- Убедитесь, что BotToken правильный в `appsettings.json`

### Ошибка 404 при настройке webhook:
- Убедитесь, что приложение запущено и доступно
- Проверьте правильность URL (должен быть HTTPS)

### Бот не подтверждает регистрацию:
- Убедитесь, что Telegram ID в базе данных совпадает с ID пользователя в Telegram
- Проверьте, что пользователь не был подтвержден ранее (`IsConfirmed = false`)

