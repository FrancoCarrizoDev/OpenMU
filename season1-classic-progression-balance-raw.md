# Datos crudos de progresión y combate — `season1-classic`

Reporte de inspección estática. No se modificó código de ejecución ni se tomaron decisiones de diseño. Las referencias `archivo:línea` corresponden a este checkout/branch.

## 1. Selección de la configuración Season 1

- El identificador de la variante es `season1-classic`: `src/Persistence/Initialization/VersionSeasonOne/DataInitialization.cs:40-52`.
- El inicializador de mapas Season 1 filtra la lista heredada de Season Six (`MapInitializerTypes`): `src/Persistence/Initialization/VersionSeasonOne/GameMapsInitializer.cs:36-91`.
- Lost Tower y Atlans provienen de las clases VersionSeasonSix, que heredan Version075: `src/Persistence/Initialization/VersionSeasonSix/Maps/LostTower.cs:12-25`, `src/Persistence/Initialization/VersionSeasonSix/Maps/Atlans.cs:12-36`. Atlans solo agrega el NPC Marlon en `:35`; no cambia monstruos. Tarkan es `Version095d.Maps.Tarkan` en `src/Persistence/Initialization/VersionSeasonSix/GameMapsInitializer.cs:34-39`.
- Season One no tiene override propio de esas tres clases de mapa. Agrega spots compactos adicionales mediante `new ClassicSpotsInitializer(...)`: `src/Persistence/Initialization/VersionSeasonOne/GameMapsInitializer.cs:87-91`.

## 2. Primer Reset

### Existencia de configuración para `season1-classic`

`GameConfigurationInitializer` Season One inicializa clases, ítems, NPCs, eventos y mapas (`src/Persistence/Initialization/VersionSeasonOne/GameConfigurationInitializer.cs:68-126`), pero no crea ni asigna `ResetFeaturePlugIn`. El plugin de reset está marcado `IDisabledByDefault`: `src/GameLogic/Resets/ResetFeaturePlugIn.cs:13-22`. Por tanto, en el código de inicialización Season One inspeccionado no hay valores específicos de reset activos; si el plugin se habilita/configura externamente, sus valores salen de `ResetConfiguration`.

### Defaults de `ResetConfiguration`

| Campo | Valor por defecto | Ubicación |
|---|---:|---|
| `ResetLimit` | `null` (sin tope efectivo) | `src/GameLogic/Resets/ResetConfiguration.cs:15-19` |
| `RequiredLevel` | `400` | `src/GameLogic/Resets/ResetConfiguration.cs:21-25` |
| `LevelAfterReset` | `10` | `src/GameLogic/Resets/ResetConfiguration.cs:27-31` |
| `RequiredMoney` | `1` | `src/GameLogic/Resets/ResetConfiguration.cs:33-37` |
| `MultiplyRequiredMoneyByResetCount` | `true` | `src/GameLogic/Resets/ResetConfiguration.cs:39-44` |
| `RequiredResetItem` | `null` | `src/GameLogic/Resets/ResetConfiguration.cs:46-50` |
| `ResetStats` | `true` | `src/GameLogic/Resets/ResetConfiguration.cs:60-64` |
| `PointsPerReset` (legacy) | `1500` | `src/GameLogic/Resets/ResetConfiguration.cs:66-72` |
| `MultiplyPointsByResetCount` (legacy) | `true` | `src/GameLogic/Resets/ResetConfiguration.cs:74-80` |
| `ReplacePointsPerReset` | `true` | `src/GameLogic/Resets/ResetConfiguration.cs:82-86` |
| `PointsTiers` | colección vacía | `src/GameLogic/Resets/ResetConfiguration.cs:88-94` |
| `ItemCostTiers` | colección vacía | `src/GameLogic/Resets/ResetConfiguration.cs:52-58` |
| `MoveHome` | `true` | `src/GameLogic/Resets/ResetConfiguration.cs:96-100` |
| `LogOut` | `true` | `src/GameLogic/Resets/ResetConfiguration.cs:102-106` |

