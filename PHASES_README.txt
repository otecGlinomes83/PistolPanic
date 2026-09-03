===============================================================================
PISTOL PANIC — PHASES_README
Статус проекта + оставшиеся фазы + что где прокинуть
================================================================================

Документы: GDD (Desktop/GDD_pistol_panic_v2.2.txt), архитектура
(Desktop/ARCHITECTURE.md), правила кода (AI_RULES.md в корне).
Правила прохода фаз — раздел 14 ARCHITECTURE.md: фазы по порядку,
один шаг = один коммит, конфиги вместе с системами, после каждой фазы
чек-лист AI_RULES.


================================================================================
РАЗДЕЛ A. ТЕКУЩЕЕ СОСТОЯНИЕ (сверено с кодом)
================================================================================

ГОТОВО:
  [x] Ф0 Каркас: пакеты (VContainer, UniTask, NIS, PluginYG2 + модули
      Localization/AutoTranslateLangs/Storage/InterstitialAdv/RewardedAdv/
      Leaderboards/Metrica/Authorization), asmdef Core/Presentation/
      EditorTools, 3 сцены (Bootstrap/Game/Shop), ProjectLifetimeScope +
      Bootstrapper, GameLifetimeScope, ShopLifetimeScope (пустая),
      SceneLoader, YgLifecycleAdapter
  [x] Ф1 Ядро: DuelSim (тик из SimulationDriver 60Гц), GunState/ArenaState,
      MapConfig + ArenaView, MovementSystem (инерция/отскоки/отдача/
      коллизия пушек), GunView + ViewBinder + SpriteFactory, InputReader
      (NIS) -> IPlayerInput
  [x] Ф2 (частично, см. B): AmmoSystem+AmmoState (магазин/реген/полная
      перезарядка), BulletSystem (прямые пули, swept отрезок-круг, смерть
      о стену/препятствие/пушку), BulletPool+BulletView (GO-пул),
      ExplosionSystem (пуля-в-пулю, радиус/урон), PhysicsConfig
  [x] Ф3 (частично, см. B): DamageSystem (HP игрока 3.0 + i-frames,
      EnemyHealthState стадии), AiShooterSystem (рандом-КД, конус+LOS,
      мгновенный выстрел), DuelFlow FSM (Armed->Grace->Fight->
      Victory/Defeat), спавн противоположные концы + случайный импульс,
      DuelResultOverlayView

НЕ ГОТОВО / ОТСУТСТВУЕТ:
  [ ] Парирование (ParrySystem нет, рикошетов нет)
  [ ] Паттерны стрельбы (FireSystem пустой — только тик AmmoSystem;
      логика выстрела вручную в DuelSim.TryFirePlayerGun)
  [ ] Траектории пуль (только прямая: Velocity = dir * speed)
  [ ] Баффы (ничего нет)
  [ ] Мета целиком: папка Core/Meta отсутствует, порты ISaveService/
      IAdsService/IAudioService отсутствуют
  [ ] Магазин: ShopLifetimeScope пустая, ShopFlowService/UI нет
  [ ] HUD нет (события патронов/уровня наружу не публикуются)
  [ ] Ротация карт нет (один MapConfig)
  [ ] Скалирование сложности/элита нет (AiShooterSystem без уровня)
  [ ] Аудио нет (IAudioService/AudioService/AudioConfig)
  [ ] FX минимальные (взрыв/попадание — проверить ViewBinder)
  [ ] Локализация текстов (LanguageYG), настройки (язык/звук)
  [ ] WebGL-сборка/баланс/модерация


================================================================================
РАЗДЕЛ B. НЕДОДЕЛКИ Ф2/Ф3 (закрыть до Ф4)
================================================================================

B1. FX выстрела и попадания (если ещё нет во ViewBinder):
    - подписка ViewBinder на BulletSpawned -> вспышка у дула GunView
    - подписка на GunHit -> шейк камеры + звук (когда будет AudioService)
    - подписка на ExplosionHappened -> шоквейв + шейк
B2. Визуал стадий врага:
    - GunView врага слушает EnemyStageChanged -> смена спрайта
      (3 стадии по ГДД) + партиклы обломков
    - звук разрушения стадии (в Ф8, когда AudioService)
B3. IPlatformLifecycle хуки:
    - проверить: YgLifecycleAdapter подключён к DuelPhaseChanged?
      GameplayStart на входе в Grace/Fight, GameplayStop на Armed-паузе
      и оверлеях — ТРЕБОВАНИЕ модерации ЯИ
