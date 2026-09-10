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

**[правка 2026-09-10, после рефакторинга — часть пунктов ниже устарела, см.
ЖУРНАЛ 2026-09-09/10]:** FireSystem больше НЕ пустой — владеет всей
стрельбой (игрок и враг) и AmmoState игрока; BulletSystem владеет
контейнером пуль; DuelSim — оркестратор+публикатор (реплей DuelPhaseChanged
удалён, подписчики слушают DuelFlow.PhaseChanged напрямую). Поражение тоже
ведёт в SHOP (запись (6) ниже актуальна, «NEXT → Game» тут устарело).
Цифры баланса в этом снапшоте НЕ истина: владелец крутит ассеты
Assets/Configs в ходе тестов. Появились: SimulationConfig (тикрейт/шаги/фпс,
ассет назначен на YgProjectLifetimeScope в Bootstrap.unity), MatchResultApplier,
GameplayLifecyclePresenter, BulletMovedSnapshot; враг стреляет теперь через
общий FireSystem.TryFire.

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

### 2026-09-10 (3) — агент: Claude Code — ВЕРДИКТ: миграция на Phaser (UNITY ЗАМОРОЖЕНА)
**Решение владельца:** перейти на Phaser 3 + TypeScript + Vite (сверено
с амбициями: веб-эксклюзив ЯИ, слабые мобилы, live-ops, агентная разработка).
**Сделано:**
- `MIGRATION_TO_PHASER.md` (корень) — ЕДИНЫЙ документ для агента-переносчика:
  вердикт, канон решений владельца, полная ГДД-выжимка с числами,
  архитектура Phaser-проекта (детерминированная симуляция 1/60 без Phaser
  внутри + вьюхи), карта переноса C#→TS файл-в-файл, план YG SDK, фазы
  M0–M6 с DoD, правила кода на TS, эталонные тесты симуляции, риски.
- Unity-версия ЗАМОРОЖЕНА на рабочем коммите `cc38ddc` (проверен компиляцией
  и запуском 2022.3.62f2 на этой машине).
- Недоведённый пиксель-арт сохранён в ветке `unity-pixelart-wip` (референс
  арт-направления для Phaser-агента: PixelArtFactory.cs — палитры/формы).
**ВАЖНО для следующего агента:** новые фичи в Unity НЕ делать. Фазу игры
брать из MIGRATION_TO_PHASER.md раздела 6 (M0→M6). Эталон поведения —
C#-симуляция в Assets/Scripts/Core/Simulation этого репо + тест-кейсы
раздела 8.2 документа.

### 2026-09-10 (2) — агент: opencode — фикс: VContainerException на бутстрапе
**Баг (лог владельца):** `VContainerException: Bootstrapper is not in this
scene DontDestroyOnLoad` → билд project-скоупа падал → все [Inject]
Bootstrapper null (`sceneLoader=False platform=False`) → NRE в Start.
**Причина:** рефакторинг (запись 2026-09-09/10) перенёс DDOL в
`ProjectLifetimeScope.Awake()`, но ПОСТАВИЛ его ДО `base.Awake()`. VContainer
внутри `base.Awake()` захватывает `gameObject.scene` и строит контейнер —
после DDOL сценой объекта была псевдо-сцена «DontDestroyOnLoad», и
`RegisterComponentInHierarchy<Bootstrapper>` искал компонент там, а не в
Bootstrap.
**Фикс:** `base.Awake()` → затем `DontDestroyOnLoad(gameObject)`. Интент
предыдущего агента сохранён (скоуп персистентен), порядок исправлен.
**Дальше:** ждать указаний.

### 2026-09-10 — агент: Claude Code — Ф5-лайт: паттерны стрельбы + 3 врага + процедурные UI/скины/звуки (ЗАВЕРШЕНО)
**Итог:** 25+ новых файлов (скрипты+ассеты), ~20 изменены. После батчей
аудит нашёл 3 критических бага — исправлены сразу:
- AudioService регистрировался без DontDestroyOnLoad → умирал при первой
  смене сцены (все SFX после Bootstrap→Game кидали MissingReference) →
  добавлен .DontDestroyOnLoad().
