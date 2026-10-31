# How Long Did I Sleep? (HLDIS)

Client + server Vintage Story mod. When a player joins, they receive a chat message with:

- the in-game time elapsed since their last visit:
  - under 1 day: "less than a day";
  - under 7 days: number of whole days ("3 days");
  - 7 days to under a month: weeks rounded to the nearest half week ("a week and a half");
  - one month to under a year: months rounded to the nearest half month ("2 and a half months");
  - one year or more: years and remaining whole months ("1 year and 3 months", "2 years");
- the part of the season at their position: early, mid or late spring / summer / autumn / winter, without the exact month.

The message is translated client-side (`assets/hldis/lang/en.json`, `fr.json`) according to the player's language.

## Versions

| Component | Version |
|---|---|
| Vintage Story (compile-time API) | 1.22.7 |
| Targeted Optimum / Stratum | 0.3.19 / v1.22.7-stratum.2 (untested) |
| Framework | net10.0 (same as `VintagestoryAPI.dll` 1.22.7) |
| SDK | .NET 10 (`global.json`: 10.0.100, `latestFeature`) |

## How it works

- **Server**: when the player enters the game (`PlayerNowPlaying`), compares `Calendar.TotalDays` with the date stored for that player, then sends a network packet (channel `hldis`). The date is updated on disconnect and on every world save (`GameWorldSave`) for connected players. It is stored in the world save (key `hldis-lastseen`).
- **Client**: picks the translated texts and shows them in chat.
- **Season**: computed from `GetSeasonRel` at the player's position (hemisphere and `SeasonOverride` included). Season boundaries match those of `GameCalendar.GetSeason` in game version 1.22.7; each season is split into thirds.
- A week is always 7 in-game days. Months follow the world's "days per month" setting (sent by the server), and a year is 12 months, as in the game calendar. With a month shorter than 7 days, the week step is skipped.

The mod is required on both sides (`requiredOnClient` and `requiredOnServer`).

## Limitations

- The game calendar stops while nobody is online, unless the server's `PassTimeWhenEmpty` setting is on (it is off by default). On a server where you play alone, and in singleplayer, the message will almost always say "less than a day"; time passes during your absence only while other players are online, or if `PassTimeWhenEmpty` is on.
- First visit to a world: only the season is shown.
- If the server crashes, the absence is counted from the last world save.

## Building

Requirements: .NET 10 SDK and a Vintage Story 1.22.7 installation (game DLLs are not included in this repository).

The build scripts compile the mod in Release and write a ready-to-install ZIP to `dist/hldis_1.0.0.zip`, containing `HLDIS.dll`, `HLDIS.pdb`, `modinfo.json`, `modicon.png` and `assets/`. Copy it into the `Mods` folder of both the client and the server.

Linux / macOS / Git Bash:

```bash
./build.sh /path/to/vintagestory
```

Windows PowerShell or PowerShell 7 (any OS):

```powershell
./build.ps1 -GamePath "C:\path\to\Vintagestory"
```

Without a path, both scripts use the `VINTAGE_STORY` environment variable. Add `--test` (`build.sh`) or `-Test` (`build.ps1`) to run the Atlas test suite first. On Windows, if PowerShell blocks the script, run it with `powershell -ExecutionPolicy Bypass -File build.ps1`.

The ZIP content is defined by the `PackageMod` target in `HLDIS/HLDIS.csproj`, so a plain `dotnet build HLDIS/HLDIS.csproj -c Release -p:VintageStoryPath=...` produces the same archive.

## Tests

`HLDIS.Tests` uses [Atlas](https://github.com/Pixnop/Atlas) (`Pixnop.Atlas.XUnit` 0.16.1): `dotnet test` boots a real headless Vintage Story server in the test process, stages the mod, joins test players and captures the packets the server sends them. No client and no manual server setup are needed.

- `WakeUpScenarios`: first join (season only), rejoin after 10 in-game days, rejoin with no time added.
- `PersistenceScenarios`: the last-seen date survives a real server restart (`RestartWorld`).
- `FormattingTests`: unit choice and rounding, season thirds (no server boot).

```bash
VINTAGE_STORY="C:/path/to/Vintagestory-1.22.7" dotnet test HLDIS.Tests/HLDIS.Tests.csproj
```

`VINTAGE_STORY` must point at the game folder (the one holding `VintagestoryLib.dll`), both to build and to run the tests. Atlas runs the server side only, so the client handler (translation and chat display) is not covered.

## Verification status

- Build against the 1.22.7 API: successful.
- Atlas tests on Vintage Story 1.22.7: 26/26 passing (server side, packets, restart persistence, formatting logic).
- Client display, Optimum and Stratum: **not tested**.

## License

MIT, see [LICENSE](LICENSE).
