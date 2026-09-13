# Relevamiento de tiendas NPC — season1-classic

Fecha del relevamiento: 2026-09-06. Investigación de solo lectura sobre el seed C# que materializa la configuración inicial; no se modificó código de producto. La búsqueda global de MerchantStore/CreateMerchantStore no encontró listas de tiendas equivalentes en JSON ni scripts SQL de inicialización: el contenido relevado está definido en los inicializadores C# y se persiste como parte de la configuración.

## Resultado ejecutivo

La inicialización de la clave season1-classic produce 22 definiciones de NPC con MerchantStore no vacío, 14 plantillas lógicas y 589 instancias de ítems (las ocho copias de la tienda de pociones se consolidan como una sola tabla). De esas definiciones, 13 NPC tienen spawn en mapas que la Season 1 conserva; 9 quedan únicamente configuradas en mapas excluidos o sin spawn estático localizado.

No se encontraron jewels vendibles ni opciones Excellent en las 589 instancias. Sí se encontraron muchas armas, escudos y sets con nivel de ítem +3 y/o opción regular +4, además de Luck/Skill: esto no cumple el modelo MU Malvinas para equipo básico; Bolo agrega opción regular +12. No hay tienda NPC en Tarkan: su initializer solo define monstruos, sin CreateNpcSpawns.

Los precios de las tablas son el precio de compra que paga el jugador (Zen) para el objeto/lote mostrado, calculado en memoria con el seed actual. No son literales de la tienda: BuyNpcItemAction.cs:93-99 llama a ItemPriceCalculator.CalculateFinalBuyingPrice (ItemPriceCalculator.cs:387-392), que aplica valores de definiciones, nivel, durabilidad/stack y opciones.

## Convenciones de la tabla

- Nivel es el nivel del ítem (+0, +1, etc.), no el nivel de la opción.
- Opción regular +4/+10/+12 es el valor visible derivado de optionLevel 1/2/3; L es Luck y Skill es habilidad del arma/escudo.
- Durabilidad / lote: en consumibles corresponde al lote (por ejemplo, x50 o x255); en equipo es la durabilidad base de la definición.
- Todos los renglones tienen Excelente = No; se deja la columna explícita para que el inventario sea auditable.
- Los grupos/números g:n son los identificadores de la definición de ítem. Los nombres se preservan tal como resultan del seed; por eso aparece Mistery en Rhea.

## NPCs con tienda y dónde se asignan

| ID | NPC | Asignación de MerchantStore | Spawn / mapa | Estado en season1-classic |
|---:|---|---|---|---|
| 230 | Alex | VersionSeasonSix/NpcInitialization.cs:63-70 | VersionSeasonSix/Maps/Lorencia.cs:35 | Activo (Lorencia) |
| 231 | Thompson the Merchant | VersionSeasonSix/NpcInitialization.cs:74-81 | No hay spawn estático localizado | Configurado, sin spawn localizado |
| 242 | Elf Lala | Version075/NpcInitialization.cs:84-92 | Version075/Maps/Noria.cs:47 | Activo (Noria) |
| 243 | Eo the Craftsman | Version075/NpcInitialization.cs:95-103 | Version075/Maps/Noria.cs:48 | Activo (Noria) |
| 244 | Caren the Barmaid | Version075/NpcInitialization.cs:106-114 | Version075/Maps/Devias.cs:46 | Activo (Devias) |
| 245 | Izabel The Wizard | Version075/NpcInitialization.cs:117-125 | Version075/Maps/Devias.cs:47 | Activo (Devias) |
| 246 | Zienna The Weapons Merchant | Version075/NpcInitialization.cs:128-136 | Version075/Maps/Devias.cs:48 | Activo (Devias) |
| 248 | Wandering Merchant Martin | Version075/NpcInitialization.cs:168-176 | Version075/Maps/Lorencia.cs:46 | Activo (Lorencia) |
| 250 | Wandering Merchant Harold | Version075/NpcInitialization.cs:208-216 | Version075/Maps/Lorencia.cs:55 | Activo (Lorencia) |
| 251 | Hanzo The Blacksmith | Version075/NpcInitialization.cs:219-227 | Version075/Maps/Lorencia.cs:56 | Activo (Lorencia) |
| 253 | Potion Girl Amy | Version075/NpcInitialization.cs:230-238 | Version075/Maps/Lorencia.cs:57; Noria:45-46; LostTower:47 | Activo (3 mapas) |
| 254 | Pasi The Mage | Version075/NpcInitialization.cs:241-249 | Version075/Maps/Lorencia.cs:58 | Activo (Lorencia) |
| 255 | Lumen the Barmaid | Version075/NpcInitialization.cs:252-260 | Version075/Maps/Lorencia.cs:59 | Activo (Lorencia) |
| 259 | Oracle Layla | VersionSeasonSix/NpcInitialization.cs:169-178 | VersionSeasonSix/Maps/KalimaBase.cs:32-35 | Activo (Kalima 1–7) |
| 376 | Pamela the Supplier | VersionSeasonSix/NpcInitialization.cs:190-198 | VersionSeasonSix/Maps/ValleyOfLoren.cs:51 | Inactivo: mapa excluido |
| 377 | Angela the Supplier | VersionSeasonSix/NpcInitialization.cs:201-209 | VersionSeasonSix/Maps/ValleyOfLoren.cs:52 | Inactivo: mapa excluido |
| 415 | Silvia | VersionSeasonSix/NpcInitialization.cs:560-568 | VersionSeasonSix/Maps/Elvenland.cs:47 | Inactivo: mapa excluido |
| 416 | Rhea | VersionSeasonSix/NpcInitialization.cs:571-579 | VersionSeasonSix/Maps/Elvenland.cs:48 | Inactivo: mapa excluido |
| 417 | Marce | VersionSeasonSix/NpcInitialization.cs:582-590 | VersionSeasonSix/Maps/Elvenland.cs:49 | Inactivo: mapa excluido |
| 545 | Christine the General Goods Merchant | VersionSeasonSix/NpcInitialization.cs:986-994 | VersionSeasonSix/Maps/LorenMarket.cs:47 | Inactivo: mapa excluido |
| 577 | Leina the General Goods Merchant | VersionSeasonSix/NpcInitialization.cs:963-971 | VersionSeasonSix/Maps/Karutan1.cs:45 | Inactivo: mapa excluido |
| 578 | Weapons Merchant Bolo | VersionSeasonSix/NpcInitialization.cs:974-982 | VersionSeasonSix/Maps/Karutan1.cs:46 | Inactivo: mapa excluido |

