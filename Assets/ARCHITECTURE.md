# ARCHITECTURE — PISTOL PANIC

Архитектура проекта. Решения зафиксированы вопрос-ответом с владельцем.
Правила кода — в `AI_RULES.md`, дизайн — в `GDD_pistol_panic_v2.2.txt`.

Версия: 2 — интеграция Яндекс.Игр переведена на официальный Unity-плагин
**PluginYG2** (документация Яндекса ведёт на него). Готовые модули плагина
заменяют самописные сервисы: локализация, сейвы, реклама, лидерборды.

---

## 1. Decision Log (зафиксировано)

| Вопрос | Решение |
|---|---|
| DI | VContainer, обязательно. Никакой статики/синглтонов |
| Async | UniTask. Корутины запрещены |
| Сцены | 3: Bootstrap → Game ↔ Shop. После Boot в Game бой на паузе, старт по тапу |
| UI | uGUI (Canvas + TextMeshPro) |
| Ядро боя | Чистая C#-симуляция + тонкие MonoBehaviour-вью поверх |
| Тик симуляции | Фиксированный 60 Гц через аккумулятор, свой timeScale (слоу-мо) |
| Ввод | New Input System (Pointer + Press), мышь/тач одной схемой |
| Сборки | asmdef: Core / Presentation / Editor |
| Namespace | `PistolPanic.Core`, `PistolPanic.Meta`, `PistolPanic.Presentation` |
| Пули рендер | Пул GameObject-вью |
| Интеграция ЯИ | **PluginYG2** — официальный Unity-плагин Яндекс.Игр, модульная система |
| Локализация | **Модуль Localization плагина + AutoTranslateLangs** (свой сервис НЕ пишем) |
| Сейвы | **Модуль Storage плагина**: облако, неавторизованным — локальное хранилище ЯИ (свой SaveService НЕ пишем) |
| Реклама | **Модуль Advertisement плагина**: interstitial + rewarded; sticky — опция после релиза |
| Лидерборды | Модуль Leaderboards плагина — после релиза (нативно, без своего API) |
| SO | Только данные. Логики в SO нет вообще |
| Физика | Не используется. Ручные проверки: точка-круг, отрезок-круг (swept), конус+LOS |
| Рендер | **Built-in render pipeline** — самый простой, без URP/HDRP. Стандартные спрайты/шейдеры |
| Формат | Портрет 9:16, ortho-камера, Canvas Scaler 1080×1920 |

---

## 2. Сборки и слои

```
┌─────────────────────────────────────────────┐
│ Presentation (asmdef)                       │
│   VContainer scopes, вью, UI, ввод, пулы,   │
│   SimulationDriver, адаптеры PluginYG2      │
├─────────────────────────────────────────────┤
│ Core (asmdef)                               │
│   Симуляция дуэли (чистый C#)               │
│   Мета: экономика/перки/прогресс (чистый C#)│
│   Порты: ISaveService, IAdsService и т.д.   │
└─────────────────────────────────────────────┘
   Editor (asmdef) — утилиты редактора
   PluginYG2 (пакет) — сторонний, не трогаем внутри
```

- `Core` НЕ зависит от `Presentation` и от плагина. Про `YG`-неймспейс
  Core не знает вообще.
- `Core` может использовать `UnityEngine` только для структур/математики.
- Вью не содержат геймплейной логики.

### Исключения из правил статики (зафиксировано явно)

PluginYG2 — сторонняя инфраструктура, у него статический вход `YG2`.
Это НЕ наш код, правила AI_RULES на него не распространяются. Наши
правила работы с ним:

1. Доступ к `YG2.*` — ТОЛЬКО из адаптеров в `Presentation/Services/`.
   Вью, UI, Core напрямую плагин не зовут.
2. `SavesYG` (partial-класс плагина) — допускается: это точка расширения
  хранилища. В наших partial-файлах — только поля, без логики.
3. Компоненты `LanguageYG` (локализация текстов) вешаются на TMP-объекты
  UI — это использование плагина, не наш код.

---

## 3. Namespace-схема

| Сборка | Namespace | Что внутри |
|---|---|---|
| Core | `PistolPanic.Core` | Симуляция: `DuelSim`, системы, сущности, геометрия |
| Core | `PistolPanic.Meta` | Экономика, перки, инвентарь, прогресс, модель сейва |
| Presentation | `PistolPanic.Presentation` | Скоупы, вью, UI, ввод, пулы, адаптеры YG |
| Editor | `PistolPanic.EditorTools` | Editor-утилиты |

