# AI_CONTEXT.md — живой контекст проекта PISTOL PANIC

## Назначение

Файл для агентов (и человека): сюда записываются мысли, планы, итоги
сессий и тех. долг по ходу разработки. Это память проекта между сессиями.

## Правила ведения (для всех агентов)

1. **Перед сессией:** перечитать этот файл + `AI_RULES.md` (стиль кода)
   + `PHASES_README.txt` (план фаз и текущее место в нём).
2. **После сессии:** дописать запись в ЖУРНАЛ СЕССИЙ (формат ниже).
   Ничего не удалять из чужих записей — исправления помечать `[правка]`.
3. Новый факт о коде, который ломает ожидания — фиксировать в
   «Тех. долг / наблюдения» сразу, не держать в голове.
4. Записи в журнале: дата, агент, сделано / не сделано / дальше / мысли.

## Документы-источники правды

| Файл | Что содержит |
|---|---|
| `AI_RULES.md` (корень) | Кодстайл, запреты, чек-лист перед отдачей |
| `PHASES_README.txt` (корень) | Статус фаз, оставшиеся шаги, что где прокинуть |
| `Desktop/GDD_pistol_panic_v2.2.txt` | Гейм-дизайн (перенести в репо в `Docs/`) |
| `Desktop/ARCHITECTURE.md` | Архитектура + фазы 0–8 (перенести в репо в `Docs/`) |

Стек: Unity 2022.3.62f2, Built-in render, 2D портрет 9:16, WebGL →
Yandex Games, VContainer + UniTask + New Input System + PluginYG2
(модули: Localization, AutoTranslateLangs, Storage, InterstitialAdv,
RewardedAdv, Leaderboards, Metrica, Authorization). Без физдвижка.

---

## СНИМОК СОСТОЯНИЯ (полное чтение кода, 53 скрипта, коммит `2962cd9`)

### Сцены и скоупы
- `Bootstrap` → `YgProjectLifetimeScope` (наследник `ProjectLifetimeScope`,
  биндит `IPlatformLifecycle → YgLifecycleAdapter`, EconomyConfig →
  `ProgressService` + `EconomyService` Singleton'ы project-скоупа,
  переживают перезагрузки сцен) + `Bootstrapper`:
  ждёт `YG2.isSDKEnabled` → `MarkReady()` (GameReadyAPI) → `SceneLoader.Load(Game)`.
  targetFrameRate=60, DontDestroyOnLoad на project scope.
- `Game` → `GameLifetimeScope`: 6 конфигов полями сцены (Map/Weapon/
  Explosion/Physics/Player/Enemy), регистрирует все системы + DuelSim
  (Scoped), компоненты через RegisterComponentOnNewGameObject +
  RegisterBuildCallback (SimulationDriver, InputReader→IPlayerInput,
  ArenaView, BulletPool→IBulletPool, ViewBinder, DuelResultOverlayView,
  SimulationDebugOverlay). `SceneSwapDebugView` — RegisterComponentInHierarchy.
- `Shop` → `ShopLifetimeScope`: только SceneSwapDebugView (заглушка).

### Симуляция (Core/Simulation)
- `SimulationDriver`: аккумулятор unscaledDeltaTime × `_simTimeScale`
  (0..∞, свойство TimeScale), шаг 1/60, max 3 шага/кадр, сброс остатка.
  Публично `TicksPerSecond` (для дебаг-оверлея).
- `DuelSim` — оркестратор: тик-порядок: ConsumePress → Armed-тап →
  DuelFlow.Tick → (если идёт симуляция) Fire → AI → Bullets → Movement →
  Explosions → ApplyDamage → CheckDeaths → PublishEvents.
- `MovementSystem`: дампинг линейный/угловой (PhysicsConfig), инерция,
  отскоки от стен и круговых препятствий, `ApplyRecoil` (импульс + рандом
  торк), `ResolveGunPairCollision` (расталкивание пушек).
- `AmmoSystem/AmmoState`: КД, реген по 1 через RegenDelay, полная
  перезарядка при пустом магазине (CanFire/ConsumeShot).
- `FireSystem` — ПУСТАЯ ОБОЛОЧКА (только вызывает AmmoSystem.Tick).
  Логика выстрела игрока — вручную в `DuelSim.TryFirePlayerGun`,
  врага — в `DuelSim.TickEnemyAi` (только одиночный паттерн).
- `BulletSystem`: прямые пули, swept «отрезок vs круг» (CollisionMath),
  смерть о стену/препятствие/пушку (свой пуля не бьёт владельца).
  Нет: траекторий, парри, рикошетов (полей в BulletState нет).
