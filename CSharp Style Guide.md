This document is my personal standard operating procedure for writing C# scripts.

**Goal:** consistent, readable, low-friction code for humans first for development teams I work with.

---

## Table of Contents

1. [Naming Conventions](#1-naming-conventions)
   - [1.1 Types](#11-types)
   - [1.2 Members](#12-members)
   - [1.3 Fields](#13-fields)
   - [1.4 Constants](#14-constants)
   - [1.5 Boolean naming](#15-boolean-naming)
2. [Accessibility & Encapsulation](#2-accessibility-encapsulation)
   - [2.1 Unity lifecycle methods](#21-unity-lifecycle-methods)
   - [2.2 Helper methods and fields](#22-helper-methods-and-fields)
   - [2.3 Why this matters](#23-why-this-matters)
3. [Design Principles](#3-design-principles)
   - [3.1 Single Responsibility Principle](#31-single-responsibility-principle)
   - [3.2 Open/Closed Principle](#32-openclosed-principle)
4. [`var` & Type Style](#4-var-type-style)
   - [4.1 Default to `var`](#41-default-to-var)
   - [4.2 When to use explicit types](#42-when-to-use-explicit-types)
   - [4.3 Rule of thumb](#43-rule-of-thumb)
5. [Formatting & Layout](#5-formatting-layout)
   - [5.1 Braces](#51-braces)
   - [5.2 Single‑line statements](#52-singleline-statements)
   - [5.3 Attributes](#53-attributes)
   - [5.4 Blank lines & member ordering](#54-blank-lines-member-ordering)
6. [Regions & Comments](#6-regions-comments)
   - [6.1 Regions](#61-regions)
   - [6.2 Comments & Self-Documenting Code](#62-comments-self-documenting-code)
7. [Console Warnings & Import Hygiene](#7-console-warnings-import-hygiene)
8. [Code Style & Clarity](#8-code-style-clarity)
   - [8.1 Parentheses & Complex Conditionals](#81-parentheses-complex-conditionals)
   - [8.2 `this.` Qualification](#82-this-qualification)
   - [8.3 Use `nameof()` to Avoid String Literals](#83-use-nameof-to-avoid-string-literals)
9. [Examples (canonical style)](#9-examples-canonical-style)
10. [When in doubt](#10-when-in-doubt)

---

## 1. Naming Conventions

Consistent naming is the foundation of readable code. Clear names eliminate ambiguity and make code self-documenting.

### 1.1 Types
- **Classes / Structs / Enums:** `PascalCase`
- **Interfaces:** `PascalCase` with **`I` prefix**

```csharp
public class GrabbableSlider : MonoBehaviour { }
public interface IGrabbable { }
```

### 1.2 Members
- **Methods / Properties / Events:** `PascalCase`
- **Locals / Parameters:** `camelCase`

```csharp
public event Action OnStateChanged = delegate { };

public Vector3 GrabPoint { get; private set; }

private void UpdateSelection()
{
    var bestHit = FindBestHit();
}
```

### 1.3 Fields
- **Private / Protected / Internal instance fields:** `_camelCase`
- **Public instance fields (discouraged):** `PascalCase`
- **Static fields:** `PascalCase`
- **Static readonly fields:** `PascalCase`

#### Serialized fields

**Prefer auto-properties with `[field: SerializeField]`** over separate backing fields:

```csharp
// Preferred: single variable, compile-time safety
[field: SerializeField] public Rigidbody ConnectedRigidbody { get; private set; }
[field: SerializeField] public float MoveSpeed { get; private set; }
```

This is safer than maintaining two separate variables (a private serialized field and a public property wrapper). It eliminates the risk of the field and property getting out of sync.

**Use traditional serialized fields** when you need custom logic in the getter/setter:

```csharp
[SerializeField] private float _moveSpeed;
public float MoveSpeed
{
    get => _moveSpeed;
    set => _moveSpeed = Mathf.Max(0, value);
}
```

**Other field examples:**

```csharp
private IGrabbable _heldObject;
public bool Locked = false;
public static GameManager Instance;
public static readonly Vector3 DefaultSpawn = Vector3.zero;
```

### 1.4 Constants
Prefer **PascalCase** for constants (public or private).
`ALL_CAPS` is acceptable only for **well‑known external identifiers**
(e.g., shader keywords, fixed protocol strings) where that casing is conventional.

```csharp
private const int FrameBuffer = 3;
private const string EmissionKeyword = "_EMISSION";

// acceptable when matching external convention:
private const int FRAME_BUFFER = 3;
```

### 1.5 Boolean naming
Name booleans positively, avoid the `Is` prefix, and avoid double negatives. Boolean names should read naturally in conditional statements.

> [!success] Prefer
> ```csharp
> public bool Enabled { get; set; }
> public bool Locked { get; set; }
> private bool _hasAmmo;
> ```

Reading naturally: `if (Enabled)`, `if (Locked)`, `if (_hasAmmo)`

> [!failure] Avoid
> ```csharp
> public bool Disabled { get; set; }        // prefer Enabled (avoid double negative: "if (!Disabled)")
> public bool Unlocked { get; set; }        // prefer Locked
> private bool _noAmmo;                     // prefer _hasAmmo
> public bool IsEnabled { get; set; }       // prefer Enabled (broken English: "if is enabled")
> ```

---

## 2. Accessibility & Encapsulation

Proper encapsulation is critical for maintainable code. Always use the most restrictive access modifier appropriate for each member. This prevents unintended dependencies, makes refactoring safer, and clearly communicates intent.

All non‑interface members must explicitly declare accessibility
(`public`, `private`, `protected`, `internal`).

### 2.1 Unity lifecycle methods

Unity lifecycle methods (`Awake`, `Start`, `Update`, `OnEnable`, `OnDisable`, etc.) should **always be marked `private`** unless you have a specific reason to expose them.

> [!failure] Common mistake: omitting access modifiers
> ```csharp
> public class GameController : MonoBehaviour
> {
>     void OnEnable()  // defaults to private, but implicit
>     {
>         EventBus.Subscribe();
>     }
>
>     void Start()  // unclear intent
>     {
>         Initialize();
>     }
>
>     void Update()  // can be accidentally called externally
>     {
>         ProcessInput();
>     }
> }
> ```

> [!success] Always explicit: shows clear intent
> ```csharp
> public class GameController : MonoBehaviour
> {
>     private void OnEnable()  // explicit encapsulation
>     {
>         EventBus.Subscribe();
>     }
>
>     private void Start()  // clear: internal lifecycle method
>     {
>         Initialize();
>     }
>
>     private void Update()  // prevents external callers
>     {
>         ProcessInput();
>     }
> }
> ```

### 2.2 Helper methods and fields

Helper methods and backing fields should be `private` unless other classes need access.

> [!failure] Avoid: exposing internals unnecessarily
> ```csharp
> public class HealthSystem : MonoBehaviour
> {
>     public float _currentHealth;  // exposes mutable state
>
>     public float ApplyDamageModifier(float damage)  // should be private
>     {
>         return damage * 1.5f;
>     }
> }
> ```

> [!success] Prefer: restrict access appropriately
> ```csharp
> public class HealthSystem : MonoBehaviour
> {
>     private float _currentHealth;  // encapsulated
>
>     public float CurrentHealth => _currentHealth;  // controlled read access
>
>     private float ApplyDamageModifier(float damage)  // private helper
>     {
>         return damage * 1.5f;
>     }
> }
> ```

### 2.3 Why this matters

- **Prevents accidental coupling:** Other code can't depend on internals
- **Enables safe refactoring:** Private members can be changed without breaking external code
- **Communicates intent:** `private` signals "implementation detail"; `public` signals "contract"
- **Catches errors earlier:** Compiler prevents misuse of internal methods

---

## 3. Design Principles

Write code that is easy to extend and maintain. Follow these core principles to keep classes and methods focused and changeable.

### 3.1 Single Responsibility Principle

Every class and method should have **one clear purpose**. If you can't describe it in one sentence without "and", it's doing too much.

> [!failure] Avoid: class doing too much
> ```csharp
> public class PlayerController : MonoBehaviour
> {
>     // Handles input, physics, animation, inventory, UI, networking
>     private void Update()
>     {
>         HandleInput();
>         UpdatePhysics();
>         UpdateAnimation();
>         ManageInventory();
>         UpdateHealthBar();
>         SyncNetworkState();
>     }
> }
> ```

> [!success] Prefer: focused, single-purpose classes
> ```csharp
> public class PlayerController : MonoBehaviour
> {
>     private PlayerInput _input;
>     private PlayerMovement _movement;
>     private PlayerAnimator _animator;
>
>     private void Update()
>     {
>         var inputData = _input.GetInput();
>         _movement.Move(inputData);
>         _animator.UpdateState(_movement.Velocity);
>     }
> }
> ```

**Methods should do one thing:**

> [!failure] Avoid: method doing multiple things
> ```csharp
> public void ProcessInteraction()
> {
>     // Validates, logs, updates state, plays audio, triggers events
>     if (!CanInteract()) return;
>     Debug.Log("Interaction started");
>     _currentState = State.Interacting;
>     _audioSource.Play();
>     OnInteractionComplete?.Invoke();
>     SaveInteractionToDatabase();
> }
> ```

> [!success] Prefer: focused methods
> ```csharp
> public void ProcessInteraction()
> {
>     if (!CanInteract()) return;
>
>     StartInteraction();
>     NotifyInteractionComplete();
>     SaveInteraction();
> }
> ```

### 3.2 Open/Closed Principle

Add new behavior without changing existing code. Use when you have **10+ variations** with **complex platform logic**. For 2-4 simple cases, use if/switch.

> [!failure] Avoid: one class handling all platforms
> ```csharp
> public class TeleportController
> {
>     public void Teleport(Vector3 destination, string platform)
>     {
>         if (platform == "Desktop")
>         {
>             _camera.transform.position = destination;
>             PlayDesktopEffect();
>             // + 40 lines of Desktop-specific code
>         }
>         else if (platform == "VR")
>         {
>             _xrRig.position = destination;
>             FadeScreen(Color.black);
>             RecenterTracking();
>             // + 40 lines of VR-specific code
>         }
>         else if (platform == "Mobile")
>         {
>             _mobileCamera.position = destination;
>             // + 40 lines of Mobile-specific code
>         }
>     }
> }
> ```

> [!success] Prefer: each platform is its own class
> ```csharp
> public interface ITeleportController
> {
>     void Teleport(Vector3 destination);
> }
>
> public class DesktopTeleportController : ITeleportController
> {
>     public void Teleport(Vector3 destination)
>     {
>         _camera.transform.position = destination;
>         PlayDesktopEffect();
>         // + 40 lines: smooth camera, collision checks, audio
>     }
> }
>
> public class VRTeleportController : ITeleportController
> {
>     public void Teleport(Vector3 destination)
>     {
>         _xrRig.position = destination;
>         FadeScreen(Color.black);
>         RecenterTracking();
>         // + 40 lines: headset tracking, haptics, comfort
>     }
> }
>
> public class MobileTeleportController : ITeleportController
> {
>     public void Teleport(Vector3 destination)
>     {
>         _mobileCamera.position = destination;
>         // + 40 lines: touch controls, gyro, performance
>     }
> }
>
> // Any class can use ITeleportController without knowing the platform
> public class TeleportPad : MonoBehaviour
> {
>     private ITeleportController _teleportController;
>
>     public void OnPlayerEnter()
>     {
>         var destination = GetTeleportDestination();
>         _teleportController.Teleport(destination);  // Works for any platform
>     }
> }
> ```

Each platform is isolated. TeleportPad works with any platform. Adding AR won't risk breaking VR or Desktop.

---

## 4. `var` & Type Style

C# remains **fully statically and strongly typed** when using `var`.
The compiler infers the **exact concrete type at compile time** from the right-hand side.
`var` does **not** introduce dynamic typing or runtime ambiguity.

### 4.1 Default to `var`

Use `var` by default for local variables:

```csharp
var slider = GetComponent<GrabbableSlider>();
var hitResults = Array.Empty<RaycastHit>();
var lookup = new Dictionary<ISelectable, List<RaycastHit>>();
var selection = FindClosestSelectable();
var items = GetSelectables();
```

**Benefits:**
- **Refactoring-friendly** – changing return types doesn't break call sites
- **Less noise** – removes redundant type information
- **Faster to read/write** – less cognition needed, especially with long generic types
- **Forces good naming** – variable names must be clear when types aren't repeated

### 4.2 When to use explicit types

Use explicit types **only** when they meaningfully improve clarity:

**Use explicit types for:**

1. **Numeric literals** – **when required** for precision and prevent unintended conversions

```csharp
int frameCount = 0;
float timeoutSeconds = 5.0f;
double precision = 0.001;
```

2. **When the right side is unclear or misleading**

```csharp
// Unclear what type Parse returns
int userId = Parse(input);

// Method name doesn't reveal type
string result = GetValue();
```

3. **When you need to cast or use a specific base type** (rare for local variables)

```csharp
// Casting step to base content type
SimulatorContent content = GetStep();

// Getting interactable as specific capability
ISelectable selectable = hoveredObject;

// Casting behavior to interface contract
IGrabbable grabbable = interaction.Target;
```

### 4.3 Rule of thumb

- **Default to `var`** for all local variables
- Use explicit types for **numeric literals**
- Use explicit types when the right side is **ambiguous or unclear**
- Use explicit types when you **intentionally need a base type**

The type is still there—the compiler knows it. `var` just removes redundancy and makes refactoring safer.

When in doubt, use `var`.

---

## 5. Formatting & Layout

Consistent formatting makes code easier to scan and understand. Follow these conventions for visual structure and organization.

### 5.1 Braces
- Use **Allman braces** (opening brace on its own line).

```csharp
public class Foo
{
    public void Bar()
    {
        if (condition)
        {
            DoThing();
        }
    }
}
```

### 5.2 Single‑line statements
- For simple, single‑statement blocks, braces are optional.
- If the block grows beyond one line, **add braces**.

```csharp
if (_isActive)
    Enable();

if (_isActive)
{
    Enable();
    LogState();
}
```

### 5.3 Attributes
- `[SerializeField]` always appears on its own line with the field declaration
- Other attributes can stack on a single line when brief
- Use separate lines when stacking becomes unclear
- Use `[Header]`, `[Tooltip]`, `[Range]` for inspector clarity
- NaughtyAttributes are allowed where helpful

```csharp
[SerializeField] private float _speed;

[Range(0f, 100f)] [SerializeField] private float _damping;

[Header("Slider Settings")]
[Tooltip("Controls how quickly the slider slows down.")] [Range(0f, 100f)]
[SerializeField] private float _drag;

[field: SerializeField] public SliderEventProcessor Events = new();
```

### 5.4 Blank lines & member ordering
Group members in categories:
1. Serialized fields
2. Constants
3. Non‑serialized fields
4. Properties / Events
5. Unity lifecycle methods
6. Public API
7. Private helpers

```csharp
public class Selector : MonoBehaviour
{
    // 1. Serialized fields
    [SerializeField] private LayerMask _mask;
    [SerializeField] private float _radius = 0.1f;

    // 2. Constants
    private const int MaxResults = 10;

    // 3. Non-serialized fields
    private RaycastHit[] _results = Array.Empty<RaycastHit>();
    private ISelectable _currentSelection;

    // 4. Properties / Events
    public ISelectable Selection { get; private set; }
    public Vector3 SelectionPoint { get; private set; }

    // 5. Unity lifecycle methods
    private void Update()
    {
        UpdateSelection();
    }

    // 6. Public API
    public void ClearSelection()
    {
        Selection = null;
    }

    // 7. Private helpers
    private void UpdateSelection()
    {
        var selection = TryGetSelection();
        if (selection != null)
            Selection = selection;
    }

    private ISelectable TryGetSelection()
    {
        return null;
    }
}
```

---

## 6. Regions & Comments

Code should be self-documenting. Avoid clutter and anti-patterns that hide problems instead of fixing them.

### 6.1 Regions

**Do not use `#region` directives.** Regions are a code smell indicating that your class is doing too much or lacks clear organization.

> [!failure] Avoid: hiding complexity with regions
> ```csharp
> public class GameManager : MonoBehaviour
> {
>     #region Initialization
>     // ... 50 lines
>     #endregion
>
>     #region Update Loop
>     // ... 100 lines
>     #endregion
>
>     #region Helper Methods
>     // ... 200 lines
>     #endregion
> }
> ```

> [!success] Prefer: split into focused classes
> ```csharp
> public class GameManager : MonoBehaviour
> {
>     private GameInitializer _initializer;
>     private GameUpdateLoop _updateLoop;
>     private GameHelpers _helpers;
> }
> ```

If you feel the need for regions, consider:
- **Extracting a new class** to handle that responsibility
- **Following the member ordering** defined in section 5.4
- **Splitting large methods** into smaller, well-named helpers

### 6.2 Comments & Self-Documenting Code

Follow the principles from *Clean Code* by Robert C. Martin: **code should be self-documenting**. Good naming and clear structure eliminate most comments.

**When comments are valuable:**
- Explaining **why** a non-obvious decision was made (not *what* the code does)
- Warning about consequences or side effects
- TODO markers for incomplete work
- Legal notices or attributions
- Documenting public APIs meant for external consumption

**When to avoid comments:**
- **Redundant XML summaries** that just restate the method name
- Explaining **what** the code does (the code itself should be clear)
- Commented-out code (use version control instead)
- Noisy comments that clutter without adding value

> [!failure] Avoid: redundant XML summaries
> ```csharp
> /// <summary>
> /// Gets the player health.
> /// </summary>
> public float GetPlayerHealth()
> {
>     return _playerHealth;
> }
>
> /// <summary>
> /// Updates the game state.
> /// </summary>
> private void UpdateGameState()
> {
>     // Update all systems
>     _physics.Update();
>     _rendering.Update();
> }
> ```

> [!success] Prefer: self-documenting code
> ```csharp
> public float PlayerHealth => _playerHealth;
>
> private void UpdateGameState()
> {
>     _physics.Update();
>     _rendering.Update();
> }
> ```

> [!success] Good comments: explain *why*, not *what*
> ```csharp
> // Unity's physics engine requires a minimum velocity threshold to prevent jitter
> // when objects are nearly stationary. Without this clamp, the slider vibrates.
> if (velocity.magnitude < MinVelocityThreshold)
>     velocity = Vector3.zero;
>
> // TODO: Replace with object pooling when we exceed 100 active projectiles
> Instantiate(_projectilePrefab, position, rotation);
>
> // Workaround for Unity bug #12345: AudioSource.Stop() doesn't release the clip
> // immediately, causing memory spikes when rapidly spawning/destroying sources.
> _audioSource.clip = null;
> _audioSource.Stop();
> ```

**XML documentation (`///`):** Use sparingly.

For **shared libraries or frameworks** (code consumed by other teams):
- Document all public APIs
- Include parameter constraints and return value descriptions
- Document exceptions that can be thrown

For **internal project code**:
- Prefer clear naming over documentation comments
- Add XML docs only when the API is complex or has non-obvious behavior
- Skip XML docs for simple getters, setters, and self-explanatory methods

---

## 7. Console Warnings & Import Hygiene

A clean console helps everyone on the team work more effectively. When warnings accumulate, they create noise that makes real issues harder to spot—both for you and your teammates.

Think of it like leaving the campground cleaner than you found it. Small efforts to keep things tidy make a big difference for everyone.

**What helps most:**
- Addressing warnings when you save and import your scripts
- Removing unused fields, variables, and obsolete API calls as you go
- Cleaning up warnings in files you're already working on

**Common warnings worth addressing:**
- Unused private fields and variables
- Deprecated/obsolete API calls
- Unreachable code
- Missing serialized field references (unless intentional)

**When you need to keep something for later:**
- A clear `// TODO:` comment explaining why helps others understand your intent
- Conditional compilation (`#if UNITY_EDITOR`) works well for editor-only code
- Consider whether it needs to exist now, or if it can wait until it's actually needed

> [!failure] Avoid: leaving warnings for others
> ```csharp
> public class PlayerController : MonoBehaviour
> {
>     private float _unusedSpeed;  // CS0414: assigned but never used
>     [SerializeField] private GameObject _debugPanel;  // never referenced
>
>     private void Start()
>     {
>         // Uses deprecated API, generates warning
>         Application.LoadLevel("MainMenu");
>     }
> }
> ```

> [!success] Prefer: clean, warning-free code
> ```csharp
> public class PlayerController : MonoBehaviour
> {
>     // Removed _unusedSpeed (wasn't being used)
>     // Removed _debugPanel (was leftover from testing)
>
>     private void Start()
>     {
>         SceneManager.LoadScene("MainMenu");  // Updated to non-deprecated API
>     }
> }
> ```

> [!success] Acceptable: documented intent for parked code
> ```csharp
> public class NetworkManager : MonoBehaviour
> {
> #if UNITY_EDITOR
>     // Editor-only: Used by automated testing framework
>     [SerializeField] private bool _simulateLatency;
>     [SerializeField] private int _simulatedPingMs = 100;
> #endif
>
> #if ENABLE_MULTIPLAYER
>     // TODO (Phase 2): Multiplayer sync - waiting on server infrastructure (Ticket #456)
>     // Will be uncommented when backend team deploys the new WebSocket endpoint
>     // private WebSocketClient _socketClient;
>     // private float _lastSyncTime;
> #endif
>
>     private void Start()
>     {
>         InitializeNetworking();
>     }
>
>     private void InitializeNetworking()
>     {
> #if UNITY_EDITOR
>         if (_simulateLatency)
>             Debug.Log($"Simulating {_simulatedPingMs}ms latency");
> #endif
>     }
> }
> ```

**How to check your work:**
- Watch the console when you save a script—warnings appear during import
- Search for `t:scripts` in the Project window, select all, right-click → Reimport
- When you're already in a file, taking a moment to address warnings there helps prevent accumulation

**Why clean console practices help:**
- Creates a cleaner development environment for everyone
- Makes real errors stand out when they occur
- Prevents warning fatigue where important messages get lost in the noise
- Shows consideration for your teammates who'll work in these files next

A clean console makes debugging faster and more pleasant for everyone on the team.

---

## 8. Parentheses & Complex Conditionals

Use parentheses to clarify intent and extract complex conditions into named variables for better readability.

- **Arithmetic / relational operators:** avoid extra parentheses unless required.
- **Logical/mixed operators:** add parentheses to make intent obvious.
- **Complex conditionals:** introduce well-named local variables to improve readability.

```csharp
// arithmetic: rely on operator precedence
var v = a + b * c;

// logical: use parentheses for clarity
if ((isReady && hasInput) || forceRun)
{
    Run();
}
```

> [!failure] Avoid: deeply nested conditionals
> ```csharp
> if ((player.Alive && player.Armed && !player.Reloading) ||
>     (enemy.Visible && enemy.InRange && Time.time > _lastShotTime + _cooldown))
> {
>     Fire();
> }
> ```

> [!success] Prefer: extract named variables
> ```csharp
> var playerCanShoot = player.Alive && player.Armed && !player.Reloading;
> var enemyTargetable = enemy.Visible && enemy.InRange;
> var cooldownElapsed = Time.time > _lastShotTime + _cooldown;
>
> if (playerCanShoot || (enemyTargetable && cooldownElapsed))
> {
>     Fire();
> }
> ```

---

## 8. `this.` Qualification

Avoid unnecessary noise. Only use `this.` when required to resolve naming conflicts.

Do **not** use `this.` unless needed to disambiguate.

```csharp
_speed = maxSpeed;
UpdateUI();
```

---

## 9. Examples (canonical style)

This example demonstrates all key conventions including design principles (SRP: focused class, OCP: uses interface).

```csharp
// ISelector interface enables Open/Closed: new selectors can be added without changing consumers
public interface ISelector
{
    ISelectable Selection { get; }
    void ClearSelection();
}

// Single Responsibility: only handles raycasting and selection logic
public class RaycastSelector : MonoBehaviour, ISelector
{
    // Serialized fields with [field:SerializeField] pattern
    [field: SerializeField] public LayerMask TargetMask { get; private set; }
    [field: SerializeField] public float MaxDistance { get; private set; } = 10f;

    // Constants
    private const int MaxHits = 10;

    // Non-serialized fields
    private RaycastHit[] _hitBuffer = Array.Empty<RaycastHit>();

    // Properties
    public ISelectable Selection { get; private set; }
    public bool Enabled { get; private set; } = true;  // No "Is" prefix, avoids double negative

    // Unity lifecycle (always private)
    private void Awake()
    {
        _hitBuffer = new RaycastHit[MaxHits];
    }

    private void Update()
    {
        if (!Enabled)
            return;

        var selection = FindClosestSelectable();
        if (selection != null)
            Selection = selection;
    }

    // Public API (Single Responsibility: simple, focused methods)
    public void ClearSelection()
    {
        Selection = null;
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }

    // Private helper (Single Responsibility: does one thing - finds closest selectable)
    private ISelectable FindClosestSelectable()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hitCount = Physics.RaycastNonAlloc(ray, _hitBuffer, MaxDistance, TargetMask);

        ISelectable closest = null;
        var minDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = _hitBuffer[i];
            var isCloser = hit.distance < minDistance;

            if (isCloser && hit.collider.TryGetComponent<ISelectable>(out var selectable))
            {
                closest = selectable;
                minDistance = hit.distance;
            }
        }

        return closest;
    }
}
```

---

## 10. When in doubt
- Prefer **readability over cleverness**.
- Prefer **consistency with nearby code**.
- If a rule feels wrong for a real case, follow your judgment and flag it for a guide update.