---

## 4. Сцены и VContainer

### Сцены

1. **Bootstrap** — index 0. `ProjectLifetimeScope`. Инициализация:
   ожидание `onGetSDKData` (сейвы плагина загружены) + язык применён →
   `LoadingAPI.ready()` (готовность игры для платформы) → загрузка Game.
2. **Game** — дуэль + меню-состояние + HUD + оверлеи. Бой не начинается
   сам: `FlowState.Armed` — пауза до первого тапа → Grace ~1с → Fight.
3. **Shop** — магазин. Переход из Game, возврат в Game.

### Скоупы

```text
ProjectLifetimeScope (Bootstrap, DontDestroyOnLoad)
  ├─ ISaveService        → YgSaveAdapter (Storage-модуль плагина)
  ├─ IAdsService         → YgAdsAdapter (Advertisement-модуль)
  ├─ IPlatformLifecycle  → YgLifecycleAdapter (ready, gameplayStart/Stop)
  ├─ IPlatformInfo       → YgInfoAdapter (deviceType, язык, браузер)
  ├─ ISceneLoader        → SceneLoader (UniTask, fade)
  ├─ IAudioService       → AudioService (пул AudioSource)
  ├─ EconomyService      (Meta)
  ├─ PerkService         (Meta)
  ├─ InventoryService    (Meta)
  └─ ProgressService     (Meta)

GameLifetimeScope (Game)
  ├─ конфиги боя (RegisterInstance из SO)
  ├─ DuelSim (Scoped)
  ├─ IBulletPool → BulletPool (GO-пул BulletView)
  ├─ SimulationDriver — тик 60 Гц + timeScale
  ├─ InputReader (New Input System) → IPlayerInput
  └─ ViewBinder — события симуляции ↔ пулы вью

ShopLifetimeScope (Shop)
  ├─ ShopFlowService (Scoped)
  └─ Shop UI-вью
```

Состояние между сценами живёт ТОЛЬКО в персистентных Meta-сервисах.

---

## 5. Интеграция Яндекс.Игр (PluginYG2)

### Используем (модули плагина)

| Модуль | Зачем | Как |
|---|---|---|
| **Localization** (+ AutoTranslateLangs) | Локализация RU/EN/TR | `YG2.lang` — язык платформы при старте (для ЯИ обязательно «при каждом запуске»); `YG2.SwitchLanguage()` + `onSwitchLang` — ручной выбор в настройках; компонент `LanguageYG` на TMP-текстах; автоперевод Google для EN/TR, шрифты по языкам. Свой LocalizationService НЕ делаем. Машинный перевод вычитать руками перед релизом |
| **Storage** | Сейвы (облако + фолбэк) | Поля в partial `SavesYG` (деньги, уровень, покупки, перки, язык, звук); запись через `YG2.SaveProgress()` — наш адаптер троттлит частоту; загрузка автоматом при старте, ждём `onGetSDKData`. `SaveCloud` включён: неавторизованные сохраняются в локальное хранилище ЯИ сами. `PlayerStats`/`RedefinePlayerPrefs` — НЕ используем |
| **Advertisement** | Монетизация | Interstitial — переходы уровней (платформа сама капит частоту, наш `AdsConfig`-интервал ≤ платформенного); Rewarded — возрождение (колбэки плагина → UniTaskCompletionSource); Sticky-баннеры — опция после релиза |
| **Game Events** | Требование модерации | `LoadingAPI.ready()` — после готовности Bootstrap; `GameplayAPI.start()/stop()` — старт/пауза боя и оверлеи. Обязателен по требованиям ЯИ |
| **EnvirData** | Окружение | `deviceType` (desktop/touch) — подсказки ввода и UX; язык для Localization |
| **Metrics** | Аналитика | Из коробки, Яндекс Метрика |
| **Leaderboards** | Лидерборд по макс. уровню | После релиза; данные уже в `ProgressService` |

### Опции на потом (не MVP, порты не плодим)

- **Payments** (инап-покупки) — монетизация скинов деньгами ЯИ
- **Flags** (удалённая конфигурация из консоли) — удалённые тумблеры
  поверх `AdsConfig`/баланса
- **Review / Shortcut** — оценки и ярлык на рабочий стол

### Требования платформы (учтено в дизайне)