- `ExplosionSystem`: O(n²) пуля-пуля (ЛЮБЫЕ пары, включая свои-свои),
  взрыв → урон обеим сторонам в радиусе (ExplosionConfig 0.85 / 1.0).
- `DamageSystem`: игрок HP 3.0 + i-frames 0.6с (по _fightElapsedSeconds),
  враг — 3 стадии HP (5/4/3) c переносом остатка урона в след. стадию.
- `AiShooterSystem`: ОДИН канал стрельбы — рандомный КД
  (FireCooldownMin/Max из EnemyConfig) стреляет всегда, в направлении
  текущего поворота ствола. Появление игрока в конусе зрения (100°) + LOS
  срезает текущий КД до ReactionSeconds. Защита от двойных выстрелов:
  1) гистерезис видимости — «появлением» считается только вход после
  VisibilityDebounceSeconds (0.3с) непрерывного отсутствия (фликер у края
  конуса не перезапускает реакцию); 2) срез не работает в течение
  ReactionSeconds после любого выстрела (выстрел прямо перед появлением
  не даёт второй вплотную). Авто-доводки врага НЕТ (решение владельца).
  Нет: скалирования от уровня, элиты, паттернов.
- `DuelFlow`: FSM Armed→Grace(1с)→Fight→Victory/Defeat. IsFireAllowed
  только в Fight. Revive-механики нет.
- Спавн: противоположные точки MapConfig, случайный импульс+вращение.

### Presentation
- `ViewBinder`: создаёт 2 GunView (квадраты, зелёный/красный) + подписки
  на все события DuelSim; BulletSpawned/Updated/Removed → IBulletPool;
  DuelPhaseChanged → IPlatformLifecycle (Armed=GameplayStop, остальное
  GameplayStart); EnemyStageChanged → SetColor по EnemyConfig.StageColors
  (цвета-заглушки вместо спрайтов стадий). Взрыв/попадание — только Debug.Log.
- `BulletPool` (сделан владельцем): Stack free + Dictionary id→view,
  Spawn/Move/Despawn, GO создаются лениво, Initialize через SpriteFactory.
- `SpriteFactory`: рантайм-генерация квадрата/круга — ЗАГЛУШКА под арт.
- `DuelResultOverlayView`: uGUI кодом (временный). Victory → награда
  «+N» (EconomyService по EconomyConfig, BaseReward 25 + PerLevel×2) →
  ProgressService.AdvanceLevel → кнопка SHOP → сцена Shop. Defeat →
  AdvanceLevel без денег → кнопка NEXT → сцена Game. Плеер-луп владельца:
  тап → Grace → Fight → Victory → Shop → новый матч.
- `Meta` (Core/Meta, namespace PistolPanic.Meta): ProgressService
  (счётчик уровня, Singleton, переживает сцены), EconomyService +
  EconomyConfig SO (создать ассет, повесить на YgProjectLifetimeScope).
- `SceneSwapDebugView` — ТОЛЬКО в сцене Shop (кнопка возврата в матч);
  из Game убран (по решению владельца кнопки сцены Game не существует).
- `InputReader`: только ConsumePress (Pointer.press). НЕТ aim-позиции
  курсора — прицеливания в игре НЕТ ПО ДИЗАЙНУ (управление только
  выстрелами).
- `SceneLoader`: fade-канвас (кодом) + EnqueueParent(project scope).
- `SimulationDebugOverlay`: OnGUI-лейбл тиков.

### Платформа
- `YgLifecycleAdapter`: WaitForDataLoadedAsync (poll isSDKEnabled),
  MarkReady=GameReadyAPI, GameplayStart/Stop. Планируется собственный
  `YgProjectLifetimeScope : ProjectLifetimeScope` c
  ConfigurePlatformServices — паттерн под будущие YG-адаптеры.

---

## ТЕХ. ДОЛГ / НАБЛЮДЕНИЯ (по факту чтения)

1. ~~Игрок не целится~~ **[решено владельцем 2026-09-08]: это дизайн, не
   баг.** Игрок НЕ целится никогда — управляет только выстрелами,
   направление ствола меняется только от физики (дрейф/отдача/торк).
   IPlayerInput aim-точку НЕ добавлять.
2. `FireSystem` пустой, логика стрельбы в `DuelSim` (гроздит) —
   рефакторить перед паттернами (PHASES_README B5).
3. `BulletState` без `IsParried/RicochetsLeft/Trajectory/HomingTarget` —
   парирование и траектории (Ф4/Ф5) упрутся в это.
4. Взрыв сталкивает ЛЮБЫЕ пары пуль, включая свои-свои: очереди (burst)
   будут взрывать сами себя. Проверить у владельца: только чужие пули
   сталкиваются? (По ГДД — «пуля в пулю» без уточнения.) Сейчас
   столкновение — swept «отрезок vs круг» обеих пуль (фикс туннелирования
   2026-09-08).