B4. Событие патронов для HUD:
    - DuelSim: публиковать PlayerAmmoChanged(snapshot: Magazine,
      MagazineSize, IsFullReloading, RegenProgress) каждый тик
      (struct-снапшот, как GunSnapshot)
B5. Рефактор выстрела под Ф5:
    - логику из DuelSim.TryFirePlayerGun/TickEnemyAi перенести в
      FireSystem (DuelSim слишком толстый), SpawnBullet+ApplyRecoil
      вызывать из FireSystem через колбэки/ссылки


================================================================================
РАЗДЕЛ C. Ф4 — ПАРИРОВАНИЕ
================================================================================

Новые файлы (Core/Simulation):
  [ ] ParrySystem.cs — буфер ввода + проверка радиуса + анти-дабл-парри
  [ ] ParryResult.cs — struct-снапшот (BulletId, NewVelocity, Position)

Изменения:
  [ ] PlayerConfig: поля ParryWindowSeconds=0.2, ParryRadius,
      ParryCooldownSeconds=0.1, ParriedBulletSpeedMultiplier,
      ParriedBulletRicochets=2..3
  [ ] BulletState: поля IsParried, RicochetsLeft, HomingTargetGun
  [ ] BulletSystem: пуля с IsParried о стену/препятствие НЕ умирает —
      отражает Velocity (нормаль = ближайшая грань/радиус препятствия),
      RicochetsLeft -= 1; после 0 отскоков умирает как обычно
  [ ] DuelSim: создать ParrySystem (конструктор), порядок тика —
      ParrySystem.Tick ПОСЛЕ ввода, ДО BulletSystem
  [ ] DuelSim: событие Parried(ParryResult) наружу

ЛОГИКА (канон «нажал = выстрел»):
  1. ConsumePress() вернул true и выстрел возможен -> FireSystem стреляет
     КАК ОБЫЧНО (патрон, отдача)
  2. ПАРАЛЛЕЛЬНО ParrySystem.OpenWindow(ParryWindowSeconds)
  3. На каждом тике окно открыто: искать вражеские пули (OwnerIsPlayer ==
     false, IsAlive) в радиусе ParryRadius от _playerGun.Position
  4. Нашли + анти-дабл-парри КД прошёл -> пуля.IsParried = true,
     пуля.Velocity = (от игрока в сторону стрелка ИЛИ от позиции парри)
     * ParriedBulletSpeedMultiplier, RicochetsLeft = из конфига,
     гоминг-пуля меняет цель на врага
  5. Окно закрывается: 200мс истекли ИЛИ успешное парри

Прокинуть:
  [ ] GameLifetimeScope: builder.Register<ParrySystem>(Lifetime.Scoped)
  [ ] ViewBinder: подписка DuelSim.Parried -> FX (слоу-мо-вспышка,
      звуковой акцент, заморозка кадра ~0.05с)
  [ ] SimulationDebugOverlay: дебаг-радиус зоны парри (gizmo-круг)

ЧЕКПОИНТ: тайминговое парри отбивает пулю, она рикошетит 2–3 раза,
добивает врага; спам-клики не дают дабл-парри одной пули.


================================================================================
РАЗДЕЛ D. Ф5 — ПАТТЕРНЫ СТРЕЛЬБЫ И ТРАЕКТОРИИ
================================================================================

Новые файлы:
  [ ] FirePattern.cs — enum: Single, Burst, Auto, Shotgun
  [ ] TrajectoryType.cs — enum: Straight, Sine, Homing, Accelerating
  [ ] TrajectoryParams.cs — struct: SineAmplitude, SineFrequency,
      HomingRadius, HomingTurnRate, Acceleration, StartSpeedFactor