- Продолжение после перезагрузки страницы — сейв на старте дуэли (есть)
- Реклама обязательна к подключению (п. 1.12) — есть
- Перевод хотя бы на один язык черновика, рекомендация ru/en/tr — есть
- Ручной выбор языка — через `SwitchLanguage`, правило запоминания (п. 6.9)

---

## 6. Симуляция дуэли (Core)

### 6.1 Тик

- `SimulationDriver`: аккумулятор unscaledDeltaTime × `_simTimeScale`,
  шаг `1/60`, до 3 шагов/кадр, остаток сбрасывается (анти-спираль смерти).
- `DuelSim.Tick(float fixedDelta)` — единственная точка входа.
- Слоу-мо бафф: `sim.TimeScale`; вражеские системы — свой множитель.

### 6.2 Сущности (данные)

```text
GunState, BulletState, BuffCapsule, ArenaState — как в v1
```

### 6.3 Системы

| Система | Ответственность |
|---|---|
| `MovementSystem` | Инерция, отскоки, импульсы отдачи |
| `FireSystem` | Паттерны (single/burst/auto/shotgun), КД, спавн пуль |
| `AmmoSystem` | Магазин, реген, полная перезарядка |
| `BulletSystem` | Траектории, swept-коллизии, пуля-в-пулю, рикошеты |
| `DamageSystem` | Числовой HP, стадии, i-frames |
| `ParrySystem` | Буфер 200мс, радиус, анти-дабл-парри |
| `AiShooterSystem` | Рандом-КД, конус+LOS, скалирование |
| `BuffSystem` | Спавн/подбор/эффекты капсул |
| `DuelFlow` | FSM: Armed → Grace → Fight → Victory/Defeat |
| `ExplosionSystem` | Взрыв пуля-в-пулю: радиус, урон обоим |

События наружу — `event Action<T>` с read-only struct-снапшотами.

### 6.4 Порядок тика — как в v1 (ввод → ИИ → огонь → пули → движение → баффы → урон → флоу → снапшоты)

Пуля-в-пуля: O(n²) при n ≤ ~50; путь оптимизации — uniform grid (не сейчас).

---

## 7. Порты и адаптеры

Интерфейсы в Core, реализации (обёртки PluginYG2) в Presentation.

| Порт (Core) | Реализация (Presentation) |
|---|---|
| `ISaveService` | `YgSaveAdapter` — read/write полей `SavesYG`, `SaveProgress` с троттлингом (не чаще раза в N сек, очередь) |
| `IAdsService` | `YgAdsAdapter` — `ShowRewardedAsync() : UniTask<bool>` (мостик UniTaskCompletionSource), `ShowInterstitial()`, гейт-интервал из `AdsConfig` |
| `IPlatformLifecycle` | `YgLifecycleAdapter` — `MarkReady()`, `GameplayStart()`, `GameplayStop()` |
| `IPlatformInfo` | `YgInfoAdapter` — `DeviceType` (desktop/touch), текущий язык |
| `ISceneLoader` | `SceneLoader` (UniTask, fade) |
| `IAudioService` | `AudioService` (пул AudioSource, банк из `AudioConfig`) |
| `IBulletPool` / `IBuffCapsulePool` | GO-пулы |
| `IPlayerInput` | `InputReader` (New Input System) |

Формат сейва: плагин сам сериализует `SavesYG` в JSON (JsonUtility/
Newtonsoft по выбору в настройках плагина). Словари — через
`List<T>`, при расширении массивов сейв не ломается.

---

## 8. Флоу дуэли и цепочки

### FSM `DuelFlow`

```text
Armed (пауза, ждём тап) → Grace (~1с) → Fight → Victory | Defeat
Defeat → [rewarded: Revive + быстрая перезарядка] | [skip: след. уровень]
Victory → сейв → экран победы → Дальше / Магазин
```

### Цепочка «выстрел», «парирование», «победа» — как в v1,
в «победе» сейв идёт через `ISaveService` (YgSaveAdapter → SaveProgress).

### Платформенные хуки (требование модерации)

- `IPlatformLifecycle.GameplayStart()` — Grace/Fight
- `IPlatformLifecycle.GameplayStop()` — Armed-пауза, оверлеи, сцена Shop

---

## 9. Мета (PistolPanic.Meta)

