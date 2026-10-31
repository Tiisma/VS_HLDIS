# How Long Did I Sleep? (HLDIS)

Client + server Vintage Story mod. When a player joins, they receive a chat message with:

- the in-game time elapsed since their last visit:
  - under 1 day: "less than a day";
  - under 7 days: number of whole days ("3 days");
  - 7 days or more: weeks rounded to the nearest half week ("a week and a half", "3 weeks");
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
- A week is 7 in-game days, regardless of the world's "days per month" setting.

The mod is required on both sides (`requiredOnClient` and `requiredOnServer`).

## Limitations

- In singleplayer, in-game time does not pass while you are away: the message will almost always say "less than a day".
- First visit to a world: only the season is shown.
- If the server crashes, the absence is counted from the last world save.
- Long absences are not converted to months or years: the duration stays in weeks.

## Building

Requirements: .NET 10 SDK and a Vintage Story 1.22.7 installation (game DLLs are not included in this repository).

```bash
dotnet build HLDIS/HLDIS.csproj -c Release -p:VintageStoryPath="C:/path/to/Vintagestory-1.22.7"
```

Without `-p:VintageStoryPath`, the `VINTAGE_STORY` environment variable is used. The installable archive is written to `dist/hldis_1.0.0.zip`; copy it into the `Mods` folder of both the client and the server.

## Verification status

- Build against the 1.22.7 API: successful.
- In-game tests (client, dedicated server, Optimum, Stratum): **not done**.

## License

MIT, see [LICENSE](LICENSE).