Las filas con estado “Activo” son las tiendas que el jugador puede encontrar en el roster de mapas Season 1. Amy se instancia en tres mapas y mantiene una única definición de tienda; Layla se instancia en Kalima 1–7. Thompson tiene la tienda en configuración pero no se encontró una referencia de spawn estático.

## Inventario completo por plantilla de tienda

Cada tabla siguiente contiene una vez cada renglón distinto de la plantilla; el listado de NPCs que la recibe está en la primera columna del resumen y en la tabla anterior. Por tanto, no se omiten duplicados de tiendas: se consolidan solo porque son byte a byte la misma lista.

| Plantilla | NPC(s) | Ítems | Definición |
|---|---|---:|---|
| Potion / general goods | 231 Thompson; 253 Amy; 259 Oracle Layla; 376 Pamela; 377 Angela; 415 Silvia; 545 Christine; 577 Leina | 32 | VersionSeasonSix/MerchantStores.cs:16-68 (override efectiva de la virtual de Version075) |
| Wandering Merchant | 248 Martin; 250 Harold | 25 | VersionSeasonSix/MerchantStores.cs:72-110 |
| Hanzo the Blacksmith | 251 Hanzo | 25 | VersionSeasonSix/MerchantStores.cs:113-151 |
| Pasi the Mage | 254 Pasi | 32 | VersionSeasonSix/MerchantStores.cs:154-200 |
| Elf Lala | 242 Elf Lala | 55 | VersionSeasonSix/MerchantStores.cs:203-282 |
| Izabel the Wizard | 245 Izabel | 42 | VersionSeasonSix/MerchantStores.cs:285-348 |
| Eo the Craftsman | 243 Eo | 20 | VersionSeasonSix/MerchantStores.cs:351-385 |
| Zienna | 246 Zienna | 16 | VersionSeasonSix/MerchantStores.cs:388-418 |
| Lumen the Barmaid | 255 Lumen | 43 | VersionSeasonSix/MerchantStores.cs:421-476 |
| Caren the Barmaid | 244 Caren | 11 | VersionSeasonSix/MerchantStores.cs:479-499 |
| Alex | 230 Alex | 8 | VersionSeasonSix/MerchantStores.cs:505-523 |
| Marce | 417 Marce | 5 | VersionSeasonSix/MerchantStores.cs:525-539 |
| Rhea | 416 Rhea | 20 | VersionSeasonSix/MerchantStores.cs:541-574 |
| Weapons Merchant Bolo | 578 Bolo | 6 | VersionSeasonSix/MerchantStores.cs:576-591 |

### Potion / general goods — 231 Thompson; 253 Amy; 259 Oracle Layla; 376 Pamela; 377 Angela; 415 Silvia; 545 Christine; 577 Leina

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:16-68 (override efectiva de la virtual de Version075).

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Apple (14:0) | +0 | 1 | — | No | 20 |
| Small Healing Potion (14:1) | +0 | 1 | — | No | 80 |
| Medium Healing Potion (14:2) | +0 | 1 | — | No | 330 |
| Large Healing Potion (14:3) | +0 | 1 | — | No | 1.500 |
| Small Mana Potion (14:4) | +0 | 1 | — | No | 80 |
| Medium Mana Potion (14:5) | +0 | 1 | — | No | 330 |
| Large Mana Potion (14:6) | +0 | 1 | — | No | 1.500 |
| Antidote (14:8) | +0 | 1 | — | No | 80 |
| Apple (14:0) | +0 | 50 | — | No | 1.000 |
| Small Healing Potion (14:1) | +0 | 50 | — | No | 4.000 |
| Medium Healing Potion (14:2) | +0 | 50 | — | No | 16.500 |
| Large Healing Potion (14:3) | +0 | 50 | — | No | 75.000 |
| Small Mana Potion (14:4) | +0 | 50 | — | No | 4.000 |
| Medium Mana Potion (14:5) | +0 | 50 | — | No | 16.500 |
| Large Mana Potion (14:6) | +0 | 50 | — | No | 75.000 |
| Antidote (14:8) | +0 | 50 | — | No | 4.000 |
| Apple (14:0) | +1 | 1 | — | No | 40 |
| Small Healing Potion (14:1) | +1 | 1 | — | No | 160 |
| Medium Healing Potion (14:2) | +1 | 1 | — | No | 660 |
| Large Healing Potion (14:3) | +1 | 1 | — | No | 3.000 |
| Apple (14:0) | +1 | 50 | — | No | 2.000 |
| Small Healing Potion (14:1) | +1 | 50 | — | No | 8.000 |
| Medium Healing Potion (14:2) | +1 | 50 | — | No | 33.000 |
| Large Healing Potion (14:3) | +1 | 50 | — | No | 150.000 |
| Bolt (4:7) | +0 | 255 | — | No | 100 |
| Bolt (4:7) | +1 | 255 | — | No | 1.400 |
| Bolt (4:7) | +2 | 255 | — | No | 2.200 |
| Arrows (4:15) | +0 | 255 | — | No | 70 |
| Arrows (4:15) | +1 | 255 | — | No | 1.200 |
| Arrows (4:15) | +2 | 255 | — | No | 2.000 |
| Town Portal Scroll (14:10) | +0 | 1 | — | No | 750 |
| Armor of Guardsman (13:29) | +0 | 1 | — | No | 5.000 |