- `ProgressService` — текущий/максимальный уровень, элита каждые 10.
- `EconomyService` — деньги, награда по формуле из `EconomyConfig`.
- `InventoryService` — купленное, экипировка (одно на сессию).
- `PerkService` — ранги, цены, КАП эффекта; множители применяются к
  `WeaponParams` при создании дуэли (одна точка слияния база+перки).
- `ShopFlowService` — цена/списание/выдача/экипировка.
- Модель сейва = поля partial `SavesYG` (версия сейва, деньги, уровень,
  купленное, экипировка, ранги перков, язык, звук). Мета-сервисы владеют
  логикой, `SavesYG` — только хранилище полей.
- Сейв-каданс: на старте каждой дуэли (ГДД) + троттлинг адаптера.

---

## 10. Данные: SO-конфиги — как в v1 (Weapon/Trajectory/Player/Enemy/
Duel/Explosion/Buff/BuffSpawn/Economy/Perk/Map/Audio Config). Локализация
в конфигах НЕ хранится — тексты живут в UI + `LanguageYG`.

---

## 11. Структура папок

```
Assets/
  Scripts/
    Core/
      Simulation/
      Meta/
      Ports/
    Presentation/
      Bootstrap/        (ProjectLifetimeScope, Bootstrapper)
      Game/             (GameLifetimeScope, SimulationDriver, ViewBinder)
      Game/Views/       (GunView, BulletView, BuffCapsuleView, ArenaView)
      Game/Input/       (InputReader)
      Game/Pools/
      Shop/
      Services/         (YgSaveAdapter, YgAdsAdapter, YgLifecycleAdapter,
                         YgInfoAdapter, SceneLoader, AudioService)
      UI/
    Editor/
  Configs/
  Prefabs/
  Sprites/
  Scenes/               (Bootstrap, Game, Shop)
  Audio/
  SavesYG partial-файлы — в Scripts/Presentation/Services/Saves/
```

---

## 12. Производительность (WebGL, zero-GC в бою)

- Пулы: пули, капсулы, FX, AudioSources. Ноль `Instantiate` в бою.
- События — struct-снапшоты, ноль аллокаций на тик.
- Кэш-массивы — инстансные `readonly` (наш код; плагин — его правила).
- LINQ/строки/боксинг — вне боевого пути.
- Профилировать в WebGL-билде; настройка билда — по странице
  «Оптимизация и настройка» плагина.

---

## 13. Что НЕ делаем (YAGNI-границы)

- Свой LocalizationService, свой формат сейвов, свой рекламный слой —
  плагин закрывает
- ECS/DOTS, Addressables, Zenject — не тащим
- Payments/Flags/Review/Shortcut, sticky-баннеры, лидерборд — после
  релиза (порты не создаём заранее)
- Генератор карт, мультиплеер, туториал-сцена — нет в MVP

## 14. Фазы и шаги реализации

Правила прохода фаз:

- Фазы строго по порядку, каждая заканчивается работающим чекпоинтом
  (компилируется + играется в редакторе)
- Шаг внутри фазы — один коммит-размер: компилируется после каждого
- Конфиги (SO) создаются вместе со своей системой, не заранее пачкой
- Названия систем/портов — из этого документа, ничего не придумывать
- После каждой фазы — сверка с чек-листом AI_RULES (статика, корутины,
  логика в SO, аллокации)

### ФАЗА 0 — Каркас проекта

| # | Шаг |
|---|---|
| 0.1 | Unity 2022.3.62f2, 2D-шаблон, **рендер Built-in (URP не ставить)**. Player settings: портрет 9:16, WebGL, Target Frame Rate 60 |
| 0.2 | Пакеты: VContainer, UniTask, Input System, PluginYG2 (+модули: Localization, AutoTranslateLangs, Storage, Advertisement, Game Events) |
| 0.3 | asmdef: `PistolPanic.Core`, `PistolPanic.Presentation` (ref Core), `PistolPanic.EditorTools`. Папки по разделу 11 |
| 0.4 | Сцены Bootstrap/Game/Shop в Build Settings (порядок 0/1/2). `AI_RULES.md` в корень проекта |
| 0.5 | `ProjectLifetimeScope` + Bootstrapper: ожидание `onGetSDKData` → `LoadingAPI.ready()` → загрузка Game. Заглушки `GameLifetimeScope`/`ShopLifetimeScope` |
| 0.6 | `ISceneLoader` → `SceneLoader` (UniTask, fade-канвас) |
| 0.7 | Камера ortho + Canvas Scaler 1080×1920, референс-арена 9:16 |