- Дробовик не списывал патрон и не ставил КД → бесконечный автоогонь →
  ConsumeShot в пути Shotgun.
- Очередь игрока обрывалась после 1-го выстрела (CanFire false из-за
  полного КД оружия против интервала очереди 0.12с) → continuation-шоты
  очереди игнорируют КД и ставят Cooldown = BurstIntervalSeconds.
Плюс мелочи: BurstCount<=1 больше не даёт лишний выстрел, PelletCount<=0
— guard, NaN-безопасные клампы прогрессов, BurstQueueState поля → свойства,
FxPool guard до Start, удалён осиротевший Assets/Audio.meta.
**Задача владельца:** процедурно сгенерировать весь UI и скины; пофиксить баги;
настроить конфиги; пару врагов — пистолет/автомат + третий дробовик, у всех
разный стиль стрельбы (один выстрел / очередь / дробь); процедурные звуки.
**Дизайн-решения (принял сам, владелец заказал фичу целиком):**
- FirePattern: Single / Burst / Shotgun (Auto из Ф5 — НЕ сейчас).
- Оружие = 3 WeaponConfig-ассета: Пистолет (одиночный, 6 патронов),
  Автомат (очередь 3, 9 патронов), Дробовик (веер 6 дробин ×0.34 урона,
  4 патрона, тяжёлая отдача).
- Враги: EnemyTypeConfig SO (оружие + цвет + цвета стадий) ×3 в
  EnemyCatalog; тип врага = (уровень−1) % 3 — предсказуемо для тестов.
- Игрок меняет оружие в МАГАЗИНЕ (новый экран «АРСЕНАЛ»: 3 карточки +
  «В БОЙ»), выбор живёт в WeaponLoadoutService (project scope, без сейвов —
  сейвы Ф7); GameLifetimeScope берёт оружие через PlayerWeaponProvider
  (каталог + лудаут + фолбэк с поля сцены).
- FireSystem: per-side WeaponParams (игрок/враг — теперь разные), очередь
  BurstState на сторону (тап во время очереди игнорируется, обрыв при
  пустом магазине → перезарядка), события PlayerAmmoChanged (B4!) и
  ShotFired. Патроны HUD: AmmoSnapshot (магазин/реген/перезарядка).
- Процедурные скины: SpriteFactory.GetGunSprite(FirePattern) — силуэты
  пистолета/автомата/дробовика из прямоугольников (белые, тинтуются вью).
  Пули цветные по владельцу (жёлтые/красные).
- Процедурный звук: ProceduralSoundFactory синтезирует AudioClip в коде
  (22050 Hz PCM): выстрелы ×3, попадание, взрыв, пробитие стадии,
  перезарядка, победа/поражение джинглы, UI-клик. AudioService (project
  scope, 3 AudioSource round-robin) + AudioConfig SO (громкости).
- Процедурный UI: HudView (уровень, монеты, пипки патронов, стадии врага,
  «ПРИГОТОВЬСЯ»), ShopWeaponsView («АРСЕНАЛ»), FxPool+FlashFxView (вспышки
  выстрела/попадания/взрыва — закрыли долг B1 «взрыв/попадание — только
  Debug.Log»).
- Экономика: EconomyService.Money + AddMoney; награда за победу реально
  начисляется (дисплей только; сейвы Ф7).
- Багфикс: стены арены рисуются ВНУТРИ края плейфилда (F77 — были за
  фрустумом камеры и не видны).
**Структура:** батчи A (Core) → B (Presentation) → C (ассеты+сцены YAML) →
аудит, тем же паттерном что и рефакторинг (последовательно, общее дерево).
**Мысли:** Auto-паттерн и траектории (синус/гоминг) — следующий шаг Ф5,
когда владелец попросит. Сейвы лудаута — Ф7.

### 2026-09-09/10 — агент: Claude Code (Claude Code CLI) — ревью + рефакторинг по SOLID (ЗАВЕРШЕНО)
**Итог: 36 файлов изменено (+556/−335), 12 новых, сцены Bootstrap дополнена.**
Полный список решений и отклонённых находок — в записи выше (разделы
«Ревью завершено», «Решения владельца», «Рефакторинг», «Отклонённые находки»).

