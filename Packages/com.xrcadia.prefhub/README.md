# xrcadia.prefhub

The home for your PlayerPrefs. PrefHub is a single editor window that lists every saved preference in your project and lets you view, edit, add, and delete them — without writing throwaway code, digging through the registry, or guessing which keys exist.

Unity's `PlayerPrefs` API can read and write a key but can't tell you which keys are there. PrefHub closes that gap by reading the underlying platform store, so the full set of keys is always in front of you.

## Features

- **Every key in one place:** Discovers all PlayerPref keys for the project — Unity gives you no enumeration API, PrefHub reads the platform store so nothing stays hidden.
- **Inline editing:** Change a value or its type (Int / Float / String) directly in the list; writes go through the standard `PlayerPrefs` API and save immediately.
- **Add & delete:** Create a new key from the bottom bar, remove a single key, or clear everything with a guarded **Delete All**.
- **Live search:** Filter the list as you type to find a key fast.
- **Cross-platform discovery:** Reads the macOS preference domain (`defaults`) and the Windows registry, with a graceful fallback elsewhere.

## Installation

Add this package to your Unity project via the Package Manager using the git URL, or by adding it to `manifest.json`:

```json
{
  "dependencies": {
    "com.xrcadia.prefhub": "https://github.com/xrcadia/xrcadia.prefhub.git"
  }
}
```

## Usage

Open **xrcadia ▸ PrefHub ▸ Manager** from the Unity menu bar. The window lists the current keys; edit values inline, use the bottom bar to add or update a key, and **Refresh** to re-read the store after external changes.

## Notes

- On **Windows**, Unity stores ints and floats with overlapping registry types, so a numeric key's type can't always be distinguished — switch the type in the dropdown if a value reads back as the wrong kind.
- Key discovery is supported on macOS and Windows editors. On other platforms the window still edits keys you add, but can't enumerate pre-existing ones.
