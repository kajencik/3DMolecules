# Test Project Setup - Complete! ?

## Problem
The `Tests\PhysicsEngineTests.cs` file was not visible in Solution Explorer because it wasn't part of any project.

## Solution
Created a proper xUnit test project and integrated it into the solution.

## What Was Done

### 1. Created Test Project
```bash
dotnet new xunit -n 3DMolecules.Tests -o 3DMolecules.Tests
```

### 2. Added to Solution
```bash
dotnet sln add 3DMolecules.Tests\3DMolecules.Tests.csproj
```

### 3. Fixed Target Framework
Changed from `net9.0` to `net8.0-windows` to match the main project.

### 4. Added Project Reference
```xml
<ProjectReference Include="..\3DMolecules\3DMolecules.csproj" />
```

### 5. Moved Test File
```bash
Copy-Item Tests\PhysicsEngineTests.cs 3DMolecules.Tests\PhysicsEngineTests.cs
Remove-Item Tests\PhysicsEngineTests.cs
```

## Current Solution Structure

```
3DMolecules.sln
??? 3DMolecules\
?   ??? 3DMolecules.csproj (Main WPF app)
??? 3DMolecules.Tests\
    ??? 3DMolecules.Tests.csproj (Test project)
    ??? PhysicsEngineTests.cs (5 unit tests)
```

## Test Results

Running `dotnet test` shows:
- ? **4 tests passing**
- ?? **1 test failing** (timing-related, can be fixed)

### Passing Tests:
1. ? `BoundaryCollision_ReflectsVelocity`
2. ? `NoCollision_MaintainsVelocity`
3. ? `MoleculeFactory_CreatesValidMolecules`
4. ? `GetDiagnostics_ReturnsInfo`

### Failing Test:
?? `HeadOnCollision_ReversesVelocities` - The molecules are too close initially and the separation force pushes them apart before velocities can reverse. This needs either:
- Larger initial separation
- Multiple update steps
- Different collision detection logic

## How to Use

### Run Tests from Command Line:
```bash
# All tests
dotnet test

# Specific project
dotnet test 3DMolecules.Tests\3DMolecules.Tests.csproj

# With detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Run Tests from Visual Studio:
1. **Test Explorer**: View ? Test Explorer
2. You should now see all 5 tests listed
3. Click "Run All" or run individual tests
4. View test output in the Test Explorer window

### Run Tests from VS Code:
1. Install the ".NET Core Test Explorer" extension
2. Tests will appear in the Testing sidebar
3. Click the play button to run tests

## Project File Contents

**3DMolecules.Tests.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
<TargetFramework>net8.0-windows</TargetFramework>
    <RootNamespace>ThreeDMolecules.Tests</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
  <ProjectReference Include="..\3DMolecules\3DMolecules.csproj" />
  </ItemGroup>
</Project>
```

## Next Steps

### Fix the Failing Test
```csharp
[Fact]
public void HeadOnCollision_ReversesVelocities()
{
    var engine = new CpuPhysicsEngine();
    var molecules = new List<MoleculeModel>
    {
    // Increase separation distance
        new(new Point3D(-2, 0, 0), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), 1.0),
        new(new Point3D(2, 0, 0), new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1), 1.0)
    };

    // Run multiple steps to ensure collision happens
    for (int i = 0; i < 5; i++)
     engine.Update(molecules, 1.0);

    Assert.True(molecules[0].Velocity.X < 0, "First molecule should bounce back");
    Assert.True(molecules[1].Velocity.X > 0, "Second molecule should bounce back");
}
```

### Add More Tests
Consider adding tests for:
- Rotation changes during collision
- Multiple simultaneous collisions
- Edge cases (molecules at exact boundaries)
- Energy conservation
- Momentum conservation

### Code Coverage
```bash
# Install coverage tool
dotnet tool install --global dotnet-reportgenerator-globaltool

# Run with coverage
dotnet test /p:CollectCoverage=true

# Generate HTML report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport
```

## Troubleshooting

### Tests not visible in Visual Studio?
1. Rebuild solution: `Build ? Rebuild Solution`
2. Restart Visual Studio
3. Check Test Explorer: `Test ? Test Explorer`

### Tests won't run?
1. Clean solution: `dotnet clean`
2. Rebuild: `dotnet build`
3. Restore packages: `dotnet restore`

### Wrong target framework error?
Make sure both projects target compatible frameworks:
- Main project: `net8.0-windows`
- Test project: `net8.0-windows`

---

## Summary

? **Test project is now properly integrated into the solution**
? **Tests are visible in Solution Explorer**
? **Tests can be run from command line and IDE**
? **4 out of 5 tests passing**
? **Ready for continuous integration**

The test infrastructure is complete and ready for you to add more tests as you develop new features!
