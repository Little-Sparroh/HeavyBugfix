# HeavyBugfix

Fixes a vanilla Mycopunk bug where the **Heavy** upgrade's fire-rate roll range
changes depending on equip/stack context, confusing players comparing copies.

## The bug

Heavy's fire-interval property uses `OverrideThenAdd`. When building the tooltip
stat list, the game converts that to either **Override** or **Add** based on how
many Heavies are already equipped (`GetAppliedCount`), then runs it through
`StatData.CreateInverse` against the weapon's fire interval.

On a weapon with `fireInterval = 0.3` and Heavy's range `0.38–0.45`, that yields:

| Context                            | Displayed range       |
|------------------------------------|-----------------------|
| No Heavy equipped yet / first copy | `[-33.33% - -21.05%]` |
| Another Heavy already equipped     | `[-60% - -55.88%]`    |

The rolled value is still seed-based; only the **bracket text** was lying.

## The fix

This mod patches `UpgradeProperty_FireInterval.GetStatData` so tooltips always:

1. Use first-copy (**Override**) display semantics for `OverrideThenAdd`
2. Use the gear **prefab** baseline fire interval (not live modified stats)

Combat application (`Apply`) is unchanged — stacking still works as designed.

## Dependencies

- Mycopunk
- [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.2403+ (Mycopunk pack)

## Install

**Thunderstore / r2modman (recommended):** install normally.

**Manual:** place `HeavyBugfix.dll` in `BepInEx/plugins/`.

## Build

```bash
dotnet build --configuration Release
```

## Authors

- Sparroh

## License

MIT — see LICENSE.