**Исправленные баги (все подтверждены владельцем):**
- Взрыв пуля×пуля домаживал того, кто ДАЛЬШЕ (флаги цели инвертированы) →
  теперь домажит того, кто в радиусе (оба в радиусе — оба, как в дизайне).
- Мёртвая пуля больше не цепляет взрывы в том же тике (был двойной урон).
- GunHit/ExplosionHappened больше не репаблишатся после конца боя.
- ИИ: смена стадии и старт боя больше не дают мгновенного выстрела
  (визибилити-гистерезис не сбрасывается; первое появление считается
  только после 0.3с реальной невидимости).
- SceneLoader: гвард от двойной загрузки + блок кликов на фейде +
  unscaled-время (фейд не замрёт при паузе).
- Boot: таймаут 10с на YG SDK с LogError; fail-fast при отсутствии
  EconomyConfig; fail-fast Parent==null в игровых скоупах.
- Сим-аккумулятор clamp вместо сброса (при лагах время не теряется).
- Инпут: [DefaultExecutionOrder(-100)] — нажатие сэмплируется до сим-тика.
- Взрыв: радиус поражения учитывает радиус пушки.

**Структура (B5 + SOLID, поведение то же):**
- FireSystem владеет стрельбой и AmmoState игрока; TryFire(gun, isPlayer)
  — один код выстрела на обе стороны; PlayerAmmo наружу для будущего HUD (B4).
- BulletSystem владеет контейнером пуль, спавном (muzzle offset из
  WeaponConfig.BulletSpawnOffsetUnits) и per-tick diff-списками
  (Spawned/Moved/Removed) вместо WasAlive; пре-варм 64 состояний (zero-GC).
- DuelSim 380 → ~280 строк: только оркестрация и публикация; батчи
  событий очищаются в конце PublishEvents.
- DuelFlow владеет fight-clock (FightElapsedSeconds) — i-frames считают по нему.
- MatchResultApplier (Core/Meta): награда/уровень больше не во вью;
  DuelResultOverlayView только рендерит.
- GameplayLifecyclePresenter: GameplayStart/Stop выделен из ViewBinder;
  ViewBinder — только вью и подписки.
- DI: WeaponParams registered as instance; ExplosionSystem/MovementSystem/
  BulletSystem/FireSystem — конфиги и коллабораторов через конструктор;
  SpriteFactory → Singleton в project scope (текстуры больше не текут при
  перезагрузках Game); DDOL перенесён в ProjectLifetimeScope.Awake;
  SimulationDebugOverlay под #if UNITY_EDITOR (не попадает в WebGL-билд).
- SimulationConfig SO (60 тиков / 3 шага / 60 fps) — ассет назначен на
  YgProjectLifetimeScope в Bootstrap.unity (проведено в YAML, вручную
  ничего назначать не надо).
- Конфиги: WeaponConfig.BulletSpawnOffsetUnits (0.05), MapConfig.EnemySpawnRotationDegrees
  (270) — ассеты дополнены; в ассеты дописаны ранее несериализованные
  _gracePeriodSeconds: 1 и _visibilityDebounceSeconds: 0.3.
- Переименования: IsTargetPlayer / IsPlayerOwned (больше не путаются флаги).

**Ждёт владельца (ручные шаги):**
1. Открыть Unity, дождаться импорта/компиляции, запустить Bootstrap:
   выстрелы, взрывы (теперь домажит того, кто в радиусе!), стадии врага
   (без мгновенных выстрелов после пробития), повторные клики по кнопкам.
2. Unity-компиляцию я проверить не мог (нет редактора в этой среде) —
   если что-то не соберётся, писать мне.
3. Player Settings → Company Name (сейчас DefaultCompany).
4. `.claude/skills/` — 7 официальных Unity-скиллов; решить, коммитить ли
   в репо или убрать.