**Чекпоинт:** Boot → Game (пустая), переход Game ↔ Shop и обратно, ready()
платформе отправляется.

### ФАЗА 1 — Каркас симуляции

| # | Шаг |
|---|---|
| 1.1 | Core: `DuelSim` + `SimulationDriver` (аккумулятор, шаг 1/60, max 3 шага/кадр, `_simTimeScale`) |
| 1.2 | Сущности: `GunState`, `ArenaState`. `MapConfig` SO + `ArenaView` (рисует стены/препятствия из конфига) |
| 1.3 | `MovementSystem`: инерция, отскок от стен/препятствий, угловая скорость |
| 1.4 | `GunView` + `ViewBinder`: снапшоты симуляции → позиция/поворот вью |
| 1.5 | `InputReader` (NIS: Pointer Position + Press) → порт `IPlayerInput` в Core |

**Чекпоинт:** пушка дрейфует по арене, отскакивает, целится за курсором,
тикает 60 Гц (видно в дебаг-оверлее).

### ФАЗА 2 — Стрельба и отдача

| # | Шаг |
|---|---|
| 2.1 | `WeaponConfig` SO (MVP: одиночный паттерн) + `WeaponParams` (структура финальных статов) |
| 2.2 | `FireSystem`: КД, попытка выстрела по нажатию. `AmmoSystem`: магазин, реген по 1, полная перезарядка при пустом |
| 2.3 | Отдача: импульс в `MovementSystem` + визуальный флип `GunView` |
| 2.4 | `BulletSystem`: движение, swept «отрезок vs круг», смерть о стену/препятствие |
| 2.5 | `IBulletPool` → `BulletPool` (GO-пул `BulletView`), снапшот `BulletSpawnedSnapshot` → вью |
| 2.6 | Пуля-в-пуля → `ExplosionSystem` (радиус/урон из `ExplosionConfig`) |
| 2.7 | `IAudioService` → `AudioService` (пул AudioSource). SFX: выстрел, взрыв. FX: вспышка, шейк камеры |

**Чекпоинт:** стреляешь — отлетает, пули летят и умирают, две пули
встречаются — взрыв. Враг-манекен ловит урон.

### ФАЗА 3 — Враг и полный цикл дуэли

| # | Шаг |
|---|---|
| 3.1 | `DamageSystem`: HP игрока 3.0 + i-frames 0.6с; HP врага стадиями из `EnemyConfig`, смена стадии → событие |
| 3.2 | Стадии врага визуально: 3 спрайта, звук разрушения, партиклы |
| 3.3 | `AiShooterSystem`: рандом-КД, конус зрения + LOS (препятствия гасят), мгновенный выстрел при видимости |
| 3.4 | `DuelFlow` FSM: Armed (бой на паузе до тапа) → Grace ~1с → Fight → Victory/Defeat → следующий уровень |
| 3.5 | Спавн по `MapConfig`: противоположные концы, случайный импульс + вращение |
| 3.6 | Оверлеи победы/поражения (uGUI), перезапуск дуэли на новый уровень |
| 3.7 | `IPlatformLifecycle`: `GameplayStart/Stop` на состояниях FSM |

**Чекпоинт:** полный цикл 1v1 против ИИ с манекенным оружием, победа/
поражение, следующий уровень, пауза-до-тапа.

### ФАЗА 4 — Парирование

| # | Шаг |
|---|---|
| 4.1 | `ParrySystem`: буфер ввода 200мс (`PlayerConfig`), проверка радиуса на тике, анти-дабл-парри ~100мс. Константа «нажал = выстрел» не нарушена |
| 4.2 | Парированная пуля: скорость ×N, рикошеты 2–3 от стен/препятствий, общий счётчик |
| 4.3 | FX парри: слоу-мо-вспышка, звуковой акцент |
| 4.4 | Дебаг-визуализация зоны парирования (gizmo/оверлей) |

**Чекпоинт:** тайминговое парирование работает, парированная пуля носится
по арене и добивает врага.

### ФАЗА 5 — Паттерны и траектории