Изменения:
  [ ] WeaponConfig: FirePattern, BurstCount=3, BurstIntervalSeconds,
      AutoFireIntervalSeconds, PelletCount=5..7, SpreadAngle,
      TrajectoryType + TrajectoryParams
  [ ] WeaponParams: проксирование новых полей
  [ ] FireSystem (после B5): реализация паттернов:
      * Single: тап = 1 пуля, КД оружия
      * Burst: тап = очередь целиком (таймер очереди внутри FireSystem,
        НЕ прерывается; тап во время очереди игнорируется/буферизуется? —
        по ГДД: одиночный тап всегда = полная очередь), обрыв при
        опустошении магазина -> полная перезарядка
      * Auto: пока IPlayerInput.IsHeld — стрельба с AutoFireInterval
        (в IPlayerInput добавить bool IsHeld { get; })
      * Shotgun: 1 тап = PelletCount дробинок веером SpreadAngle,
        урон дробинки ~0.3 от базовой
  [ ] BulletSystem: расчёт Velocity по траектории на тике:
      * Straight: как сейчас
      * Sine: базовое направление + перпендикуляр*sin(t*Freq)*Amplitude
      * Homing: steer Velocity к цели (для пуль игрока — _enemyGun,
        для врага — _playerGun) с HomingTurnRate, захват в HomingRadius
      * Accelerating: Velocity += dir * Acceleration * dt
  [ ] DuelSim.SpawnBullet: проставлять траекторию/цель из WeaponParams
  [ ] AiShooterSystem: скалирование от уровня — параметры множителей из
      EnemyConfig (кривая/коэффициенты), элита каждые 10 (HP стадий ×2,
      КД ×коэф, флаг IsElite наружу для вью)
  [ ] EnemyConfig: ScalingByLevel, EliteEveryNLevels=10,
      EliteHealthMultiplier=2, EliteCooldownMultiplier
  [ ] Конфиги: по WeaponConfig на паттерн × траекторию (тестовые ассеты
      в Assets/Configs/Weapons/)

Прокинуть:
  [ ] GameLifetimeScope: новые конфиги не нужны (WeaponConfig расширился)
  [ ] ViewBinder: элита -> рамка/цвет врага

ЧЕКПОИНТ: все 4 паттерна ощущаются по-разному, синус/гоминг/разгон
летят по-своему, сложность растёт по уровням, на 10-м — элита.


================================================================================
РАЗДЕЛ E. Ф6 — БАФФЫ
================================================================================

Новые файлы:
  Core/Simulation:
  [ ] BuffType.cs — enum: TimeSlow, FastReload, ArenaStrike, Minigun
  [ ] BuffCapsuleState.cs — Position, Velocity, BuffType, IsAlive
  [ ] BuffSystem.cs — спавн по интервалу, дрейф/отскоки, подбор
      касанием игроком (враг игнорирует), таймеры активных эффектов
  Core/Simulation (конфиги):
  [ ] BuffConfig.cs — DurationSeconds/сила по типам, MinigunRate,
      MinigunSpread, ArenaStrikeDamage=1.0
  [ ] BuffSpawnConfig.cs — SpawnIntervalSeconds (или рандом min/max),
      MaxConcurrentCapsules, CapsuleRadius, DriftSpeed

Изменения:
  [ ] DuelSim: создать BuffSystem, тик после MovementSystem;
      события BuffCapsuleSpawned/Removed/BuffActivated/BuffEnded наружу
  [ ] Слоу-мо: DuelSim.TimeScale (свойство) — глобальный множитель на
      вражеские системы (AiShooter/BulletSystem для вражеских пуль);
      SimulationDriver умножает шаг на TimeScale ИЛИ DuelSim применяет
      внутри — решить одним местом (рекомендую внутри DuelSim: вражеский
      множитель, игрок без замедления, по ГДД)
  [ ] FastReload: множитель скорости AmmoSystem на время
  [ ] ArenaStrike: урон 1.0 ТОЛЬКО врагу сквозь препятствия
      (DamageRequest напрямую, не взрыв)
  [ ] Minigun: FireSystem автострельба БЕЗ списывания патронов,
      направление — рандомный сектор, отдача работает полностью
  [ ] MapConfig: зоны спавна баффов (или весь прямоугольник арены)

Presentation:
  [ ] BuffCapsulePool.cs + BuffCapsuleView.cs (GO-пул, как BulletPool)
  [ ] ViewBinder: подписки на события капсул/баффов -> вью + FX
  [ ] HUD-индикатор активных баффов (в Ф7 вместе с HUD)

Прокинуть:
  [ ] GameLifetimeScope: BuffConfig/BuffSpawnConfig -> RegisterInstance,
      BuffSystem -> Register, BuffCapsulePool -> As<...> + RegisterBuildCallback

ЧЕКПОИНТ: капсулы дрейфуют, 4 баффа работают, миниган — хаос с отдачей.


================================================================================
РАЗДЕЛ F. Ф7 — МЕТА И МАГАЗИН
================================================================================

F1. Порты (Core/Ports — создать до сервисов):
  [ ] ISaveService.cs — Save(), событие Loaded
  [ ] IAudioService.cs — PlaySfx(SfxType), PlayMusic(), SetSoundEnabled
  [ ] SfxType.cs — enum (Shoot variants, Hit, StageBreak, Explosion,
      ParrySuccess, ParryFail, BuffPickup, Reload, Victory, Defeat, UiClick)

