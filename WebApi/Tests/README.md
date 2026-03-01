# tests layout rules

This folder uses a centralized `Directory.Build.props` for test projects.

## Minimal test project template

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="..\..\Services\SomeService\SomeProject.csproj" />
  </ItemGroup>
</Project>
```

## Integration test exception

If a specific integration project needs `Microsoft.AspNetCore.Mvc.Testing`,
add it in that project only. Do not add it to `tests/Directory.Build.props`.

## xUnit runner config rule

Runner config source priority (highest to lowest):

1. `<AssemblyName>.xunit.runner.json` in test project root (project-specific override)
2. `xunit.runner.json` in test project root (project-specific override)
3. `tests/xunit.runner.json` (shared default)

xUnit does not merge these files. Assembly-specific config overrides generic config.

Do not duplicate runner-copy settings in individual test `.csproj` files.
Only add project-local runner config when behavior must differ from the shared default.