### Wandering Merchant — 248 Martin; 250 Harold

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:72-110.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Leather Helm (7:5) | +0 | 30 | Opción regular +4; Luck | No | 610 |
| Bronze Helm (7:0) | +2 | 34 | Opción regular +4; Luck | No | 7.700 |
| Scale Helm (7:6) | +3 | 40 | Opción regular +4; Luck | No | 23.100 |
| Brass Helm (7:8) | +3 | 44 | Opción regular +4; Luck | No | 43.200 |
| Leather Armor (8:5) | +0 | 30 | Opción regular +4; Luck | No | 1.400 |
| Bronze Armor (8:0) | +2 | 34 | Opción regular +4; Luck | No | 9.400 |
| Scale Armor (8:6) | +3 | 40 | Opción regular +4; Luck | No | 26.500 |
| Brass Armor (8:8) | +3 | 44 | Opción regular +4; Luck | No | 48.200 |
| Bronze Pants (9:0) | +2 | 34 | Opción regular +4; Luck | No | 6.900 |
| Scale Pants (9:6) | +3 | 40 | Opción regular +4; Luck | No | 21.500 |
| Brass Pants (9:8) | +3 | 44 | Opción regular +4; Luck | No | 40.800 |
| Leather Pants (9:5) | +0 | 30 | Opción regular +4; Luck | No | 960 |
| Bronze Boots (11:0) | +2 | 34 | Opción regular +4; Luck | No | 4.800 |
| Scale Boots (11:6) | +3 | 40 | Opción regular +4; Luck | No | 17.200 |
| Brass Boots (11:8) | +3 | 44 | Opción regular +4; Luck | No | 34.200 |
| Leather Boots (11:5) | +0 | 30 | Opción regular +4; Luck | No | 480 |
| Bronze Gloves (10:0) | +2 | 34 | Opción regular +4; Luck | No | 5.500 |
| Scale Gloves (10:6) | +3 | 40 | Opción regular +4; Luck | No | 17.200 |
| Brass Gloves (10:8) | +3 | 44 | Opción regular +4; Luck | No | 34.200 |
| Leather Gloves (10:5) | +0 | 30 | Opción regular +4; Luck | No | 370 |
| Plate Pants (9:9) | +3 | 50 | Opción regular +4; Luck | No | 68.700 |
| Plate Gloves (10:9) | +3 | 50 | Opción regular +4; Luck | No | 59.300 |
| Plate Helm (7:9) | +3 | 50 | Opción regular +4; Luck | No | 72.000 |
| Plate Boots (11:9) | +3 | 50 | Opción regular +4; Luck | No | 59.300 |
| Plate Armor (8:9) | +3 | 50 | Opción regular +4; Luck | No | 78.900 |

### Hanzo the Blacksmith — 251 Hanzo

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:113-151.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Small Shield (6:0) | +0 | 22 | Opción regular +4; Luck | No | 230 |
| Buckler (6:4) | +1 | 24 | Opción regular +4; Skill; Luck | No | 2.300 |
| Horn Shield (6:1) | +2 | 28 | Opción regular +4; Luck | No | 2.600 |
| Kite Shield (6:2) | +3 | 32 | Opción regular +4; Luck | No | 5.500 |
| Skull Shield (6:6) | +3 | 34 | Opción regular +4; Skill; Luck | No | 18.800 |
| Big Round Shield (6:10) | +3 | 35 | Opción regular +4; Skill; Luck | No | 24.800 |
| Plate Shield (6:9) | +3 | 38 | Opción regular +4; Skill; Luck | No | 43.100 |
| Spiked Shield (6:7) | +3 | 40 | Opción regular +4; Skill; Luck | No | 60.400 |
| Dragon Slayer Shield (6:5) | +3 | 44 | Opción regular +4; Skill; Luck | No | 81.700 |
| Tower Shield (6:8) | +3 | 46 | Opción regular +4; Skill; Luck | No | 107.200 |
| Serpent Shield (6:11) | +3 | 48 | Opción regular +4; Skill; Luck | No | 137.400 |
| Bronze Shield (6:12) | +3 | 52 | Opción regular +4; Skill; Luck | No | 204.800 |
| Short Sword (0:1) | +0 | 22 | Opción regular +4; Luck | No | 230 |
| Hand Axe (1:1) | +1 | 20 | Opción regular +4; Luck | No | 610 |
| Morning Star (2:1) | +2 | 25 | Opción regular +4; Luck | No | 4.400 |
| Rapier (0:2) | +2 | 23 | Opción regular +4; Luck | No | 2.600 |
| Double Axe (1:2) | +2 | 26 | Opción regular +4; Luck | No | 4.900 |
| Sword of Assassin (0:4) | +3 | 24 | Opción regular +4; Skill; Luck | No | 13.800 |
| Morning Star (2:1) | +3 | 25 | Opción regular +4; Skill; Luck | No | 15.400 |
| Tomahawk (1:3) | +3 | 28 | Opción regular +4; Skill; Luck | No | 24.800 |
| Kris (0:0) | +2 | 20 | Opción regular +4; Luck | No | 1.600 |
| Gladius (0:6) | +3 | 30 | Opción regular +4; Skill; Luck | No | 29.400 |
| Falchion (0:7) | +3 | 34 | Opción regular +4; Skill; Luck | No | 40.100 |
| Serpent Sword (0:8) | +3 | 36 | Opción regular +4; Luck | No | 24.100 |
| Blade (0:5) | +3 | 39 | Opción regular +4; Skill; Luck | No | 86.400 |