Para un personaje con `Resets = 0`, la progresión por defecto calcula `NextResetCount = 1`, `RequiredZen = max(0, RequiredMoney) * 1 = 1`, `RequiredItemAmount = 0` porque no hay `RequiredResetItem`, y `PointsForReset = 1500` porque no hay tier: `src/GameLogic/Resets/ResetProgressionCalculator.cs:19-32`, `:35-64`. Esto describe el objeto de configuración por defecto, no una configuración Season One activa.

La acción de reset comprueba primero la existencia/configuración del plugin (`src/GameLogic/Resets/ResetCharacterAction.cs:37-63`), el nivel (`:65-71`), y el límite (`:73-77`). Después consume ítems configurados y Zen (`:121-146`); los ítems se buscan por `Group`/`Number` (`:149-177`). El NPC que dispara la acción es Leo the Helper, número `371`: `src/GameLogic/Resets/ResetCharacterNpcPlugin.cs:20-35`.

## 3. Monstruos y spots

### Cómo leer los spots

Las llamadas de mapa usan `CreateMonsterSpawn(number, monster, x, y)`, que crea un área rectangular de un solo punto `(x,y)`, `Quantity = 1`, `SpawnTrigger.Automatic`: `src/Persistence/Initialization/BaseMapInitializer.cs:240-254`, `:257-270`. El código no da nombres semánticos a las zonas; los spots originales están representados por sus coordenadas y las líneas de las declaraciones. Los rangos de líneas de las tablas siguientes son el inventario de todas las declaraciones para cada `NpcDictionary[id]`; cada línea contiene los literales de coordenadas.

Season One añade cuatro áreas rectangulares a Lost Tower, cada una con `Quantity = 7`: `src/Persistence/Initialization/VersionSeasonOne/ClassicSpotsInitializer.cs:17-49`, aplicadas en `:64-81`.

### Lost Tower (mapa 4)

Definición de mapa: `src/Persistence/Initialization/Version075/Maps/LostTower.cs:19-26`; spawns originales: `:52-652`; definiciones de monstruos: `:654-947`.

| ID / monstruo | Level | MaximumHealth | daño físico min–max | DefenseBase | AttackRatePvm | DefenseRatePvm | resistencias (Poison/Ice/Water/Fire) | definición (Number / atributos) | spots originales (líneas) |
|---|---:|---:|---:|---:|---:|---:|---|---|---|
| 34 Cursed Wizard | 54 | 4000 | 160–170 | 95 | 270 | 80 | 5/255, 5/255, 7/255, 7/255 | `LostTower.cs:660-684` | `:164,166,169,174-184,198,200,203,207,210,214,216,227-228,285,304,321,336-343,400-405,461-463,466-467,472,496-498` |
| 35 Death Gorgon | 64 | 6000 | 200–210 | 130 | 320 | 94 | 6/255, 6/255, 6/255, 8/255 | `LostTower.cs:693-717` | `:132,134,287,289,297,300,308,310,312-320,322,324-327,331,359-367,422-444,455-457,476-478` |
| 36 Shadow | 47 | 2800 | 148–153 | 78 | 235 | 67 | 3/255, 3/255, 3/255, 5/255 | `LostTower.cs:726-749` | `:61-131,490` |
| 37 Devil | 60 | 5000 | 180–195 | 115 | 300 | 88 | 5/255, 5/255, 5/255, 7/255 | `LostTower.cs:758-782` | `:236,239,241-244,247-252,254-257,259-268,274,276-277,279,281,288,332-335,344-358,414,416,473` |
| 38 Balrog | 66 | 9000 | 220–240 | 160 | 330 | 99 | 10/255, 10/255, 10/255, 15/255 | `LostTower.cs:791-815` | `:323,453` |
| 39 Poison Shadow | 50 | 3500 | 155–160 | 85 | 250 | 73 | 6/255, 4/255, 4/255, 6/255 | `LostTower.cs:824-849` | `:136-163,165,167-168,170-173,185-194,368-399,488-489,491-495,499-501` |
| 40 Death Knight | 62 | 5500 | 190–200 | 120 | 310 | 91 | 6/255, 6/255, 6/255, 7/255 | `LostTower.cs:858-881` | `:54-60,133,135,253,258,269-273,275,278,280,282-284,286,290-296,298-299,301-303,305-307,309,311,328-330,415,417-421,445-452,454,458-460,474-475,479-487` |
| 41 Death Cow | 57 | 4500 | 170–180 | 110 | 285 | 85 | 5/255, 5/255, 5/255, 7/255 | `LostTower.cs:890-913` | `:195-197,199,201-202,204-206,208-209,211-213,215,217-226,229-235,237-238,240,245-246,406-413,464-465,468-471` |
| 103 Meteorite Trap *(ObjectKind Trap)* | 90 | 1000 | 160–190 | no `DefenseBase` attribute | 450 | 500 | no resistance attributes | `LostTower.cs:920-945` | `:504-651` |

