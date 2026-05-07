# MyPause

MyPause is a Windows desktop app (WPF, .NET) that reminds users to take breaks with configurable rules, pause windows, snooze limits, and persistent runtime state.

## Highlights

- Fixed-time alerts (for example: 10:30, 16:00)
- Interval timer alerts (for example: every 60 minutes)
- Per-alert break duration, snooze settings, active days, and custom sound
- Work schedule window (including overnight shift handling)
- Persistent configuration and runtime state in JSON
- Tray icon support with minimize-to-tray behavior
- Auto startup integration via Windows registry

## Tech Stack

- Language: C#
- Framework: .NET 9 (WPF)
- UI: XAML + code-behind components
- Serialization: Newtonsoft.Json
- Scheduling: DispatcherTimer-based runtime state machine

## Project Structure

- MyPause/Models
Contains alert state machine, schedule types, and configuration contracts.

- MyPause/Services
Contains alert orchestration, storage, notifications, tray icon integration, and startup registration.

- MyPause/Views
Contains reusable UI components and modal windows (alert card, progress bar, editors, notification modal).

- MyPause/Helpers
Contains formatting and visual helper utilities.

## Core Architecture

- Alert runtime engine
Each alert is represented by `Alert`, which owns its runtime state and emits immutable `AlertSnapshot` updates.

- Alert orchestration
`AlertsManager` stores and coordinates all alerts, including cross-alert reset behavior when one alert enters pause state.

- UI composition
Main window renders one `AlertCard` per alert. Each card listens to snapshot updates and is responsible for its own pause notification behavior.

- Persistence
`StorageManager` saves and loads:
  - `config.json` for full settings
  - `runtime-state.json` for active/running alert IDs

- Work schedule
`WorkSchedule` centralizes business logic for day/time validation and overnight ranges.

## Build and Run

Prerequisites:

- Windows 10 or 11
- .NET 9 SDK (or compatible installed SDK)

Build:

```bash
cd MyPause
dotnet build
```

Run:

```bash
dotnet run
```

## Configuration Files

The app stores data under:

- `%AppData%/MyPause/config.json`
- `%AppData%/MyPause/runtime-state.json`

## Runtime Behavior Notes

- If one alert triggers a break, timer counters of other alerts are reset to avoid stacked breaks.
- Pause windows are modal and use an overlay to block background interaction.
- When snooze is exhausted, snooze action is hidden automatically.
- Active days use `DayOfWeek` numeric mapping: `0=Sunday ... 6=Saturday`.

## Status

- Refactored alert state machine and schedule logic
- Modularized work schedule and alert card UI
- Notification logic moved into alert cards
- Event subscription cleanup for dynamic card add/remove
- Documentation updated to English across Models, Services, Views, and Helpers

## License

**Non-Commercial with Attribution Required**

MyPause is released under a Non-Commercial License. You are free to use, modify, and distribute the software for **non-commercial purposes only**, provided you:

1. **Include attribution** to Andrea Forlini as the original author
2. **Include a copy of the LICENSE file** in any distribution or derivative work
3. **Maintain the same license terms** for any derivative works

Commercial use is **strictly prohibited** without explicit permission from the author.

For the full license terms, see [LICENSE](LICENSE).

### Quick Summary

✅ **Allowed:**
- Personal use
- Educational projects
- Research
- Non-profit use
- Open-source contributions
- Modifications for personal use

❌ **Not Allowed:**
- Selling the software
- Commercial products/services
- Generating revenue from the software
- Removing attribution

For inquiries about commercial licensing, contact the author.