F2. Core/Meta (новая папка):
  [ ] ProgressService.cs — CurrentLevel, MaxLevel, элита каждые N,
      событие LevelChanged
  [ ] EconomyService.cs — Money, RewardForWin(level) из EconomyConfig
  [ ] InventoryService.cs — купленные ID оружия/скинов,
      EquippedWeaponId (одно на сессию, смена вне боя)
  [ ] PerkService.cs — ранги перков, Apply(WeaponParams) -> WeaponParams
      (единственная точка слияния база+перки, КАП эффектов)
  [ ] ShopFlowService.cs — CanAfford/Buy/Equip
  [ ] EconomyConfig.cs (SO) — базовая награда 25, за уровень ×2,
      цены, коэффициент роста
  [ ] PerkConfig.cs (SO) — 5 перков (КД/магазин/реген/урон/отдача),
      ранги 5, значения/ранг, цены рангов, КАП
  [ ] WeaponCatalog.cs (SO) — список WeaponConfig + цены (для витрины
      и выбора экипировки по ID)

F3. Сейвы:
  [ ] SavesYG partial (Scripts/PlatformYG или Presentation/Services/Saves):
      поля: версия, деньги, уровень, купленные оружия List<string>,
      экипированный ID, ранги перков List<int>, язык, звук вкл
  [ ] YgSaveAdapter.cs (Presentation/Services) -> ISaveService:
      чтение YG2.saves -> сервисы при старте, SaveProgress с
      троттлингом (не чаще раза в N сек)
  [ ] Каданс: сейв на старте каждой дуэли + после покупок
  [ ] ProjectLifetimeScope: бинд Meta-сервисов + ISaveService

F4. Связка мета <-> бой:
  [ ] Bootstrapper: после onGetSDKData раздать сейвы в сервисы ->
      загрузить Game
  [ ] GameLifetimeScope: WeaponConfig брать НЕ с поля сцены, а из
      WeaponCatalog по InventoryService.EquippedWeaponId (поле сцены —
      фолбэк/дефолт пистолет)
  [ ] DuelFlow: Victory -> ProgressService.LevelUp + EconomyService
      начисление; Defeat -> следующий уровень БЕЗ денег
  [ ] EnemyScaling: AiShooterSystem получает уровень из ProgressService
  [ ] Возрождение (каркас без рекламы): DuelResultOverlayView — кнопка
      «Возродиться» (кап 1/бой, счётчик в DuelFlow), восстановление HP
      игрока/патронов + N сек быстрой перезарядки

F5. Магазин (сцена Shop):
  [ ] ShopLifetimeScope: ShopFlowService + вью
  [ ] UI: вкладки Оружие/Скиллы/Скины (uGUI), карточки из WeaponCatalog/
      PerkConfig, кнопки Купить/Экипировать
  [ ] Переходы: меню Game-сцены и экран победы -> кнопка «Магазин» ->
      SceneLoader -> Shop; кнопка «Назад» -> Game
  [ ] Скины: минимально — каталог + экипировка ID (визуал потом)

F6. HUD и экраны:
  [ ] HudView: патроны/реген (по PlayerAmmoChanged из B4), номер уровня,
      индикатор баффов, деньги
  [ ] Экран победы: деньги за дуэль, «Дальше», «Магазин»
  [ ] Экран поражения: «Продолжить без награды» / «Возродиться» (кап)

F7. Карты:
  [ ] 5–6 MapConfig ассетов (ручная сборка, Assets/Configs/Maps/)
  [ ] MapRotationService (Meta или Game скоуп): рандом без повтора
      предыдущей
  [ ] GameLifetimeScope: выбор MapConfig через ротацию, не поле сцены

ЧЕКПОИНТ: полный цикл — дуэль -> деньги -> магазин -> экипировка/перки ->
дуэль; после перезагрузки страницы продолжение с уровня (в редакторе —
локальный сейв плагина).