Spots compactos agregados solo por Season One (`X1..X2, Y1..Y2, Quantity=7`): Lost Tower ID 40 Death Knight `(5..13,95..103)`, ID 36 Shadow `(190..198,120..128)`, ID 39 Poison Shadow `(232..240,120..128)`, ID 41 Death Cow `(120..128,230..238)`: `src/Persistence/Initialization/VersionSeasonOne/ClassicSpotsInitializer.cs:44-48`.

### Atlans (mapa 7)

Definición de mapa y spawns: `src/Persistence/Initialization/Version075/Maps/Atlans.cs:19-25`, `:57-397`; definiciones: `:399-661`. El override Season Six solo añade Marlon como NPC/wandering spawn (`src/Persistence/Initialization/VersionSeasonSix/Maps/Atlans.cs:27-36`).

| ID / monstruo | Level | MaximumHealth | daño físico min–max | DefenseBase | AttackRatePvm | DefenseRatePvm | resistencias (Poison/Ice/Water/Fire) | definición (Number / atributos) | spots originales (líneas) |
|---|---:|---:|---:|---:|---:|---:|---|---|---|
| 45 Bahamut | 43 | 2400 | 130–140 | 65 | 215 | 52 | 1/255, 1/255, 1/255, 1/255 | `Atlans.cs:405-428` | `:61-89` |
| 46 Vepar | 45 | 2800 | 135–145 | 70 | 225 | 58 | 2/255, 2/255, 3/255, 2/255 | `Atlans.cs:437-461` | `:90-134` |
| 47 Valkyrie | 46 | 3200 | 140–150 | 75 | 230 | 64 | 6/255, 2/255, 2/255, 2/255 | `Atlans.cs:470-493` | `:208-250` |
| 48 Lizard King | 70 | 9000 | 240–270 | 180 | 350 | 115 | 7/255, 7/255, 7/255, 7/255 | `Atlans.cs:502-526` | `:59,135-170,316,318-320,322-328,333-334,338-345,390-396` |
| 49 Hydra | 74 | 19000 | 250–310 | 200 | 430 | 125 | 12/255, 12/255, 12/255, 12/255 | `Atlans.cs:535-559` | `:296-297,329-330` |
| 50 Sea Worm | 74 | 19000 | 250–310 | 200 | 430 | 125 | 12/255, 12/255, 12/255, 12/255 | `Atlans.cs:568-591` | no `NpcDictionary[50]` spawn declaration found in this file |
| 51 Great Bahamut | 66 | 7000 | 210–230 | 150 | 330 | 98 | 6/255, 6/255, 6/255, 6/255 | `Atlans.cs:600-623` | `:60,251-286,289-295,298-309,317,358,378,384-389` |
| 52 Silver Valkyrie | 68 | 8000 | 230–260 | 170 | 340 | 110 | 7/255, 7/255, 7/255, 7/255 | `Atlans.cs:632-656` | `:171-207,310-315,321,331-332,335-337,346-357,359-377,379-383` |