5. `SimulationDriver.TimeScale` — глобальный множитель (замедлит и
   игрока). Слоу-мо бафф по ГДД замедляет ТОЛЬКО врага — нужен
   вражеский множитель внутри DuelSim, а не глобальный тик.
6. ~~`SceneSwapDebugView` в Game-сцене~~ **[сделано 2026-09-08]:** из
   кода Game-скоупа убран; остался только в Shop как возврат в матч.
   Владельцу: удалить GO `SceneSwapDebugView` из Game-сцены в редакторе.
7. UI собирается кодом (DuelResultOverlay, SceneSwapDebug) — временно,
   до префабов uGUI.
8. `SpriteFactory` — заглушки вместо арт-спрайтов (квадраты/круги).
9. `Debug.Log` спам на каждый спавн/смерть пули — вычистить перед
   WebGL-профилем.
10. `EnemyConfig.StageColors` — заглушка; на смену — спрайты стадий +
    звук (Ф8).
11. Документы GDD/ARCHITECTURE лежат на рабочем столе — перенести в
    репо (`Docs/`), чтобы другой ПК имел всё из git.

---

## ПЛАН ДАЛЬШЕ (сверка с PHASES_README.txt)

Позиция: Ф0–Ф3 готовы, идём в раздел B (недоделки) → Ф4 (парирование).

Порядок:
1. **B1+B4** — FX выстрела/попадания/взрыва во ViewBinder, событие
   патронов для HUD (aim НЕ нужен — см. долг №1, решено).
2. **B5** — рефактор: логика выстрела из DuelSim → FireSystem.
3. **C (Ф4)** — ParrySystem + поля BulletState + рикошеты.
4. **D (Ф5)** — паттерны + траектории + скалирование/элита.

---

## ЖУРНАЛ СЕССИЙ

### 2026-09-08 (6) — агент: opencode — поражение через магазин + двойные выстрелы ИИ
**Сделано:**
- `DuelResultOverlayView`: поражение теперь тоже ведёт в МАГАЗИН
  (кнопка SHOP → сцена Shop), без денег, AdvanceLevel как раньше.
  Оба исхода теперь: оверлей → Shop → новый матч.
- **Двойные выстрелы ИИ — найдена причина.** Таймер один (двух нет),
  но событие «появление игрока» срабатывало повторно: 1) фликер
  видимости у края конуса/за препятствием — каждое «появление» снова
  резало КД до 0.2с → серия выстрелов; 2) случайный выстрел прямо перед
  появлением игрока + срез КД → два выстрела с зазором 0.2с.
  **Фикс:** EnemyConfig + `VisibilityDebounceSeconds` (0.3с) — «появлением»
  считается вход в зону только после 0.3с непрерывного отсутствия;
  срез КД подавляется в течение ReactionSeconds после любого выстрела.
  `_wasPlayerVisible/_invisibleSeconds/_sinceLastShotSeconds` — служебные,
  таймер стрельбы по-прежнему один.
**Дальше:** ждать указаний; кандидаты — B1/B4 (FX + HUD-событие патронов)
→ B5 → Ф4.

### 2026-09-08 (5) — агент: opencode — плеер-луп через магазин + деньги за уровень
**Сделано (решение владельца: кнопки перехода в бой быть не должно,
луп = тап → Grace → Fight → Victory(+деньги) → Shop → новый матч):**
- `Core/Meta/` (namespace `PistolPanic.Meta`): `EconomyConfig` (SO,
  BaseReward=25, RewardPerLevel=2), `ProgressService` (CurrentLevel с 1,
  AdvanceLevel, Singleton в project-скоупе — переживает сцены),
  `EconomyService.GetVictoryReward(level)`.
- `YgProjectLifetimeScope`: поле `EconomyConfig` + регистрация сервисов.
- `DuelResultOverlayView`: Victory → «+N» (золотая цифра) → AdvanceLevel →
  кнопка SHOP → сцена Shop; Defeat → без денег → AdvanceLevel → кнопка
  NEXT → сцена Game (по ГДД поражение = следующий уровень без награды).
- `GameLifetimeScope`: регистрация `SceneSwapDebugView` убрана.
**Ждёт владельца (ручные шаги в редакторе):**
1. Создать ассет EconomyConfig (Create → PistolPanic → EconomyConfig),
   повесить на YgProjectLifetimeScope в Bootstrap-сцене.
2. Удалить GO SceneSwapDebugView из Game-сцены (код больше его не
   инжектит — кнопка стала нерабочей и мусорной).