### Pasi the Mage — 254 Pasi

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:154-200.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Scroll of Fire Ball (15:3) | +0 | 1 | — | No | 300 |
| Scroll of Power Wave (15:10) | +0 | 1 | — | No | 1.100 |
| Scroll of Lighting (15:2) | +0 | 1 | — | No | 3.000 |
| Scroll of Meteorite (15:1) | +0 | 1 | — | No | 11.000 |
| Scroll of Teleport (15:5) | +0 | 1 | — | No | 5.000 |
| Scroll of Ice (15:6) | +0 | 1 | — | No | 14.000 |
| Scroll of Poison (15:0) | +0 | 1 | — | No | 17.000 |
| Orb of Impale (12:13) | +0 | 1 | — | No | 10.000 |
| Scroll of Flame (15:4) | +0 | 1 | — | No | 21.000 |
| Scroll of Twister (15:7) | +0 | 1 | — | No | 25.000 |
| Scroll of Evil Spirit (15:8) | +0 | 1 | — | No | 35.000 |
| Scroll of Hellfire (15:9) | +0 | 1 | — | No | 60.000 |
| Orb of Twisting Slash (12:7) | +0 | 1 | — | No | 29.000 |
| Pad Helm (7:2) | +0 | 28 | Opción regular +4; Luck | No | 480 |
| Bone Helm (7:4) | +2 | 30 | Opción regular +4; Luck | No | 9.400 |
| Sphinx Mask (7:7) | +3 | 36 | Opción regular +4; Luck | No | 34.200 |
| Skull Staff (5:0) | +0 | 20 | Opción regular +4; Luck | No | 480 |
| Pad Armor (8:2) | +0 | 28 | Opción regular +4; Luck | No | 1.400 |
| Bone Armor (8:4) | +2 | 30 | Opción regular +4; Luck | No | 13.500 |
| Sphinx Armor (8:7) | +3 | 36 | Opción regular +4; Luck | No | 48.200 |
| Angelic Staff (5:1) | +2 | 38 | Opción regular +4; Luck | No | 9.400 |
| Pad Pants (9:2) | +0 | 28 | Opción regular +4; Luck | No | 960 |
| Bone Pants (9:4) | +2 | 30 | Opción regular +4; Luck | No | 11.300 |
| Sphinx Pants (9:7) | +3 | 36 | Opción regular +4; Luck | No | 38.500 |
| Pad Boots (11:2) | +0 | 28 | Opción regular +4; Luck | No | 370 |
| Bone Boots (11:4) | +2 | 30 | Opción regular +4; Luck | No | 7.700 |
| Serpent Staff (5:2) | +3 | 50 | Opción regular +4; Luck | No | 30.200 |
| Sphinx Boots (11:7) | +3 | 36 | Opción regular +4; Luck | No | 30.200 |
| Pad Gloves (10:2) | +0 | 28 | Opción regular +4; Luck | No | 290 |
| Bone Gloves (10:4) | +2 | 30 | Opción regular +4; Luck | No | 6.200 |
| Sphinx Gloves (10:7) | +3 | 36 | Opción regular +4; Luck | No | 26.500 |
| Thunder Staff (5:3) | +3 | 60 | Opción regular +4; Luck | No | 59.300 |

### Elf Lala — 242 Elf Lala

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:203-282.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Apple (14:0) | +0 | 1 | — | No | 20 |
| Small Healing Potion (14:1) | +0 | 1 | — | No | 80 |
| Medium Healing Potion (14:2) | +0 | 1 | — | No | 330 |
| Large Healing Potion (14:3) | +0 | 1 | — | No | 1.500 |
| Small Mana Potion (14:4) | +0 | 1 | — | No | 80 |
| Medium Mana Potion (14:5) | +0 | 1 | — | No | 330 |
| Large Mana Potion (14:6) | +0 | 1 | — | No | 1.500 |
| Antidote (14:8) | +0 | 1 | — | No | 80 |
| Apple (14:0) | +0 | 50 | — | No | 1.000 |
| Small Healing Potion (14:1) | +0 | 50 | — | No | 4.000 |
| Medium Healing Potion (14:2) | +0 | 50 | — | No | 16.500 |
| Large Healing Potion (14:3) | +0 | 50 | — | No | 75.000 |
| Small Mana Potion (14:4) | +0 | 50 | — | No | 4.000 |
| Medium Mana Potion (14:5) | +0 | 50 | — | No | 16.500 |
| Large Mana Potion (14:6) | +0 | 50 | — | No | 75.000 |
| Antidote (14:8) | +0 | 50 | — | No | 4.000 |
| Apple (14:0) | +1 | 1 | — | No | 40 |
| Small Healing Potion (14:1) | +1 | 1 | — | No | 160 |
| Medium Healing Potion (14:2) | +1 | 1 | — | No | 660 |
| Large Healing Potion (14:3) | +1 | 1 | — | No | 3.000 |
| Apple (14:0) | +1 | 50 | — | No | 2.000 |
| Small Healing Potion (14:1) | +1 | 50 | — | No | 8.000 |
| Medium Healing Potion (14:2) | +1 | 50 | — | No | 33.000 |
| Large Healing Potion (14:3) | +1 | 50 | — | No | 150.000 |
| Orb of Healing (12:8) | +0 | 1 | — | No | 800 |
| Orb of Greater Defense (12:9) | +0 | 1 | — | No | 3.000 |
| Orb of Greater Damage (12:10) | +0 | 1 | — | No | 7.000 |
| Orb of Summoning (12:11) | +0 | 1 | — | No | 150 |
| Orb of Summoning (12:11) | +1 | 1 | — | No | 150 |
| Orb of Summoning (12:11) | +2 | 1 | — | No | 150 |
| Orb of Summoning (12:11) | +3 | 1 | — | No | 150 |
| Orb of Summoning (12:11) | +4 | 1 | — | No | 150 |
| Vine Helm (7:10) | +0 | 22 | Opción regular +4; Luck | No | 610 |
| Vine Armor (8:10) | +0 | 22 | Opción regular +4; Luck | No | 1.400 |
| Vine Pants (9:10) | +0 | 22 | Opción regular +4; Luck | No | 960 |
| Vine Gloves (10:10) | +3 | 22 | Opción regular +4; Luck | No | 2.400 |
| Vine Boots (11:10) | +3 | 22 | Opción regular +4; Luck | No | 2.800 |
| Silk Helm (7:11) | +2 | 26 | Opción regular +4; Luck | No | 7.700 |
| Silk Armor (8:11) | +2 | 26 | Opción regular +4; Luck | No | 11.300 |
| Silk Pants (9:11) | +2 | 26 | Opción regular +4; Luck | No | 9.400 |
| Silk Gloves (10:11) | +2 | 26 | Opción regular +4; Luck | No | 6.200 |
| Silk Boots (11:11) | +2 | 26 | Opción regular +4; Luck | No | 6.900 |
| Wind Helm (7:12) | +3 | 32 | Opción regular +4; Luck | No | 26.500 |
| Wind Armor (8:12) | +3 | 32 | Opción regular +4; Luck | No | 34.200 |
| Wind Pants (9:12) | +3 | 32 | Opción regular +4; Luck | No | 30.200 |
| Wind Gloves (10:12) | +3 | 32 | Opción regular +4; Luck | No | 23.100 |
| Wind Boots (11:12) | +3 | 32 | Opción regular +4; Luck | No | 24.800 |
| Armor of Guardsman (13:29) | +0 | 1 | — | No | 5.000 |
| Town Portal Scroll (14:10) | +0 | 1 | — | No | 750 |
| Bolt (4:7) | +0 | 255 | — | No | 100 |
| Bolt (4:7) | +1 | 255 | — | No | 1.400 |
| Bolt (4:7) | +2 | 255 | — | No | 2.200 |
| Arrows (4:15) | +0 | 255 | — | No | 70 |
| Arrows (4:15) | +1 | 255 | — | No | 1.200 |
| Arrows (4:15) | +2 | 255 | — | No | 2.000 |

