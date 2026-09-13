# Auditoría de tiendas NPC para `season1-classic`

## Alcance

Se revisaron las 14 fábricas de tiendas de `VersionSeasonSix/MerchantStores.cs` contra el catálogo que realmente carga `VersionSeasonOne`: DK/DW/FE con segunda evolución, MG y DL; sin clases Master, Summoner ni Rage Fighter, y sin contenido posterior. Las fábricas heredadas de `Version075` se incluyeron porque siguen siendo las tiendas de Martin, Harold, Hanzo, Amy, Pasi, Lumen, Caren, Elf Lala, Eo, Izabel y Zienna.

## Cambios inequívocos aplicados

- `CreateLumenTheBarmaidStore`: retirados `Old Scroll` (grupo 13, número 49) e `Illusion Sorcerer Covenant` (grupo 13, número 50), materiales de Illusion Temple, que está fuera del roster classic.
- `CreateMarceStore`: retirado `Drain Life Parchment` (grupo 15, número 20), cuya definición es exclusiva de Summoner.
- `CreateRheaStore`: retirados los sets `Mistery`/`Violent Wind`/`Red Wing` (sets 39 y 40) y sus `Mistery`, `Violent Wind` y `Red Wing` Sticks; son equipo de Summoner y quedan sin clases calificadas en el catálogo season1.
- No se agregaron precios ni objetos económicos dudosos. Los cuatro pergaminos DW/SM clásicos (`Flame`, `Twister`, `Evil Spirit`, `Hellfire`) y los orbes DK/MG ya están presentes en Pasi y se conservaron.

Se añadió `MerchantCatalogSeasonOneTests`, que inicializa `season1-classic` y protege esas exclusiones, además de detectar equipo de tienda sin ninguna clase jugable.

## Tiendas revisadas

| Fábrica | NPC(s) | Resultado |
| --- | --- | --- |
| `CreatePotionGirlItemStorage` | Thompson 231, Oracle Layla 259, Pamela 376, Angela 377, Silvia 415, Leina 577, Christine 545 | Pociones, munición, Town Portal Scroll y Armor of Guardsman; compatibles. |
| `CreateWanderingMerchant` | Martin 248, Harold 250 | Leather–Plate para DK/DL/MG y objetos básicos; compatibles. |
| `CreateHanzoTheBlacksmith` | Hanzo 251 | Escudos y armas con al menos una clase jugable; compatible. |
| `CreatePasiTheMageStore` | Pasi 254 | Progresión clásica DW/SM y MG, más Orb of Impale/Twisting Slash para DK/MG; compatible. |
| `CreateElfLalaStore` | Elf Lala 242 | Pociones, equipo FE, munición y orbes de invocación para Elf. Se conservaron: son habilidades de Elf, no objetos de la clase Summoner. |
| `CreateIzabelTheWizardStore` | Izabel 245 | Equipo DW/SM/MG, armas auxiliares FE y pergaminos clásicos; compatible. |
| `CreateEoTheCraftsmanStore` | Eo 243 | Equipo y arcos de FE; compatible. |
| `CreateZiennaStore` | Zienna 246 | Equipo y armas DK/FE/MG; compatible. |
| `CreateLumenTheBarmaidStore` | Lumen 255 | Se conservaron Blood Castle, Devil Square, Chaos Castle, Town Portal y Ale; se retiró Illusion Temple. |
| `CreateCarenTheBarmaidStore` | Caren 244 | Ale, Town Portal, Blood Castle y Devil Square; compatible. |
| `CreateAlexStore` | Alex 230 | Armas DK/FE/MG/DL; compatible. |
| `CreateMarceStore` | Marce 417 | Cuatro pergaminos DW/SM/MG clásicos; se retiró Drain Life de Summoner. |
| `CreateRheaStore` | Rhea 416 | Se retiró equipo/armas Summoner; se conservaron armas genéricas con clases jugables. |
| `CreateBoloStore` | Bolo 578 | Las seis armas tienen al menos una clase jugable y no requieren Master/Summoner/RF; se dejaron sin alterar. |

## Casos explícitos para decisión del coordinador

1. Rhea pertenece a Elvenland y Bolo a Karutan 1, mapas que `season1-classic` no incluye. Este cambio audita sus catálogos, pero no elimina las definiciones de NPC ni las fábricas; decidir aparte si se desea retirar esos NPCs del inicializador.
2. Bolo vende armas de alto nivel con opciones +12. No se alteraron niveles, opciones ni precios: retirar o rediseñar esa economía requiere una política explícita.
3. Las tiendas conservan `Armor of Guardsman`, tickets de Blood Castle/Devil Square/Chaos Castle y los orbes de invocación de Elf porque esos objetos no son, por sí mismos, clases o mapas excluidos del roster elegido.