El código contiene la definición de Sea Worm (ID 50), pero no una llamada `CreateMonsterSpawn(...NpcDictionary[50]...)` en este mapa. El NPC Guard ID 240 en `Atlans.cs:53` no es monstruo y no se incluye en la tabla.

### Tarkan (mapa 8)

Definición de mapa y spawns: `src/Persistence/Initialization/Version095d/Maps/Tarkan.cs:27-35`, `:34-253`; definiciones: `:255-485`.

| ID / monstruo | Level | MaximumHealth | daño físico min–max | DefenseBase | AttackRatePvm | DefenseRatePvm | resistencias (Poison/Ice/Water/Fire) | definición (Number / atributos) | spots originales (líneas) |
|---|---:|---:|---:|---:|---:|---:|---|---|---|
| 57 Iron Wheel | 80 | 17000 | 280–330 | 215 | 446 | 150 | 9/255, 9/255, 9/255, 9/255 | `Tarkan.cs:261-284` | `:91,118-129,131-137,139-140,252` |
| 58 Tantallos | 83 | 22000 | 335–385 | 250 | 500 | 175 | 9/255, 9/255, 9/255, 9/255 | `Tarkan.cs:293-317` | `:38,41,130,141-158,160-171,173-182,184-192,207,209,219,221,233-234,237-238,249-251` |
| 59 Zaikan | 90 | 34000 | 510–590 | 400 | 550 | 185 | 13/255, 13/255, 13/255, 15/255 | `Tarkan.cs:326-350` | `:40` |
| 60 Bloody Wolf | 76 | 13500 | 260–300 | 200 | 410 | 130 | 8/255, 8/255, 8/255, 8/255 | `Tarkan.cs:359-382` | `:57,73,89-90,92-117,138,159` |
| 61 Beam Knight | 84 | 25000 | 375–425 | 275 | 530 | 190 | 10/255, 10/255, 10/255, 10/255 | `Tarkan.cs:391-415` | `:36-37,39,43-45,53,172,183,193-206,208,210-218,220,223-232,235-236,239-248` |
| 62 Mutant | 72 | 10000 | 250–280 | 190 | 365 | 120 | 8/255, 8/255, 8/255, 8/255 | `Tarkan.cs:424-447` | `:42,46-52,54-56,58-72,74-88` |
| 63 Death Beam Knight | 93 | 40000 | 590–650 | 420 | 575 | 220 | 13/255, 13/255, 13/255, 17/255 | `Tarkan.cs:456-480` | `:222` |

## 4. Fórmula exacta de daño físico

### Camino de ejecución

- Monster → Player: `src/GameLogic/NPC/Monster.cs:113-121` llama `target.AttackByAsync(this, null, false)`; `Player.AttackByAsync` calcula mediante `attacker.CalculateDamageAsync(this, ...)` en `src/GameLogic/Player.cs:658-711`.
- Player → Monster: `AttackableNpcBase.AttackByAsync` llama el mismo cálculo en `src/GameLogic/NPC/AttackableNpcBase.cs:104-121`.
- Fórmula común: `src/GameLogic/AttackableExtensions.cs:68-306`.

### Defensa, acierto y daño

1. Antes de calcular daño se tira el acierto en `AttackableExtensions.cs:70-73`. Si no acierta, devuelve `HitInfo(0,0,...)`.
2. Si no ignora defensa, la defensa usada es

   ```text
   defense = int((defender.Attributes[defenseAttribute]
                  + defender.Attributes[Stats.GreaterDefenseBonus])
                 * defender.Attributes[Stats.DefenseDecrement])
   defense = max(defense, 0)
   ```

   Ubicación exacta: `src/GameLogic/AttackableExtensions.cs:86-99`. `GetDefenseAttribute` elige `Stats.DefensePvp` solo para Player/Player; en los demás casos, incluido Monster↔Player, devuelve `Stats.DefensePvm`: `:718-726`.