================================================================================
РАЗДЕЛ G. Ф8 — ПЛАТФОРМА, ЛОКАЛИЗАЦИЯ, РЕЛИЗ
================================================================================

  [ ] G1. IAdsService порт + YgAdsAdapter (модули InterstitialAdv/
      RewardedAdv плагина):
      - ShowRewardedAsync() : UniTask<bool> (мост UniTaskCompletionSource
        на колбэки плагина)
      - ShowInterstitialIfAllowed(): гейт-интервал из AdsConfig
        (ориентир 60с, платформа капит сама — наш не больше)
      - Кнопка «Возродиться» -> rewarded -> результат в DuelFlow
  [ ] G2. AdsConfig (SO): интервал interstitial, кап возрождений=1
  [ ] G3. Interstitial: на переходах уровней (после «Дальше» и после
      «Продолжить без награды»), НЕ между Boot->Game
  [ ] G4. YgInfoAdapter + IPlatformInfo порт: DeviceType (desktop/touch)
      -> подсказки ввода в меню
  [ ] G5. AudioService (IAudioService): пул AudioSource, банк клипов из
      AudioConfig (SO), фоновая музыка 1 трек; SFX-сет из ГДД раздел 7
  [ ] G6. Звук стадий врага + парри + взрыв (добавить в ViewBinder
      обработчики)
  [ ] G7. Локализация: LanguageYG (AutoTranslateLangs) на все TMP-тексты
      UI; автоперевод EN/TR; настройки: ручной выбор языка ->
      YG2.SwitchLanguage + onSwitchLang; шрифты по языкам; машинный
      перевод ВЫЧИТАТЬ
  [ ] G8. Настройки: звук вкл/выкл (в сейв), язык
  [ ] G9. WebGL-сборка: спрайт-атласы, аудио Vorbis/ADPCM, настройки
      плагина по его странице оптимизации, прогон в браузере +
      debug-панель ЯИ (?debug-mode=16)
  [ ] G10. Баланс по конфигам (цифры из ГДД 12.4): окно/радиус парри,
      скорости нестандартных пуль, урон/HP, отдача по паттернам,
      экономика
  [ ] G11. Черновик в Консоли ЯИ: галки облака сейвов, языки ru/en/tr,
      реклама подкл. -> тест -> модерация

ЧЕКПОИНТ: билд в ЯИ, реклама/сейвы/локализация работают на платформе.

ПОСЛЕ РЕЛИЗА: лидерборд (модуль Leaderboards, ProgressService.MaxLevel),
sticky-баннеры, Payments (инапы скинов), Flags, обучение парри.


================================================================================
РАЗДЕЛ H. СВОДКА «ЧТО ГДЕ ПРОКИНУТЬ»
================================================================================

VContainer-биндинги добавить:
  GameLifetimeScope:
    + ParrySystem (Ф4)
    + BuffConfig, BuffSpawnConfig, BuffSystem, BuffCapsulePool (Ф6)
    + ProgressService/EconomyService/... — НЕТ, они в Project (см. ниже)
    + WeaponConfig из WeaponCatalog по экипировке (Ф7)
    + MapConfig через MapRotationService (Ф7)
  ProjectLifetimeScope:
    + ISaveService -> YgSaveAdapter, IAudioService -> AudioService,
      IAdsService -> YgAdsAdapter (Ф7/G1)
    + ProgressService, EconomyService, InventoryService, PerkService,
      ShopFlowService (Scoped? нет — Singleton-жизнь на сессию)
  ShopLifetimeScope:
    + Shop UI-вью, ShopFlowService уже в Project — резолвить

Новые события наружу (DuelSim -> ViewBinder/вью):
  Parried(ParryResult)                    (Ф4)
  PlayerAmmoChanged(AmmoSnapshot)         (B4)
  BuffCapsuleSpawned/Removed(...)         (Ф6)
  BuffActivated/BuffEnded(BuffType)       (Ф6)

Новые порты (Core/Ports):
  ISaveService, IAudioService, IAdsService, IPlatformInfo    (Ф7/G)

IPlayerInput расширить:
  bool IsHeld { get; }                    (Ф5, для паттерна Auto)

Цепочка победы (итоговая):
  DamageSystem: враг HP=0 -> DuelFlow.Victory -> ProgressService.LevelUp +
  EconomyService.Reward -> ISaveService.Save -> экран победы (дальше/
  магазин) -> interstitial-гейт -> MapRotationService -> новая дуэль

Цепочка возрождения (итоговая):
  HP игрока 0 -> Defeat -> оверлей: «Возродиться» -> IAdsService.
  ShowRewardedAsync() -> успех: DuelFlow.Revive (HP=3, патроны, N сек
  быстрой перезарядки) / провал-закрытие: «Продолжить без награды»
  -> следующий уровень без денег

================================================================================
Конец. Продолжать сверху вниз: B -> C(Ф4) -> D(Ф5) -> E(Ф6) -> F(Ф7) ->
G(Ф8). Не забывать: AI_RULES — перечитывать перед каждой сессией.