**Дальше:** ждать указаний; по плану PHASES_README — B1/B4 (FX + HUD-событие
патронов; PlayerAmmo уже проброшен наружу через FireSystem) → Ф4 парирование.

### 2026-09-09 — агент: Claude Code (Claude Code CLI) — ревью + рефакторинг по SOLID (В ПРОЦЕССЕ)
**Задача владельца:** тщательное ревью и рефакторинг, чтобы всё было по SOLID и
по AI_RULES. Логику не менять без явного вопроса владельцу. Лимит саб-агентов
20–30 на всю сессию. Мысли по ходу — вести в этом файле.
**Сделано:**
- Установлены официальные Unity-скиллы (github.com/Unity-Technologies/skills)
  в `.claude/skills/`: optimize-web, ui-ugui, optimize-text-mesh-pro,
  unity-cli, manage-sprite-atlas, unity-package-management, build-live-game.
  В git пока НЕ закоммичены — владелец решает, оставлять ли в репо.
- Перечитаны AI_RULES.md, PHASES_README.txt, этот файл, DuelSim.cs.
**План:**
1. Workflow-ревью (6 ревьюеров: кодстайл AI_RULES / SOLID-SRP / баги /
   GC-перф / VContainer+UniTask / монтировка сцен-ассетов-asmdef) →
   кластерная адверсариальная верификация каждой находки.
2. Вопросы владельцу по пунктам, где правка меняет логику.
3. Рефакторинг workflow'ом: только стиль/структура (правки логики —
   только после ответа владельца).
4. Контр-ревью после правок + чек-лист AI_RULES + итоговая запись здесь.
**Мысли:** план PHASES_README B5 (вынос стрельбы из DuelSim → FireSystem)
сам по себе меняет структуру, но НЕ поведение — попадает в рефакторинг,
но подтвержжу у владельца вместе с остальными вопросами.
**Ревью завершено (workflow, 4 линзы + добивочная верификация): 75 подтверждённых
находок. Ключевые:**
- CRITICAL: ExplosionSystem передаёт флаги цели наоборот — взрыв у игрока
  домажит врага и наоборот. Когда в радиусе оба — выглядит правильно
  («домажит обоих»), поэтому не замечали.
- Мёртвые пули цепляют взрывы в том же тике (двойной урон).
- После Victory/Defeat GunHit/Explosion репаблишатся каждый тик.
- Смена стадии врага сбрасывает визибилити-гистерезис ИИ → мгновенный
  выстрел после каждой стадии и в начале боя (владелец подтвердил: баг).
- SceneLoader без гварда от повторного входа; boot без таймаута SDK;
  сим-аккумулятор сбрасывает остаток при лагах (бой растягивается).
- Codebase в целом чистый по AI_RULES; главный структурный candidate — B5.
**Решения владельца (2026-09-09):**
1. Взрыв — «домажит обоих в радиусе» = чинить инверсию флагов (фикс даёт
   ровно это поведение).
2. Оба бага событий симуляции — чинить.
3. Стадийный сброс ИИ — баг, чинить (гистерезис не сбрасывать; старт
   боя не считать «появлением»).
4. Баланс: ассеты = живая правда, владелец крутит их в тестах; цифры
   доков не истина — НЕ выравнивать ассеты под документацию.
5. «Все фикси» robustness: SceneLoader гвард + unscaled фейд, boot
   таймаут SDK + fail-fast, clamp аккумулятора, радиус взрыва + пушки.
6. Сим-цикл ОСТАЁТСЯ на MonoBehaviour.Update (проще, для WebGL надёжнее).
   Правило AI_RULES «тик систем через UniTask» — санкционированное
   исключение для SimulationDriver.
7. «Делай как можно проще, но со вкусом» — без спекулятивных абстракций.
**Рефакторинг (workflow, 4 последовательных батча + аудит):**
- Батч 1: баги симуляции (инверсия взрыва, цепные взрывы мёртвых пуль,
  гистерезис ИИ, clamp аккумулятора, [DefaultExecutionOrder] инпута) +
  переименования IsTargetPlayer/IsPlayerOwned.
