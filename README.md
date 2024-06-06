## Introduction

For more information please consult the **[Risk of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options)** documentation. However, suffice to say it provides a convenient API for adding mod options to the in-game settings menu. While simple to use, much of the content on this site does not support it.

This optional extension takes care of that by scanning and filling out the configuration for nearly every plugin, as well as any icon or description that may be missing. Since the majority are implemented in a standard way, this approach tends to "just work", but there are some edge cases described below. A separate configuration file is also provided to disable individual sections if needed.

## Known Issues

- Depending on how a given configuration is implemented, changes might still require a restart or not take effect until the next stage/lobby.
- Certain configuration files will not be displayed (e.g. for game patcher, or not directly attached to main plugin).
- The following types are not supported: `Vector2`, `Vector3`, `Vector4`, `Quaternion`, and `Rect`.
- In extremely rare cases modifying a configuration value at runtime could break associated functionality - while this is not typically expected, you have been warned.

Please report any significant incompatibilities [here](https://github.com/6thmoon/OptionGenerator/issues). Feel free to check out my other [work](https://thunderstore.io/package/6thmoon/?ordering=top-rated) as well.


## Version History

#### `0.1.2` **- Initial Release**