### Izabel the Wizard — 245 Izabel

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:285-348.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Apple (14:0) | +0 | 1 | — | No | 20 |
| Small Healing Potion (14:1) | +0 | 1 | — | No | 80 |
| Medium Healing Potion (14:2) | +0 | 1 | — | No | 330 |
| Large Healing Potion (14:3) | +0 | 1 | — | No | 1.500 |
| Small Mana Potion (14:4) | +0 | 1 | — | No | 80 |
| Medium Mana Potion (14:5) | +0 | 1 | — | No | 330 |
| Large Mana Potion (14:6) | +0 | 1 | — | No | 1.500 |
| Antidote (14:8) | +0 | 1 | — | No | 80 |
| Apple (14:0) | +0 | 50 | — | No | 1.000 |
| Small Healing Potion (14:1) | +0 | 50 | — | No | 4.000 |
| Medium Healing Potion (14:2) | +0 | 50 | — | No | 16.500 |
| Large Healing Potion (14:3) | +0 | 50 | — | No | 75.000 |
| Small Mana Potion (14:4) | +0 | 50 | — | No | 4.000 |
| Medium Mana Potion (14:5) | +0 | 50 | — | No | 16.500 |
| Large Mana Potion (14:6) | +0 | 50 | — | No | 75.000 |
| Antidote (14:8) | +0 | 50 | — | No | 4.000 |
| Apple (14:0) | +1 | 1 | — | No | 40 |
| Small Healing Potion (14:1) | +1 | 1 | — | No | 160 |
| Medium Healing Potion (14:2) | +1 | 1 | — | No | 660 |
| Large Healing Potion (14:3) | +1 | 1 | — | No | 3.000 |
| Apple (14:0) | +1 | 50 | — | No | 2.000 |
| Small Healing Potion (14:1) | +1 | 50 | — | No | 8.000 |
| Medium Healing Potion (14:2) | +1 | 50 | — | No | 33.000 |
| Large Healing Potion (14:3) | +1 | 50 | — | No | 150.000 |
| Legendary Helm (7:3) | +3 | 42 | Opción regular +4; Luck | No | 86.300 |
| Legendary Armor (8:3) | +3 | 42 | Opción regular +4; Luck | No | 111.100 |
| Legendary Pants (9:3) | +3 | 42 | Opción regular +4; Luck | No | 98.200 |
| Legendary Gloves (10:3) | +3 | 42 | Opción regular +4; Luck | No | 65.500 |
| Legendary Boots (11:3) | +3 | 42 | Opción regular +4; Luck | No | 72.000 |
| Armor of Guardsman (13:29) | +0 | 1 | — | No | 5.000 |
| Town Portal Scroll (14:10) | +0 | 1 | — | No | 750 |
| Bolt (4:7) | +0 | 255 | — | No | 100 |
| Bolt (4:7) | +1 | 255 | — | No | 1.400 |
| Bolt (4:7) | +2 | 255 | — | No | 2.200 |
| Arrows (4:15) | +0 | 255 | — | No | 70 |
| Arrows (4:15) | +1 | 255 | — | No | 1.200 |
| Arrows (4:15) | +2 | 255 | — | No | 2.000 |
| Gorgon Staff (5:4) | +3 | 65 | Opción regular +4; Luck | No | 94.100 |
| Legendary Staff (5:5) | +3 | 66 | Opción regular +4; Luck | No | 100.000 |
| Legendary Shield (6:14) | +3 | 50 | Opción regular +10 (Defense Rate); Luck | No | 94.700 |
| Scroll of Flame (15:4) | +0 | 1 | — | No | 21.000 |
| Scroll of Twister (15:7) | +0 | 1 | — | No | 25.000 |

### Eo the Craftsman — 243 Eo

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:351-385.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Spirit Helm (7:13) | +3 | 38 | Opción regular +4; Luck | No | 53.600 |
| Spirit Armor (8:13) | +3 | 38 | Opción regular +4; Luck | No | 65.500 |
| Spirit Pants (9:13) | +3 | 38 | Opción regular +4; Luck | No | 59.300 |
| Spirit Gloves (10:13) | +3 | 38 | Opción regular +4; Luck | No | 48.200 |
| Spirit Boots (11:13) | +3 | 38 | Opción regular +4; Luck | No | 53.600 |
| Guardian Helm (7:14) | +3 | 45 | Opción regular +4; Luck | No | 98.200 |
| Guardian Armor (8:14) | +3 | 45 | Opción regular +4; Luck | No | 115.600 |
| Guardian Pants (9:14) | +3 | 45 | Opción regular +4; Luck | No | 102.400 |
| Guardian Gloves (10:14) | +3 | 45 | Opción regular +4; Luck | No | 86.300 |
| Guardian Boots (11:14) | +3 | 45 | Opción regular +4; Luck | No | 94.100 |
| Crossbow (4:8) | +1 | 22 | Opción regular +4; Skill; Luck | No | 1.900 |
| Golden Crossbow (4:9) | +3 | 26 | Opción regular +4; Skill; Luck | No | 17.300 |
| Short Bow (4:0) | +0 | 20 | Opción regular +4; Skill; Luck | No | 600 |
| Bow (4:1) | +0 | 24 | Opción regular +4; Skill; Luck | No | 2.400 |
| Elven Bow (4:2) | +2 | 28 | Opción regular +4; Skill; Luck | No | 19.200 |
| Battle Bow (4:3) | +3 | 36 | Opción regular +4; Skill; Luck | No | 57.900 |
| Light Crossbow (4:11) | +3 | 40 | Opción regular +4; Skill; Luck | No | 85.600 |
| Tiger Bow (4:4) | +3 | 43 | Opción regular +4; Skill; Luck | No | 134.000 |
| Arquebus (4:10) | +3 | 31 | Opción regular +4; Skill; Luck | No | 36.700 |
| Elven Shield (6:3) | +3 | 36 | Opción regular +10 (Defense Rate); Luck | No | 19.100 |

