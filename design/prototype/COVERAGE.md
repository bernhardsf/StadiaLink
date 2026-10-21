# Redesign coverage audit

Reviewed 2026-10-20 against the connected app in Chrome Remote Desktop and `app/MainWindow.cs`, `app/Model.cs`, and `app/BatteryTray.cs`.

## Observed in the running app

- Controller diagram, physical-input selector, function/output selectors, connection label, raw L2/R2 meters, device selector and battery.
- Sticks and triggers: two stick scopes, deadzones, sensitivity, inversion, input/output trigger bars, shared trigger deadzone and shared button threshold. Lower swap controls also checked against source.
- Vibration: strong/light gains, calculated test output, test signal, individual/both/continuous/stop actions and status.
- Profiles: Standard and Strong Vibration, new/import, editable name, duplicate/export/delete.
- Settings: startup, notifications, language, all three shortcut entry points. Opened Assistant shortcut dialog and inspected key, modifiers, hold behavior and Update draft. Navigated through lower Settings using keyboard focus and visually confirmed Bluetooth settings, Scan again, driver installation help, six-second response timing and Open report, local-build update status, and Open profiles folder.

Remote Desktop interaction was intermittent, but keyboard navigation allowed the remaining Settings sections to be inspected. Their controls were cross-checked against source. Native tray behavior remains source-reviewed only; diagnostic measurements, external folder/settings launches and hardware failure states were not exercised live. No settings were saved and no motor tests were invoked on the Windows app.

## Included in v3

| Area | Preview coverage |
| --- | --- |
| Controller | All 19 diagram targets and selector options, every Windows action, supported keys/modifiers, gamepad outputs/unmapped, raw L2/R2 meters |
| Sticks | Separate deadzones 0–40%, no-deadzone reset, sensitivity 50–200%, X/Y inversion, raw/output scopes, swap sticks |
| Triggers | Shared deadzone 0–40%, threshold 1–100%, raw marker, configured fill, threshold marker, swap triggers |
| Vibration | Independent gains 0–200%, immediate gain persistence, test output, signal, individual/both/continuous/stop, error states |
| Profiles | Create, duplicate, rename, select, save/apply simulation, JSON export/import with validation, delete confirmation and last-profile guard |
| Settings | Accent presets, startup/notifications, four language choices, three shortcut dialogs, connection/help/discovery, six-second diagnostic simulation/report, local updates status, profiles folder affordance |
| Shell | Responsive navigation and content, titlebar control simulations, device selection, battery/charging/unavailable/disconnected states, tray preview |

## Deliberate redesign changes

- Preserve unsaved edits when navigating pages. Ask before replacing them by switching profiles. Current app can discard drafts silently.
- Label the header “Editing profile” because selecting a profile does not necessarily apply it.
- Equal-width motor cards and tighter settings rows replace oversized asymmetric panels.
- Accent selection is a proposed addition, not an existing Windows-app feature.
- Keep the controller recognizable with a white shell, symmetrical sticks and orange stick surrounds. All buttons are keyboard-selectable. This is a custom instructional SVG, not a Fluent system icon.

## Prototype limits

All device input and hardware states are simulated. Expand **Demo controls** below the window to vary input and connection states. Profile and accent data use browser-local storage; app preferences are session-only. Language choices are represented but the preview remains English. No real Windows settings, driver installation, motor activation, HID timing, Windows notifications, native tray or shell integration occurs.

The six-second diagnostic does not invent measured latency numbers. It explains the real report fields. Folder and Bluetooth actions explain their production destinations. This is HTML styled to reflect Fluent conventions, not a WinUI runtime. Native Snap, OS resize borders, DPI handling and high-contrast behavior need production verification.

## Verification

JavaScript syntax checked. DOM tests passed for five screens across seven simulated states, 19 diagram targets, inline handler syntax, mapping/save/profile switching, raw versus configured meters, shortcut editing, create/delete/import and schema boundary validation. Local asset references checked. Follow-up DOM checks passed for controller keyboard-focus retention, scroll reset only on page changes, synchronized device/demo selection, preserved maximize/restore bounds, and minimize/restore focus. Responsive CSS was reviewed for narrow toolbars, long labels and inspector stacking; this is not a rendered-layout verification.

Browser automation rejected local file URLs earlier in the session; no workaround was used. This revision has not received a rendered browser screenshot/layout pass. Review Wide, Medium, Compact and Narrow presets manually, including long profile names and keyboard navigation.

## Follow-up inspection

The Assistant dialog was dismissed and keyboard navigation exposed the remaining Settings sections. They match the functionality already included in the preview. Source review of BatteryTray.cs and DeviceNotifications.cs identified low battery (15% or less), full charge (100%) and unavailable readings. Demo controls now include an adjustable battery level, proportional icon fill, low-battery label and matching tray text. These are simulated states, not live readings or OS notifications.

Shortcut rows now summarize the current draft assignment, including key modifiers and hold/press-once behavior. Unconfigured shortcuts show their actual gamepad or unmapped assignment rather than implying a shortcut is already active.

The connection indicator now uses a larger status icon, a tinted panel, a left border and a second explanatory line. Connected, disconnected, driver warning, unavailable vibration and command-failure states have separate semantic treatments independent of the selected accent.

Keyboard follow-up: changing the mapping Function, Gamepad output or Key retains focus on that selector after the inspector rerenders. Trigger meters expose their label, range and current percentage to assistive technology, including the disconnected state. Targeted DOM assertions passed for selector focus and full/released/disconnected meter readings.

## 2026-10-21 follow-up

Reopened the connected Remote Desktop session and visually rechecked Settings, Controller, and Sticks and triggers. No profile was saved or motor test invoked. Source comparison of MainWindow.PerformanceReport revealed that the preview report lacked its detailed structure. Added three distribution rows (HID query duration, query intervals, latest query to UI frame), median/p95/p99/max/sample-count columns, render/HID rates and WPF tier. All values are explicitly unmeasured. The report explains the limit on interpreting these timings as end-to-end latency. Targeted DOM tests passed for empty-report and completed-demo report states. Native tray and actual diagnostic execution remain unverified live.

Tray follow-up: the remote session did not visibly focus the Windows notification area through the keyboard shortcut, so native tray verification remains incomplete. Based on BatteryTray.cs, the preview now shows a numeric battery badge with proportional charge strip, low-charge color, app-icon fallback for unknown/disconnected readings, charging/full status, and an Open/Exit context-menu mockup. Exit presents a Relaunch action rather than the normal Restore label. This remains a browser simulation; no Windows process is terminated. Targeted DOM tests cover low battery, unknown battery, exit and reopen.


## Installer packaging verification, 2026-10-21

Setup and Update executables compile on Windows. Install/update/removal client areas render in all four supported languages; the German removal view was also inspected at the minimum 560 × 600 window size. Hashes for both executables and the source archive match SHA256SUMS.txt. Packaged installer and icon sources match the repository after normalizing UTF-8 BOM and line endings. Both executables contain the logo resource and payload. The ICO contains 16, 24, 32, 48, 64 and 256 pixel frames. Local HTML navigation and asset references resolve.

The HTML installer preview retains the optional driver-removal selection across state changes and disables it during work and after completion. Actual Windows renders are available in the comparison disclosure. No install, update, removal, UAC flow or hardware operation was executed; those remain functional validation steps on a disposable Windows test environment. Browser rendered-layout verification remains outstanding as described above.
