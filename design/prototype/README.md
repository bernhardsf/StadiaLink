# StadiaLink visual prototype

Open [preview.html](preview.html) in a browser to explore the five app screens. Use the width controls or drag the preview’s bottom-right corner to inspect responsive layouts. Settings includes accent-color presets, remembered in browser storage.

- [Brand concepts](brand.html): app icon, wordmark, and controller illustration.
- [Design notes](design-notes.html): sources, control behavior, and implementation considerations.
- `assets/`: local SVG assets and the Fluent System Icons MIT license.

This is a standalone visual prototype. It sends no hardware commands and is not part of the WPF application build. Window controls simulate browser-preview behavior; dropdowns are styled HTML controls, not native WinUI controls. Segoe UI is used when installed, with Arial as a fallback.

All runtime assets are local. No package installation or build step is required.

## v3 feature coverage

The preview now includes all 19 input assignments, Windows actions and shortcut dialogs, raw trigger meters, configured input visualization, correct control ranges, shared trigger controls, working profile CRUD/import/export, diagnostics affordances and connection/error states. Expand **Demo controls** below the app to exercise simulated hardware input. See [COVERAGE.md](COVERAGE.md) for the audit, deliberate UX changes and verification limits.

Implementation is split into `artwork.js`, `model.js`, `prototype.js`, `window.js`, and `prototype.css`. Saved demo profiles and the accent persist only in this browser. The Windows app now implements the redesign; this browser demo remains independent of hardware.

Product identity: the interlocking S uses fixed teal/blue brand colors. Full-detail SVG is used at 48 px and above; separate simplified 16/24/32 px assets are available. The title bar uses the 16 px version. `assets/brand-review.png` is a rendered review image of the vector identity.


Installer refresh: `installer.html` previews install, update, and uninstall states. Production WPF setup uses the same approved mark from `app/assets/logo.xaml`, a separate neutral/blue installer palette, native resizable Windows chrome, and a scrollable body. `app/create-icon.ps1` renders transparent 16/24/32/48/64/256 px ICO frames, with simplified geometry below 48 px. Current build outputs are in `release/`. See the [main README](../../README.md) for installation and driver security guidance.