### Zienna — 246 Zienna

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:388-418.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Dragon Helm (7:1) | +3 | 68 | Opción regular +4; Luck | No | 115.600 |
| Dragon Armor (8:1) | +3 | 68 | Opción regular +4; Luck | No | 125.000 |
| Dragon Pants (9:1) | +3 | 68 | Opción regular +4; Luck | No | 106.600 |
| Dragon Gloves (10:1) | +3 | 68 | Opción regular +4; Luck | No | 94.100 |
| Dragon Boots (11:1) | +3 | 68 | Opción regular +4; Luck | No | 102.400 |
| Sword of Salamander (0:9) | +3 | 40 | Opción regular +4; Skill; Luck | No | 85.600 |
| Legendary Sword (0:11) | +3 | 54 | Opción regular +4; Skill; Luck | No | 163.700 |
| Giant Sword (0:15) | +3 | 60 | Opción regular +4; Skill; Luck | No | 235.300 |
| Double Blade (0:13) | +3 | 43 | Opción regular +4; Skill; Luck | No | 157.900 |
| Lighting Sword (0:14) | +3 | 50 | Opción regular +4; Skill; Luck | No | 250.000 |
| Heliacal Sword (0:12) | +3 | 66 | Opción regular +4; Skill; Luck | No | 277.700 |
| Serpent Crossbow (4:12) | +3 | 45 | Opción regular +4; Skill; Luck | No | 197.400 |
| Great Scythe (3:8) | +3 | 68 | Opción regular +4; Skill; Luck | No | 256.000 |
| Bill of Balrog (3:9) | +3 | 74 | Opción regular +4; Skill; Luck | No | 363.300 |
| Silver Bow (4:5) | +3 | 48 | Opción regular +4; Skill; Luck | No | 277.700 |
| Bluewing Crossbow (4:13) | +3 | 56 | Opción regular +4; Skill; Luck | No | 434.000 |

### Lumen the Barmaid — 255 Lumen

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:421-476.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Blood Bone (13:17) | +1 | 1 | — | No | 10.000 |
| Blood Bone (13:17) | +2 | 1 | — | No | 50.000 |
| Blood Bone (13:17) | +3 | 1 | — | No | 100.000 |
| Blood Bone (13:17) | +4 | 1 | — | No | 300.000 |
| Blood Bone (13:17) | +5 | 1 | — | No | 500.000 |
| Blood Bone (13:17) | +6 | 1 | — | No | 800.000 |
| Blood Bone (13:17) | +7 | 1 | — | No | 1.000.000 |
| Blood Bone (13:17) | +8 | 1 | — | No | 1.200.000 |
| Scroll of Archangel (13:16) | +1 | 1 | — | No | 10.000 |
| Scroll of Archangel (13:16) | +2 | 1 | — | No | 50.000 |
| Scroll of Archangel (13:16) | +3 | 1 | — | No | 100.000 |
| Scroll of Archangel (13:16) | +4 | 1 | — | No | 300.000 |
| Scroll of Archangel (13:16) | +5 | 1 | — | No | 500.000 |
| Scroll of Archangel (13:16) | +6 | 1 | — | No | 800.000 |
| Scroll of Archangel (13:16) | +7 | 1 | — | No | 1.000.000 |
| Scroll of Archangel (13:16) | +8 | 1 | — | No | 1.200.000 |
| Devil's Eye (14:17) | +1 | 1 | — | No | 10.000 |
| Devil's Eye (14:17) | +2 | 1 | — | No | 50.000 |
| Devil's Eye (14:17) | +3 | 1 | — | No | 100.000 |
| Devil's Eye (14:17) | +4 | 1 | — | No | 300.000 |
| Devil's Eye (14:17) | +5 | 1 | — | No | 500.000 |
| Devil's Eye (14:17) | +6 | 1 | — | No | 800.000 |
| Devil's Eye (14:17) | +7 | 1 | — | No | 1.000.000 |
| Town Portal Scroll (14:10) | +0 | 1 | — | No | 750 |
| Devil's Key (14:18) | +1 | 1 | — | No | 15.000 |
| Devil's Key (14:18) | +2 | 1 | — | No | 75.000 |
| Devil's Key (14:18) | +3 | 1 | — | No | 150.000 |
| Devil's Key (14:18) | +4 | 1 | — | No | 450.000 |
| Devil's Key (14:18) | +5 | 1 | — | No | 750.000 |
| Devil's Key (14:18) | +6 | 1 | — | No | 1.200.000 |
| Devil's Key (14:18) | +7 | 1 | — | No | 1.500.000 |
| Illusion Sorcerer Covenant (13:50) | +1 | 1 | — | No | 500.000 |
| Illusion Sorcerer Covenant (13:50) | +2 | 1 | — | No | 600.000 |
| Illusion Sorcerer Covenant (13:50) | +3 | 1 | — | No | 800.000 |
| Illusion Sorcerer Covenant (13:50) | +4 | 1 | — | No | 1.000.000 |
| Illusion Sorcerer Covenant (13:50) | +5 | 1 | — | No | 1.200.000 |
| Old Scroll (13:49) | +1 | 1 | — | No | 500.000 |
| Old Scroll (13:49) | +2 | 1 | — | No | 600.000 |
| Old Scroll (13:49) | +3 | 1 | — | No | 800.000 |
| Old Scroll (13:49) | +4 | 1 | — | No | 1.000.000 |
| Old Scroll (13:49) | +5 | 1 | — | No | 1.200.000 |
| Ale (14:9) | +0 | 1 | — | No | 750 |
| Armor of Guardsman (13:29) | +0 | 1 | — | No | 5.000 |

