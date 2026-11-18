# CodeScan

CodeScan checks a Unitystation build for code that isn't allowed to run. It reads the compiled assemblies (without running them) and flags any type, method, or field that isn't on the whitelist.

It's based on how SS14 scans content, adapted for Unitystation by Bod.

## Running it

```sh
CodeScan ">/path/to/unitystation/build"
```

Keep the quotes. Without them your shell treats `>` as a redirect.

It scans the build's `Managed` folder for the OS you're running on, so scan a Windows build on Windows, and so on. The result goes to `errors.json` next to the executable. An empty list means the build passed. The exit code is always 0, so don't rely on it.

If anything got rejected, it also writes `CodeScanList.corrected.json`: the whitelist with the missing entries added. Pass `-supressCorrectedWhitelist` to skip that.

`FilesToMoveToManaged.json` sits one folder above the executable. It lists dependencies like Mirror or Firebase that shouldn't be scanned. CodeScan copies them to `MissingAssemblies/<OS>/`, and also looks there for anything the build doesn't ship.

## The whitelist

The whitelist isn't in this repo. CodeScan downloads it on every run from the Unitystation repo:

https://raw.githubusercontent.com/unitystation/unitystation/develop/CodeScanList.json

To allow something, edit it there. The easiest way is to diff it against `CodeScanList.corrected.json` and copy over only what should actually be allowed. Only do this if you know what you are doing.

Some things to know when editing:

- `"All": true` allows every member of a type. Without it, every method and field has to be listed by exact signature, overloads included.
- Namespace entries are prefix matches, so `"System"` also allows `System.IO`.
- Signatures use metadata names (``List`1``, `!0`, `Outer/Inner`, `ref int`). Copy the style of the existing entries.

## Building

It's a .NET 10 Native AOT app, so you get a single native executable and don't need .NET installed to run it.

The catch: Native AOT can't build for another OS. Locally you only get a binary for the machine you're on:

```sh
dotnet publish -c Release -r linux-x64   # or win-x64 / osx-x64 on those systems
```

You'll need the native toolchain too: clang and zlib on Linux, the VS C++ tools on Windows, the Xcode CLI tools on macOS.

To release, tag a commit on `main` and push the tag. CI builds each platform on its own OS and creates a draft release with the zips. Check it and publish it on GitHub.

`dotnet run -- ">/path/to/build"` is fine for quick testing. Copy `FilesToMoveToManaged.json` into `bin/Debug/` first. It runs regular .NET rather than AOT, so check a published build before releasing.

Publishing shows a Pidgin `IL3053`/`IL3054` warning. It's expected and harmless.
