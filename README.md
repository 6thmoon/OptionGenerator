## Introduction

For more information please consult the **[Risk of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options)** documentation. However, suffice to say it provides a convenient API for adding mod options to the in-game settings menu. While simple to use, much of the content on this site does not support it.

This optional extension takes care of that by scanning and filling out the configuration for nearly every plugin, as well as any icon or description that may be missing. Since the majority are implemented in a standard way, this approach generally works, but there are some limitations described below. A separate configuration file is provided to disable individual sections if needed.

## Known Issues

- Depending on how a given configuration is implemented, changes might still require a restart or not take effect until the next stage/lobby.
- Certain configuration files will not be displayed (e.g. for game patcher, or not directly attached to main plugin).
- The following types are not supported: `Vector2`, `Vector3`, `Vector4`, `Quaternion`, and `Rect`.
- In some circumstances modifying a configuration value at runtime could break associated functionality - while this is not typically expected, you have been warned.

Please report any issues or significant incompatibilities [here](https://github.com/6thmoon/OptionGenerator/issues). Feel free to check out my other [work](https://thunderstore.io/package/6thmoon/?ordering=top-rated) as well.


## Version History

#### `0.2.0`
- Update for latest version `2.8.1` of **Risk of Options**.

#### `0.1.5`
- Fix edge case that could occur if another plugin did not load properly or is destroyed.

#### `0.1.4`
- Now properly generates its' own option menu.

#### `0.1.3`
- Any plugin may opt out by including a `BepInDependency` on Risk of Options.

#### `0.1.2` **- Initial Release**