### Caren the Barmaid — 244 Caren

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:479-499.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Ale (14:9) | +0 | 1 | — | No | 750 |
| Town Portal Scroll (14:10) | +0 | 1 | — | No | 750 |
| Scroll of Archangel (13:16) | +1 | 1 | — | No | 10.000 |
| Scroll of Archangel (13:16) | +2 | 1 | — | No | 50.000 |
| Blood Bone (13:17) | +1 | 1 | — | No | 10.000 |
| Blood Bone (13:17) | +2 | 1 | — | No | 50.000 |
| Devil's Eye (14:17) | +1 | 1 | — | No | 10.000 |
| Devil's Eye (14:17) | +2 | 1 | — | No | 50.000 |
| Devil's Key (14:18) | +1 | 1 | — | No | 15.000 |
| Devil's Key (14:18) | +2 | 1 | — | No | 75.000 |
| Armor of Guardsman (13:29) | +0 | 1 | — | No | 5.000 |

### Alex — 230 Alex

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:505-523.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Double Poleaxe (3:5) | +3 | 38 | Opción regular +4; Luck | No | 7.700 |
| Spear (3:1) | +3 | 42 | Opción regular +4; Luck | No | 18.600 |
| Giant Trident (3:3) | +3 | 44 | Opción regular +4; Luck | No | 28.300 |
| Berdysh (3:7) | +3 | 54 | Opción regular +4; Skill; Luck | No | 114.200 |
| Serpent Spear (3:4) | +3 | 58 | Opción regular +4; Skill; Luck | No | 180.100 |
| Light Spear (3:0) | +3 | 56 | Opción regular +4; Skill; Luck | No | 148.400 |
| Light Saber (0:10) | +3 | 50 | Opción regular +4; Skill; Luck | No | 134.000 |
| Great Hammer (2:3) | +3 | 50 | Opción regular +4; Skill; Luck | No | 120.600 |

### Marce — 417 Marce

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:525-539.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Scroll of Fire Ball (15:3) | +0 | 1 | — | No | 300 |
| Scroll of Power Wave (15:10) | +0 | 1 | — | No | 1.100 |
| Scroll of Meteorite (15:1) | +0 | 1 | — | No | 11.000 |
| Scroll of Ice (15:6) | +0 | 1 | — | No | 14.000 |
| Drain Life Parchment (15:20) | +0 | 1 | — | No | 100.000 |

### Rhea — 416 Rhea

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:541-574.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Mistery Helm (7:39) | +2 | 36 | Opción regular +4; Luck | No | 21.500 |
| Mistery Armor (8:39) | +2 | 36 | Opción regular +4; Luck | No | 32.200 |
| Mistery Pants (9:39) | +2 | 36 | Opción regular +4; Luck | No | 24.800 |
| Mistery Gloves (10:39) | +2 | 36 | Opción regular +4; Luck | No | 15.900 |
| Mistery Boots (11:39) | +2 | 36 | Opción regular +4; Luck | No | 18.600 |
| Red Wing Helm (7:40) | +3 | 42 | Opción regular +4; Luck | No | 86.300 |
| Red Wing Armor (8:40) | +3 | 42 | Opción regular +4; Luck | No | 111.100 |
| Red Wing Pants (9:40) | +3 | 42 | Opción regular +4; Luck | No | 98.200 |
| Red Wing Gloves (10:40) | +3 | 42 | Opción regular +4; Luck | No | 65.500 |
| Red Wing Boots (11:40) | +3 | 42 | Opción regular +4; Luck | No | 72.000 |
| Small Axe (1:0) | +1 | 18 | Opción regular +4; Luck | No | 290 |
| Short Sword (0:1) | +0 | 22 | Opción regular +4; Luck | No | 230 |
| Hand Axe (1:1) | +1 | 20 | Opción regular +4; Luck | No | 610 |
| Rapier (0:2) | +2 | 23 | Opción regular +4; Luck | No | 2.600 |
| Elven Axe (1:4) | +1 | 32 | Opción regular +4; Luck | No | 11.700 |
| Kris (0:0) | +1 | 20 | Opción regular +4; Luck | No | 950 |
| Skull Staff (5:0) | +1 | 20 | Opción regular +4; Luck | No | 950 |
| Mistery Stick (5:14) | +1 | 50 | Opción regular +4; Luck | No | 13.800 |
| Violent Wind Stick (5:15) | +2 | 60 | Opción regular +4; Luck | No | 40.700 |
| Red Wing Stick (5:16) | +3 | 65 | Opción regular +4; Luck | No | 100.000 |

### Weapons Merchant Bolo — 578 Bolo

Fuente de la lista: VersionSeasonSix/MerchantStores.cs:576-591.

| Ítem (g:n) | Nivel | Durabilidad / lote | Opciones | Excelente | Precio (Zen) |
|---|---:|---:|---|---|---:|
| Sword of Destruction (0:16) | +3 | 84 | Opción regular +12; Luck | No | 515.600 |
| Battle Scepter (2:8) | +3 | 40 | Opción regular +12; Skill; Luck | No | 194.500 |
| Master Scepter (2:9) | +3 | 45 | Opción regular +12; Skill; Luck | No | 377.400 |
| Aquagold Crossbow (4:14) | +3 | 60 | Opción regular +12; Skill; Luck | No | 1.179.500 |
| Saint Crossbow (4:16) | +3 | 72 | Opción regular +12; Skill; Luck | No | 1.708.600 |
| Staff of Resurrection (5:6) | +3 | 70 | Opción regular +12; Luck | No | 353.100 |

## Cumplimiento del modelo MU Malvinas