| # | Шаг |
|---|---|
| 5.1 | `FireSystem`: очередь (тап = полная очередь, не рвётся, обрыв при пустом магазине) |
| 5.2 | `FireSystem`: зажим (удержание), дробь (веер 5–7). Отдача по паттернам |
| 5.3 | `BulletSystem`: синусоида (амплитуда/частота), разгоняющаяся (старт/ускорение) |
| 5.4 | Гоминг: радиус захвата, сила поворота; цель: враг для игрока, игрок для врага; перехватывается чужими пулями; при парри — смена цели на стрелка |
| 5.5 | `EnemyConfig`: таблица выдачи паттернов по уровням, скалирование КД/реакции, элита каждые 10 (×HP, ×КД, уникальный спрайт) |
| 5.6 | Набор `WeaponConfig`-ов: по одному на каждый паттерн × траектория (тестовые ассеты) |

**Чекпоинт:** всё оружие канона 4.3.1/4.4.2 стреляет и ощущается по-разному,
сложность растёт по уровням, элита на 10-м.

### ФАЗА 6 — Баффы

| # | Шаг |
|---|---|
| 6.1 | `BuffSystem` + `BuffCapsulePool` + `BuffCapsuleView`: дрейф капсулы, спавн по `BuffSpawnConfig`, подбор касанием (только игрок) |
| 6.2 | Бафф слоу-мо: `sim.TimeScale` + вражеский множитель |
| 6.3 | Бафф быстрой перезарядки (таймер поверх `AmmoSystem`) |
| 6.4 | Бафф AoE по всей арене (урон только врагу, сквозь препятствия) |
| 6.5 | Бафф миниган: автострельба без патронов, рандомный сектор, полная отдача |
| 6.6 | Индикация активных баффов в HUD |

**Чекпоинт:** все 4 баффа подбираются и работают, миниган — хаос.

### ФАЗА 7 — Мета и магазин

| # | Шаг |
|---|---|
| 7.1 | `ProgressService`, `EconomyService` (награда 25+ур×2 из `EconomyConfig`) |
| 7.2 | `SavesYG` partial-поля (версия, деньги, уровень, покупки, экипировка, перки, настройки) + `YgSaveAdapter` (троттлинг) |
| 7.3 | Сейв-каданс: старт каждой дуэли + после покупок |
| 7.4 | `InventoryService` + экипировка (одно на сессию, смена вне боя) |
| 7.5 | `PerkService`: 5 перков, ранги, растущие цены, КАПы; точка слияния база+перки → `WeaponParams` |
| 7.6 | Сцена Shop: вкладки Оружие/Скиллы/Скины, `ShopFlowService` (цена/списание/выдача), вход из меню и с экрана победы |
| 7.7 | HUD: патроны/реген, номер уровня, баффы. Экран победы: деньги + «Дальше» + «Магазин» |
| 7.8 | 5–6 `MapConfig` карт (ручная сборка) + ротация без повторов подряд |

**Чекпоинт:** полный цикл: дуэль → деньги → магазин → экипировка/перки →
дуэль; после перезагрузки страницы — продолжение с уровня.

### ФАЗА 8 — Платформа, локализация, релиз

| # | Шаг |
|---|---|
| 8.1 | `YgAdsAdapter`: rewarded-возрождение (кап 1/бой, меню поражения: возродить + быстрая перезарядка / продолжить без награды), interstitial на переходах с гейт-интервалом из `AdsConfig` |
| 8.2 | `YgInfoAdapter`: deviceType → подсказки ввода |
| 8.3 | Локализация: `LanguageYG` на все TMP-тексты, автоперевод EN/TR, ручной выбор языка в настройках (`SwitchLanguage`), шрифты по языкам |
| 8.4 | Фоновая музыка (1 трек) + полный SFX-сет из ГДД |
| 8.5 | WebGL-сборка: атласы спрайтов, аудио-компрессия, настройки плагина по его странице оптимизации; прогон в браузере + debug-панель ЯИ |
| 8.6 | Балансный тюнинг по конфигам (окно парри, скорости, урон/HP, экономика) — плейтест-цифры из ГДД 12.4 |
| 8.7 | Черновик в Консоли ЯИ, галки: облако сейвов, языки ru/en/tr, тестирование → модерация |

**Чекпоинт:** билд в Яндекс.Играх, реклама/сейвы/локализация работают на
платформе, игра прошла модерацию.

### После релиза (не MVP)

- Лидерборд (модуль Leaderboards, `ProgressService.MaxLevel`)
- Sticky-баннеры, Payments (инапы для скинов), Flags (удалённые тумблеры)
- Обучение парированию в первых дуэлях (если плейтесты попросят)

================================================================================
Конец документа. Порядок фаз = порядок работы. Начало — ФАЗА 0.