3. Para daño físico, `GetBaseDmg` obtiene `MinimumPhysBaseDmg + skillMinimumDamage` y `MaximumPhysBaseDmg + skillMaximumDamage`: `src/GameLogic/AttackableExtensions.cs:837-866`, específicamente `:855-857`.
4. Golpe físico normal: se suma el bonus Berserker, se sortea entero entre min/max (el máximo es exclusivo según `Rand.NextInt`), y luego se resta defensa:

   ```text
   dmg = random_int(baseMinDamage + BerserkerMinPhysDmgBonus,
                   baseMaxDamage + BerserkerMaxPhysDmgBonus)
   dmg = int((dmg * duelDmgDec) - defense)
   ```

   Ubicación: `src/GameLogic/AttackableExtensions.cs:117-137`. Golpe crítico/excelente tiene las ramas de `:105-115`; la resta de `defense` para físico está en la expresión exacta de `:137`.
5. Después se aplican ajustes posteriores: `GreaterDamageBonus` (`:185`), decremento físico Berserker (`:193-197`), reducción de 70% por `Overrates` en PvM cuando `DefenseRatePvm > AttackRatePvm` (`:205-208`, condición en `:728-731`), `ArmorDamageDecrease` (`:210`), mínimo de daño por nivel (`:212-218`), `AttackDamageIncrease` y `DamageReceiveDecrement` (`:220-224`), multiplicador/bonus de skill (`:226-248`), Soul Barrier (`:258-266`) y bonus final (`:268`).

`DefenseRatePvm` no se resta como puntos en la fórmula de daño: se usa para la probabilidad de acertar. El valor efectivo es `DefenseRatePvm + IncreaseBlockBonus`: `src/GameLogic/AttackableExtensions.cs:678-686`; la probabilidad es `0.03` si `defenseRate >= attackRate`, o `1 - defenseRate/attackRate` si es menor: `:694-715`. Para monstruos, `MonsterAttributeHolder` expone `DefensePvm` desde `DefenseBase` (con aumento de monstruo invocado si aplica): `src/GameLogic/Attributes/MonsterAttributeHolder.cs:16-27`.

## 5. Stats base, puntos por nivel y derivaciones

### Clases creadas en Season One

`VersionSeasonOne.CharacterClassInitialization` crea Dark Knight→Blade Knight, Dark Wizard→Soul Master, Fairy Elf→Muse Elf, Magic Gladiator y Dark Lord: `src/Persistence/Initialization/VersionSeasonOne/CharacterClassInitialization.cs:29-43`. En las parejas, ambas generaciones llaman al mismo constructor de clase y por ello comparten los valores de stats/formulas de abajo. Dark Lord tiene `canGetCreated = true` en `:42`; Magic Gladiator también está habilitado en `:40`.

El otorgamiento efectivo por level-up es `Level++` seguido de `LevelUpPoints += Attributes[Stats.PointsPerLevelUp]`: `src/GameLogic/PlayerExperience.cs:229-234`.

