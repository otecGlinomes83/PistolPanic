# Правила для AI: Unity C# Gameplay Programming — PISTOL PANIC

Это инструкции для нейросети, помогающей мне писать код. Следовать строго.

---

## Кто я

- Senior Unity gameplay programmer
- Пишу production-ready C# для геймплейных систем
- Знаю что делаю — объяснения и расшаривания не нужны
- Общаюсь на русском, код — на английском
- Прямой стиль, без расшаркиваний

## Проект

- Unity **2022.3.62f2 LTS**, **2D**, **WebGL**, публикация — **Yandex Games**
- Портрет **9:16**, только. Ввод: мышь + тач, одна кодовая база
- **VContainer** — DI обязателен. Зависимости — только через него
- **UniTask** — вместо корутин. Корутины запрещены полностью
- Физический движок **НЕ используется**: кинематика, ручные overlap-проверки
  (дистанция/радиус, swept «отрезок vs круг»), никакого Rigidbody2D
- Рендер: **Built-in pipeline** (без URP). Спрайты, стандартные материалы,
  никаких Shader Graph/кастом-шейдеров без явного запроса
- Стек: TextMeshPro, Yandex Games SDK (rewarded/interstitial/сейвы)
- Сцены: **Bootstrap** (composition root) → **Game** (дуэль) / **Shop**
  (магазин). Переходы между сценами — через сервис загрузки сцен
- IDE: JetBrains Rider

## Стиль общения

- **Терсе, без воды.** Никаких "great idea!", "you're right!", "let me help you".
- **Не объяснять код**, если не попросил явно. Сделал — показал — всё.
- **Не предлагать улучшения** "на будущее" или "ещё можно сделать". Только то, что просил.
- **Если задал уточняющий вопрос и я его скипнул — НЕ предполагай ответ, жди явного ответа от меня.**
- **Не создавать левых файлов**: README, документация, прогресс-логи — только если попросил.
- Прямой тон, без излишней вежливости.

## Перед тем как кодить

1. Если задача нетривиальная — короткий план, затем по шагам
2. Если что-то непонятно — задай конкретный вопрос с вариантами выбора
3. Если знание Unity API неточное — ищи в актуальной доке, не выдумывай
4. Не лезь в файлы, которые не относятся к задаче
5. **Если просят что-то сделать или поменять — сначала поищи это в коде (grep/glob). Если уже существует — уточни у меня, не создавай дубль.** Пример: «сделай кошелёк» → сначала `grep "Wallet"`, не плодить параллельные реализации
6. Новая система — сначала подумай, в какой LifetimeScope она попадает и кто её владелец. Не уверен — спроси

---

## C# Базовые правила

### Запрещено

- `var` — всегда явные типы
- LINQ в рантайме — WebGL, zero-GC. В Editor-утилитах — допустимо
- Reflection
- `UnityEvent` — только C# `Action<T>` события
- Анонимные lambda при подписке на события (`+= () => ...`)
- **`static` — запрещён полностью**: классы, поля, свойства, события, методы.
  Exception: `const` (это не состояние). Кэш-буферы (массивы для NonAlloc/
  ручных проверок) — инстансные `readonly` поля
- `GameObject.Find`, `Transform.Find`, `FindObjectOfType` — зависимости через VContainer
- Service Locator паттерн
- Singleton — VContainer вместо него
- Имена `Manager`, `Handler`, `Utility`, `Helper` (если ответственность не очевидна)
- Создавать интерфейсы "на будущее"
- Глубокие иерархии наследования
- Магические абстракции, hidden side effects
- Комментарии в коде. Никогда.
- Тернарный оператор (`condition ? a : b`) — писать обычным `if/else`, явнее
- **Корутины** — вообще никогда. Только UniTask
- **Логика в ScriptableObject — никак вообще.** SO = чистые данные: только
  поля. Ни методов с поведением, ни событий, ни геймплейных вычислений
- Rigidbody2D / физдвижок / физические триггеры — все пересечения считаем руками
- Публичные поля (кроме data-полей DTO/структур данных, если оправдано)
- Хардкод балансных чисел в логике — **любое число, которое может захотеть
  поменять дизайнер, живёт в SO-конфиге**

### Обязательно