| Regla / expectativa | Hallazgo en el seed actual | Estado |
|---|---|---|
| Jewels no vendibles en NPC | No hay llamadas de los stores a una definición de jewel; las entradas grupo 12 detectadas son Orbs (por ejemplo Orb of Healing, Orb of Impale), no Jewels. GameConfigurationInitializer.cs:105 inicializa definiciones de jewels globales, pero no las agrega a MerchantStore. | **Cumple** (0 filas) |
| Items Excellent solo por cajas/eventos | Las llamadas de equipo pasan targetExcellentOption = null; ItemHelper.cs:222-227 devuelve null y ItemHelper.cs:175-181 solo adjunta Excellent si existe. El runtime del seed dio 0 opciones Excellent. | **Cumple** (0 filas) |
| Equipo/set básico sin +3/+4 o superior | Las plantillas activas venden equipo +3 y con opción regular +4/Luck: Alex, Wandering, Hanzo, Pasi, Elf Lala, Izabel, Eo y Zienna. El equipo configurado aunque hoy inactivo suma Rhea y Bolo; Bolo vende +3 +12 +Luck (+Skill en varias armas). | **No cumple** |
| Sets fuera de lugar / tiers altos | Hay Plate +3 en Wandering; Vine/Silk/Wind en Elf Lala; Legendary +3 en Izabel; Spirit/Guardian +3 en Eo; Dragon +3 en Zienna; Pad/Bone/Sphinx en Pasi; y Mistery/Red Wing en Rhea (mapa excluido). En todos esos CreateSetItem el seed agrega opción regular y Luck. | **No cumple** |
| Solo básicos, pociones y entradas/tickets | Pociones, scrolls de evento, keys/eyes/bones/archangel y otros consumibles sí están presentes. Lumen vende tickets de Blood Castle/Devil Square/Illusion Temple/Chaos Castle en niveles +1…+8 o +1…+5; por ser entradas de eventos no se marcan como Excellent ni como set, pero conviene decidir si se desean todas las variantes. Pasi/Marce también venden scrolls de habilidades y Elf Lala orbs de soporte/summon, que exceden un catálogo mínimo de solo básicos. | **Revisión de alcance** |
| Tarkan / último tier | Version095d/Maps/Tarkan.cs:34-... implementa CreateMonsterSpawns y no CreateNpcSpawns; no hay NPC vendor allí. | **No existe hoy** |

### Filas que requieren atención prioritaria

- Activas y directamente visibles: todas las armas de Alex son +3 +4 Luck; Hanzo vende escudos hasta +3 +4 (varios Skill) y armas hasta +3 +4; Pasi vende Sphinx +3 +4 Luck y staffs +3 +4 Luck; Elf Lala vende Wind +3 +4 Luck y partes Vine +3; Izabel vende Legendary +3 +4 Luck y staffs +3; Eo vende Spirit/Guardian +3 +4 Luck, arcos +3 y Elven Shield +3; Zienna vende Dragon +3 +4 Luck y armas +3 +4 Luck; Wandering vende Scale/Brass/Plate +3 +4 Luck.
- Configuradas pero en mapas excluidos: Rhea vende Red Wing +3 +4 Luck y armas hasta +3; Bolo vende seis armas +3 +12 Luck, varias Skill. Si se reactivan Karutan/Elvenland, estas tiendas pasarían a ser visibles.
- No son incumplimientos del criterio de jewels/excellent: los objetos de entrada de eventos de Lumen/Caren están implementados como ítems nivelados sin opciones Excellent. Sus niveles representan la clase/etapa del ticket, no un set de armadura; se separan de la regla de sets básicos.

## NPCs pedidos explícitamente que NO tienen shop

| NPC | Definición exacta | Qué hace / evidencia |
|---|---|---|
| Marlon (229) | VersionSeasonSix/NpcInitialization.cs:52-60 | NpcWindow.LegacyQuest; no asigna MerchantStore. Spawn visible en Lorencia: VersionSeasonSix/Maps/Lorencia.cs:41. |
| Elf Soldier (257) | VersionSeasonSix/NpcInitialization.cs:113-121 | NpcWindow.NpcDialog; no asigna MerchantStore. Su bloque siguiente agrega el buff, no una tienda (:123-167). |
| Charon (237) | Version095d/NpcInitialization.cs:55-63 | NpcWindow.DevilSquare; no asigna MerchantStore. Es el NPC/ventana de entrada del evento, no vendor. |
| GameMaster (378) | VersionSeasonSix/NpcInitialization.cs:212-219 | Solo crea la definición NPC; no NpcWindow merchant ni MerchantStore. No hay GM shop estático en este seed. |
| Tarkan | Version095d/Maps/Tarkan.cs:27-35 | No override de CreateNpcSpawns; el método presente es CreateMonsterSpawns, sin vendors. |

## Ubicaciones de inicialización y persistencia

- VersionSeasonOne/DataInitialization.cs:40-52 define la clave season1-classic; VersionSeasonOne/GameConfigurationInitializer.cs:68-115 inicializa ítems/NPCs y llama new NpcInitialization(...).Initialize() en la línea 113. Por el using de VersionSeasonSix (línea 13), esa clase es la implementación Season Six que hereda la cadena VersionSeasonSix/NpcInitialization.cs:18, Version095d/NpcInitialization.cs y Version075/NpcInitialization.cs.
- Las asignaciones de los NPC base están en Version075/NpcInitialization.cs; las agregadas por Season Six, incluidas Alex, Oracle, Rhea, Marce, Leina, Bolo y Christine, están en VersionSeasonSix/NpcInitialization.cs. La lista efectiva de ítems está en los 14 métodos de VersionSeasonSix/MerchantStores.cs referenciados por las tablas.
- Version075/MerchantStores.cs:442-451 solo materializa el ItemStorage a partir de la lista; los overrides Season Six son los que fijan el contenido efectivo.
- MonsterDefinition.MerchantStore es la propiedad persistida en DataModel/Configuration/MonsterDefinition.cs:315; GameConfigurationHelper.cs:96 incluye esos stores al recorrer la configuración.
- DataInitializationBase.cs:202-205 filtra actualizaciones por DataInitializationKey. Los updates AddMissingMerchantStoresPlugIn.cs:35-51, AddKalimaPlugIn.cs:14-35 y AddLorenMarketJuliaWarpPlugIn.cs:34-42 están keyed a VersionSeasonSix.DataInitialization.Id (season6), no a season1-classic, por lo que no se aplican al seed Season 1 fresco; aun así el initializer directo ya asigna las tiendas de Christine y Oracle.