3. Проверить, что SceneSwapDebugView в Shop-сцене таргетит «Game».
**Дальше:** ждать указаний; следующие кандидаты — B1/B4 (FX + HUD-событие
патронов) → B5 → Ф4 парирование. Магазин-UI (Ф7) поверх SceneSwapDebugView.

### 2026-09-08 (4) — агент: opencode — фикс: враг стрелял два раза подряд
**Баг (по логам владельца):** пули 24→25 и 30→31 — выстрелы в соседних
тиках. Причина: реакция на видимость и рандомный КД были ДВУМЯ
независимыми таймерами — при срабатывании реакции КД продолжал тикать и,
если оставалось мало, стрелял через 1–2 тика после реакционного выстрела.
**Фикс:** каналы слиты в один. Видимость игрока не создаёт второй выстрел —
она срезает текущий `_randomCooldownSeconds` до `ReactionSeconds`, если тот
больше. Источник выстрела один (истечение КД) → подряд стрелять нельзя,
минимальный зазор между любыми двумя выстрелами = FireCooldownMinSeconds.
Состояния `_isReactionPending/_reactionTimerSeconds` удалены.
**Дальше:** ждать указаний.

### 2026-09-08 (3) — агент: opencode — фикс: реакция не трогает рандомный КД
**Сделано:** `AiShooterSystem.TryFireReaction` больше НЕ сбрасывает
рандомный КД после выстрела реакции (решение владельца: выстрел по КД и
выстрел по видимости — независимые события, реакция не прерывает цикл КД
между выстрелами). Рандомный КД тикает непрерывно и стреляет строго по
своему расписанию; выстрел реакции — дополнительный, поверх. Если оба
срабатывают в один тик — одна пуля (события мержатся), КД при этом
истёк естественно.
**Дальше:** ждать указаний; кандидаты — B1/B4 (FX + HUD-событие патронов)
→ B5 → Ф4.

### 2026-09-08 (2) — агент: opencode — ревью кода + фикс ИИ врага
**Сделано:**
- Переработан `AiShooterSystem`: теперь ДВА контура — рандомный КД
  (стреляет всегда, видимость не нужна) + реакция на появление игрока
  в конусе зрения (ReactionSeconds из EnemyConfig, потом выстрел).
  Убран старый баг: рандом-КД не стрелял, пока игрок невидим (КД
  подменялся интервалом проверки видимости).
- Убрана авто-доводка врага (`DuelSim.AimEnemyGunAtPlayer` удалён):
  враг стреляет в текущем направлении ствола, никого не доворачивается.
- `EnemyConfig`: + `ReactionSeconds` (0.2), убран
  `VisionCheckIntervalSeconds` (видимость проверяется каждый тик —
  дёшево). Рандомный КД = существующие FireCooldownMin/MaxSeconds.
- Фикс туннелирования пуль: взрыв пуля-в-пулю теперь swept — «отрезок
  за тик vs круг» обеих пуль (`BulletState.PreviousPosition`,
  `ExplosionSystem` через `CollisionMath`). Раньше сближающиеся пули
  проскакивали друг друга за тик (~26 u/s относительная при радиусах 0.15).
- `DuelSim.SpawnBullet` инициализирует `PreviousPosition`.

**Ревью прочего кода (без правок):** InputReader теряет нажатия при
2 нажатиях между тиками (не критично при 60Гц); клик по UI-кнопке =
выстрел в бою (кнопок в бою пока нет — отложить); Debug.Log спам пуль —
чистить перед WebGL-профилем; FireSystem пустой — рефактор B5 перед
паттернами.

**Дальше:** ждать указаний владельца. Кандидаты: B1/B4 (FX + HUD-событие
патронов) → B5 → Ф4 парирование.

### 2026-09-08 — агент: opencode (Claude/sonnet-подобная сессия)
**Сделано:** полное чтение проекта (53 скрипта, сцены, конфиги-ассеты,
биндинги скоупов); создан этот файл. Проверены: пул владельца
(BulletPool/IBulletPool/BulletView), ViewBinder, DuelSim, Bootstrapper,
Yg-адаптеры.
**Статус:** Ф0–Ф3 готовы (см. Снимок состояния). Пул работает: события
симуляции → ViewBinder → Stack/Dictionary пул, возврат по Despawn.
**Дальше:** порядок в разделе «План дальше»; первый шаг — прицеливание
игрока (долг №1), затем B-шаги → Ф4. [правка 2026-09-08 (2): aim отменён
владельцем — это дизайн; порядок см. выше]
**Мысли/вопросы владельцу:**
- Взрыв: свои пули должны сталкиваться друг с другом? (см. долг №4)
- Слоу-мо баффа: подтверди, что замедляем только врага (долг №5)
- Документы с рабочего стола перенести в `Docs/` репозитория?