- Композиция > наследование
- 1 класс = 1 ответственность = 1 файл
- SOLID — жёстко, но прагматично. YAGNI, KISS, DRY
- Короткие сфокусированные методы
- Early returns вместо вложенных if
- Поток выполнения сверху вниз — код читается линейно
- Явные зависимости: VContainer `[Inject]` для MonoBehaviour,
  конструктор для чистых C# классов
- **Максимальная настройка через SO-конфиги** — вся балансировка (урон, КД,
  скорости, тайминги, цены, интервалы) выносится в данные, логика их только читает
- `sealed class` для классов, которые не наследуются
- Простые FSM где есть состояния
- Пулы объектов для всего, что спавнится (пули, капсулы, эффекты, партиклы)

---

## Naming Conventions

| Что | Стиль | Пример |
|---|---|---|
| Private field | `_fieldName` | `_bulletPool`, `_parryWindow` |
| Constants | `PascalCase` | `BulletLayer`, `ArenaPadding` |
| Methods | `PascalCase` | `Fire`, `OnBulletHit` |
| Events | `PascalCase` | `Fired`, `Parried`, `StageChanged` |
| Local vars | `camelCase` | `elapsedTime`, `hitCount` |
| Arguments | `camelCase` | `targetPosition`, `pullSpeed` |
| Interfaces | `IInterfaceName` | `IDamageable`, `IPoolable` |
| Bool fields/properties | `_isSomething` / `IsSomething` | `_isParryWindowOpen`, `IsReloading` |
| SO-конфиги | суффикс `Config` | `WeaponConfig`, `ParryConfig`, `EnemyConfig` |
| LifetimeScope | суффикс `LifetimeScope` | `ProjectLifetimeScope`, `GameLifetimeScope` |
| Фабрики | суффикс `Factory` | `BulletFactory` |

### Имена переменных

- **Никаких однобуквенных имён.** Не `t`, не `b`, не `c`.
- Исключение: `i`, `j` как счётчики простых for-циклов.
- Имена описательные: `bulletSpeed`, `parryWindowSeconds`, `closestHitDistance`.

---

## Форматирование

- **Соблюдать [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).** Это базовая линия стиля.
- **Никаких однострочных методов и блоков.** Каждое тело метода/свойства/if/else/цикла — на отдельных строках, с фигурными скобками на новой строке. Никогда не писать `public void Foo() { Bar(); }` — так не пишем никогда.
- **Открывающая фигурная скобка всегда на новой строке.**
- Всегда braces для if/else/loops (даже однострочных).
- Логические блоки разделяются пустыми строками.
- Удалять unused using-директивы.
- **Один файл = один класс.**
- Сравнение с false через `== false`, не `!`:

```csharp
if (weapon.IsReadyToFire == false)
{
    return;
}
```

- Игра 2D: типы `Vector2`, `Collider2D`, углы/повороты — `float` в градусах
  через `Quaternion.Euler(0f, 0f, angle)`, спрайты вместо мешей.

---

## Архитектура

### Слои (жёсткое разделение)

| Слой | Что это | Примеры |
|---|---|---|
| **Data** | SO-конфиги (чистые данные), save-модели, таблицы | `WeaponConfig`, `EnemyStageConfig` |
| **Logic** | Plain C# классы без Unity-зависимостей где возможно | `DamageService`, `ParryBuffer`, `AiShooter`, `EconomyService` |
| **Presentation** | MonoBehaviour-вью: спрайты, анимации, партиклы, UI | `GunView`, `BulletView`, `ParryFxView`, `HudView` |

- **SRP — святое.** Класс делает одну вещь. `ParryBuffer` не знает про эффекты,
  `DamageService` не знает про спрайты стадий.
- **Логика не зависит от вью.** Вью подписываются на события логики.
  Обратное — запрещено.
- Источник истины в одном месте. UI читает через события, не дёргает
  состояние систем напрямую.
- Не создавать абстракции до того, как реально нужны. Интерфейсы — только
  если есть причина (полиморфизм, разные реализации, тесты).

### VContainer — канон

- **Composition root — Bootstrap-сцена**: `ProjectLifetimeScope` —
  персистентные сервисы (сейвы, экономика, аудио, реклама, загрузчик сцен).
- **Сценовые скоупы**: `GameLifetimeScope` (дуэль: спавнеры, пулы, ИИ,
  конфиги боя), `ShopLifetimeScope` (витрины, покупки).
- Между сценами состояние передаётся ТОЛЬКО через персистентные сервисы
  из `ProjectLifetimeScope`. Никаких static-посредников.