- Батч 2 (B5): FireSystem владеет стрельбой (общая для игрока/врага),
  BulletSystem владеет контейнером пуль + diff-списки вместо WasAlive,
  DuelSim — чистый оркестратор; WeaponParams/ArenaState через DI;
  fight-clock в DuelFlow; релей DuelPhaseChanged удалён.
- Батч 3: MatchResultApplier (деньги/уровень из вью), GameplayLifecyclePresenter,
  SceneLoader гвард, boot таймаут/fail-fast, SpriteFactory Singleton,
  DDOL в скоуп, fail-fast Parent==null, дебаг-оверлей под #if UNITY_EDITOR.
- Батч 4: SimulationConfig SO (тикрейт/шаги/фпс), поля конфигов вместо
  магических чисел (muzzle offset, поворот врага), константы эпсилонов,
  sqrMagnitude, пре-варм пуль, чистка логов Core, YAML-доводка ассетов.
- Аудит: адверсариальная проверка диффа против AI_RULES + графов DI.
**Отклонённые находки ревью (с причинами, чтобы не всплывали снова):**
- Удалить WeaponParams — НЕТ (роадмап Ф5/Ф7 прямо расширяет его).
- Удалить ISceneLoader/IBulletPool — НЕТ (роадмап добавляет порты
  ISaveService/IAudioService/IAdsService; паттерн портов sanctioned).
- Удалить ProgressService.LevelChanged — НЕТ (подписчик придёт с HUD, B4/F6).
- Перенести PlatformYG в Presentation-asmdef — НЕВОЗМОЖНО: у PluginYG2 нет
  asmdef, его код в Assembly-CSharp, из asmdef на него не сослаться.
  Штатная интеграция PluginYG2 — адаптеры живут в Assembly-CSharp.
- EditorTools asmdef пустой — оставить (скелет Ф0).
- GunHealthState/TickRateMeter/VisualConfig/grace→DuelConfig — отложено
  (принцип владельца «как можно проще»; цвета — плейсхолдеры до арт-паса).
- Стены арены за фрустумом камеры (орто 5, стены на ±5.125) — заметка
  для арт-паса, не трогали.
- companyName DefaultCompany — ручной шаг владельца в Player Settings.
**Наблюдения по собственному чтению кода (все 50+ файлов Assets/Scripts):**
- Кодстайл проекта в целом соблюдает AI_RULES очень чисто (нет var,
  нет комментариев, скобки с новой строки, `== false`, sealed, именованные
  обработчики событий, SO — чистые данные, нет статики кроме const).
- Сцен-виринг здоров: все 7 конфиг-ассетов существуют и подключены
  (Bootstrap: Economy Config на YgProjectLifetimeScope; Game: Map/Pistol/
  Bullet Explosion/Physics/Player/Enemy на GameLifetimeScope).
  SceneSwapDebugView из Game.unity владелец удалил (GUID в Game=0, в Shop=1) —
  пункты «Ждёт владельца» из записи 2026-09-08 (5) закрыты.
- Кандидаты в вопросы владельцу (меняют логику, без его слова не трогать):
  1) SceneLoader.Load без защиты от повторного входа — спам по кнопке
     SHOP/свап = двойная загрузка сцены (фикс — гвард, меняет поведение).
  2) AiShooterSystem.OnStageChanged сбрасывает визибилити-гистерезис →
     после каждого пробития стадии следующий видимый тик считается
     «появлением» и режет КД до ReactionSeconds (враг стреляет быстрее
     после смены стадии). Дизайн или побочка?
  3) ExplosionSystem взрывает ЛЮБЫЕ пары пуль, включая свои-свои (известный
     вопрос №4) + CheckDeaths при одновременной смерти обоих отдаёт Victory.
  4) Враг стреляет _weaponParams ИГРОКА (общий WeaponConfig на обоих) —
     по Ф5 разойдётся, сейчас это дизайн?
  5) B5-рефактор: fire-логика DuelSim → FireSystem + выделение
     bullet-spawner'а/even-publisher'а.
  6) DuelResultOverlayView считает награду и AdvanceLevel внутри вью —
     вынести в Meta-сервис/flow (структура, поведение то же).

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