| Línea de clase | Puntos por level-up | STR inicial | AGI inicial | VIT inicial | ENE inicial | LEA inicial | HP máxima (sin equipo/modificadores) | DefenseBase por AGI | DefenseRatePvm por AGI | daño físico base derivado de stats |
|---|---:|---:|---:|---:|---:|---:|---|---|---|---|
| Dark Knight / Blade Knight — `src/Persistence/Initialization/CharacterClasses/ClassDarkKnight.cs:36-41` | 5 | 28 | 20 | 25 | 10 | — | `35 + 2*TotalLevel + 3*TotalVitality` (`:68-69`, base `:106-108`) | `TotalAgility/3` (`:51`) | `TotalAgility/3` (`:52`) | min `TotalStrength/6`; max `TotalStrength/4` (`:70-71`) |
| Dark Wizard / Soul Master — `ClassDarkWizard.cs:36-41` | 5 | 18 | 18 | 15 | 30 | — | `30 + TotalLevel + 2*TotalVitality` (`:68-69`, base `:111`) | `TotalAgility/4` (`:51`) | `TotalAgility/3` (`:52`) | min `TotalStrength/8`; max `TotalStrength/4` (`:70-71`) |
| Fairy Elf / Muse Elf — `ClassFairyElf.cs:39-44` | 5 | 22 | 25 | 20 | 15 | — | `39 + TotalLevel + 2*TotalVitality` (`:73-74`, base `:119`) | `TotalAgility/10` (`:56`) | `TotalAgility/4` (`:57`) | arco: min `TotalAgility/7 + TotalStrength/14`, max `TotalAgility/4 + TotalStrength/8`; melee: min `(TotalStrength+TotalAgility)/7`, max `(TotalStrength+TotalAgility)/4` (`:75-92`) |
| Magic Gladiator — `ClassMagicGladiator.cs:39-44` | 7 | 26 | 26 | 26 | 26 | — | `57 + TotalLevel + 2*TotalVitality` (`:71-72`, base `:121`) | `TotalAgility/5` (`:54`) | `TotalAgility/3` (`:55`) | min `TotalStrength/6 + TotalEnergy/12`; max `TotalStrength/4 + TotalEnergy/8` (`:73-76`) |
| Dark Lord — `ClassDarkLord.cs:40-46` | 7 | 26 | 20 | 20 | 15 | 25 | `48.5 + 1.5*TotalLevel + 2*TotalVitality` (`:77-78`, base `:128-130`) | `TotalAgility/7` (`:57`) | `TotalAgility/7` (`:58`) | min `TotalStrength/7 + TotalEnergy/14`; max `TotalStrength/5 + TotalEnergy/10` (`:79-82`) |

### Ensamblaje común de HP, defensa y ataque

- `TotalLevel = Level + MasterLevel`; `TotalStrength`, `TotalAgility`, `TotalVitality` y `TotalEnergy` parten de sus stats base: `src/Persistence/Initialization/CharacterClasses/CharacterClassInitialization.cs:92-100`. Season One no crea clases Master, por lo que para las clases de esta tabla `TotalLevel` normalmente coincide con `Level`.
- La defensa común se arma como `DefenseBase = DefenseShield + aporte de clase`, `DefenseFinal = 0.5 * DefenseBase`, `DefensePvm = DefenseFinal`: `src/Persistence/Initialization/CharacterClasses/CharacterClassInitialization.cs:102-105`. Los ítems/escudo pueden agregar relaciones adicionales en `:132-145`.
- El daño físico final común parte de `Minimum/MaximumPhysBaseDmgByWeapon`, agrega `BaseMinDamageBonus`/`BaseMaxDamageBonus`, agrega `PhysicalBaseDmg`, y aplica `PhysicalBaseDmgIncrease`: `src/Persistence/Initialization/CharacterClasses/CharacterClassInitialization.cs:111-121`. Por eso las expresiones de la tabla son el aporte de stats de clase; arma, skill y power-ups se ensamblan adicionalmente en `src/GameLogic/AttackableExtensions.cs:759-827`, `:837-866`.
- La defensa efectiva de un jugador al recibir el golpe también pasa por `DefenseDecrement` y `GreaterDefenseBonus` en `AttackableExtensions.cs:86-99`; la defensa efectiva de un monstruo parte de su `DefenseBase` en `MonsterAttributeHolder.cs:16-27`.

## Referencias de nombres de atributos usados

Los campos de monstruos son atributos persistidos `Stats.Level`, `MaximumHealth`, `MinimumPhysBaseDmg`, `MaximumPhysBaseDmg`, `DefenseBase`, `AttackRatePvm`, `DefenseRatePvm` y resistencias elementales, tal como se ven en cada diccionario `attributes` de las definiciones citadas. No se observó un atributo separado de `AttackDamage`; en este motor el equivalente físico es el par `MinimumPhysBaseDmg`/`MaximumPhysBaseDmg`.