- MonoBehaviour — инъекция через `[Inject]`-метод, не конструктор.
- Чистые C# классы — через конструктор.
- Конфиги SO биндятся из ассетов прямо в скоупе.

```csharp
public sealed class GameLifetimeScope : LifetimeScope
{
    [SerializeField]
    private WeaponConfig _playerWeaponConfig = null;

    [SerializeField]
    private ParryConfig _parryConfig = null;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_playerWeaponConfig).AsSelf();
        builder.RegisterInstance(_parryConfig).AsSelf();

        builder.Register<ParryBuffer>(Lifetime.Scoped);
        builder.Register<DamageService>(Lifetime.Scoped);
        builder.Register<BulletPool>(Lifetime.Scoped).As<IBulletPool>();

        builder.RegisterComponentOnNewGameObject<GunView>(Lifetime.Scoped, "PlayerGun");
    }
}
```

### События (C# Action)

- Только `event Action<T>`. **Никогда UnityEvent.**
- Всегда отписываться в `OnDisable` (или `OnDestroy` если уместнее).
- **Никогда анонимные лямбды:**

```csharp
// ПЛОХО:
_bulletPool.Spawned += (bullet, position) => { ... };

// ХОРОШО:
private void OnEnable()
{
    _bulletPool.Spawned += OnBulletSpawned;
}

private void OnDisable()
{
    _bulletPool.Spawned -= OnBulletSpawned;
}

private void OnBulletSpawned(IBullet bullet, Vector2 spawnPosition)
{
    // ...
}
```

---

## Unity-Specific

- `TryGetComponent` вместо `GetComponent` + null check
- TextMeshPro вместо legacy Text
- **Минимизировать аллокации**: ноль GC в бою. Никакого бокса, строк в
  горячем пути, LINQ в рантайме. Кэш-массивы — инстансные `readonly`
- Избегать лишних Update-методов: тик систем — через UniTask-циклы в
  сервисах; Update во вью — только когда реально нужен кадр
- `[SerializeField]` — только для ссылок/префабов, которые вешаются в
  инспекторе. Балансные значения — в SO-конфигах, не в инспекторе логики
- НЕ exposить public fields, если не нужно
- `[RequireComponent]` где жёсткая зависимость от компонента на том же GO
- Пули — быстрый проджектайл: пул + swept-тест «отрезок vs круг» против
  туннелирования на скорости 13:1
- Пересечения руками: дистанция точка-круг, отрезок-круг, конус зрения —
  через `Vector2.Dot` и `Vector2.SignedAngle`. Никакой физики.

---

## Async — UniTask

UniTask — единственный async-инструмент. Корутины запрещены.

### Канонический паттерн с cancellation

```csharp
private async UniTaskVoid ReloadCycleAsync()
{
    CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

    try
    {
        await DoReloadAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
        return;
    }

    OnReloadFinished();
}

private async UniTask DoReloadAsync(CancellationToken cancellationToken)
{
    while (_isReloading)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
    }
}
```

### Правила UniTask

- Fire-and-forget — `.Forget()`
- Возвращаемый тип fire-and-forget методов — `UniTaskVoid`
- Cancellation через `this.GetCancellationTokenOnDestroy()` для авто-отмены при уничтожении
- `OperationCanceledException` ловить явно
- `UniTask.Yield(PlayerLoopTiming.Update, cancellationToken)` — тик систем
- Тайминги/КД/реген/баффы/рекламные флоу (Yandex SDK) — через UniTask

---

## SO-конфиги — канон

- SO = чистые данные. Только `[SerializeField]` поля + get-only свойства.
- **Никаких методов с логикой, событий, вычислений, OnValidate-геймплея.**
- Любое число баланса — в конфиг. Логика читает, не решает.

```csharp
[CreateAssetMenu(menuName = "PistolPanic/WeaponConfig")]
public sealed class WeaponConfig : ScriptableObject
{
    [SerializeField]
    private float _damage = 1f;

    [SerializeField]
    private float _cooldownSeconds = 0.8f;

    [SerializeField]
    private int _magazineSize = 6;

    [SerializeField]
    private float _bulletSpeed = 13f;

    public float Damage => _damage;

    public float CooldownSeconds => _cooldownSeconds;

    public int MagazineSize => _magazineSize;

    public float BulletSpeed => _bulletSpeed;
}
```

---

## Канонический пример класса

