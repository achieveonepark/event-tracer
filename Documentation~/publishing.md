# Publishing to OpenUPM

[한국어](publishing.ko.md) · [README](../README.md)

The package is prepared for version **1.0.0** under the **MIT** license. The package lives at the repository root and uses the ID `com.achive.event-tracer`. Keep the existing `achive` spelling in registry entries and dependencies.

## First release

1. Run the package's Unity EditMode tests, review the changes, and commit the release to `main`.
2. Push the commit and a matching version tag:

   ```sh
   git push origin main
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. Submit the repository through [OpenUPM's package submission form](https://openupm.com/packages/add/), using these values:

   | Field | Value |
   | --- | --- |
   | Package name | `com.achive.event-tracer` |
   | Display name | `Event Tracer` |
   | Repository | `https://github.com/achieveonepark/event-tracer` |
   | License | `MIT` |
   | Topic | `utilities` |
   | Minimum version | `1.0.0` |
   | README | `main:README.md` |
   | Hunter | `achieveonepark` |

4. Submit the generated metadata to the OpenUPM repository and wait for it to be merged and built. The metadata belongs in OpenUPM's `data/packages/com.achive.event-tracer.yml`, rather than this package repository. Check the package page and build results after registration.

OpenUPM requires the Git tag's version to match `package.json.version`. Updating this repository's manifest alone does not register or publish the package. See the [official submission guide](https://openupm.com/docs/adding-upm-package) for current requirements.

## Installation after registration

Once OpenUPM has built version 1.0.0, run this from a consuming Unity project:

```sh
openupm add com.achive.event-tracer@1.0.0
```

Alternatively, merge the following into the project's `Packages/manifest.json`, preserving existing registries and dependencies:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": ["com.achive.event-tracer"]
    }
  ],
  "dependencies": {
    "com.achive.event-tracer": "1.0.0"
  }
}
```

Until registration is complete, use the Git URL from the README. After pushing `v1.0.0`, append `#v1.0.0` to pin the Git dependency to this release.