```csharp
public sealed class GunView : MonoBehaviour
{
    [Inject]
    private readonly ParryBuffer _parryBuffer = null;

    [Inject]
    private readonly WeaponConfig _weaponConfig = null;

    private event Action<IBullet, Vector2> BulletFired;

    private bool _isSetupFinished;

    private void Update()
    {
        if (_isSetupFinished == false)
        {
            return;
        }

        RotateTowardsPointer();
    }

    [Inject]
    private void Construct(ParryBuffer parryBuffer, WeaponConfig weaponConfig)
    {
        _parryBuffer = parryBuffer;
        _weaponConfig = weaponConfig;

        _isSetupFinished = true;
    }
}
```

Примечание: поля `[Inject] readonly` ИЛИ `[Inject]`-метод — не оба сразу.
Метод — когда нужен порядок/валидация, поля — когда просто.

---

## Чек-лист перед отдачей кода

1. Нет `var` — всё с явными типами
2. Нет комментариев
3. Нет анонимных лямбд при `+=`
4. Все события отписываются
5. Имена переменных описательные (не `t`, не `b`, не `c`)
6. `== false` вместо `!`
7. Braces на новой строке, везде используются
8. Пустые строки между логическими блоками
9. `sealed` где уместно
10. **Нет `static` нигде** (кроме `const`)
11. **Нет корутин — только UniTask**
12. **SO не содержат логику — только данные**
13. **Нет хардкода балансных чисел — всё в конфигах**
14. Зависимости — через VContainer, не Find/статик/синглтон
15. Нет Manager/Handler/Utility/Helper имён
16. Нет лишних абстракций / интерфейсов «на будущее»
17. Нет лишних using-директив
18. Один класс в одном файле
19. **Нет однострочных тел методов / if-блоков / свойств.** Каждое тело — многострочное, скобки на отдельных строках.
20. Ноль GC-аллокаций в боевом пути
21. Соответствие Microsoft C# coding conventions

---

## Если AI не знает как сделать

1. **Не выдумывать Unity API.** Не существует — спросить или поискать актуальную доку.
2. **Не предполагать ответы на скипнутые вопросы.** Жди явного ответа.
3. При сложной задаче — короткий план, реализация по шагам.
4. VContainer/UniTask API неточный — смотреть доку пакета, не угадывать.

---

## Поведенческий итог

1. Делай быстро, делай минимально
2. Не делай ничего "на потом"
3. Не объясняй то, о чём не спросил
4. Не комментируй код
5. Если скипнул вопрос — жди ответа
6. Уважай SRP, KISS, YAGNI, SOLID
7. Один класс — одна ответственность — один файл
8. **Не пиши тела в одну строку.** Метод, свойство, if — многострочный блок
9. **Статика, корутины, логика в SO, хардкод баланса — не обсуждаются, их просто нет**

---

# Дополнительные правила поведения AI

1. **Не соглашайся автоматически.** Если решение плохое, переусложнённое, нарушает архитектуру или создаёт тех. долг — скажи прямо и предложи лучший путь.

2. **Не пиши код, пока не попросили.** По умолчанию помогай думать: ответственность классов, архитектура, flow данных, декомпозиция, SOLID, масштабирование.

3. **Если показываю код — сначала анализируй.** Не переписывай всё без причины.

4. **Если можно упростить — упрощай.**

5. **Не навязывай паттерны.** Каждый паттерн должен решать реальную проблему.

6. **Перед кодом думай:**
   - Какая ответственность класса?
   - Кто владеет состоянием?
   - Кто инициирует действие?
   - В каком скоупе VContainer это живёт?
   - Где источник истины?
   - Не создаётся ли лишняя связанность?

7. **При разборе класса объясняй:** ответственность, входы, выходы, зависимости, жизненный цикл, возможные проблемы.

8. **WebGL-first производительность:** zero-GC в бою, пулы, ручные overlap-проверки, никакого LINQ в рантайме.

9. **Текстовый стиль:** короткие предложения, без воды, без корпоративного стиля. Пиши прямо. Если ответ плохой — говори это.

10. **Копируй текущий кодстайл проекта.** Перед написанием нового кода в существующем модуле — посмотри соседние файлы: нейминг, форматирование, паттерны, неймспейсы. Следуй тому, как написано в проекте. Глобальные правила (нет `var`, нет комментариев, `== false`, нет статики/корутин/логики в SO и т.д.) — остаются в силе поверх стиля проекта.
