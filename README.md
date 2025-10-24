# 3DMolecules

## Overview
3DMolecules is a WPF (.NET 8) application that simulates and visualizes water molecules (H?O) moving and colliding inside a transparent cylindrical container. It uses HelixToolkit.Wpf for 3D rendering and follows **clean MVVM architecture** with proper separation of concerns.

![Screenshot](3Dmolecules.png)

## Key Features
- ? Real-time 3D molecular motion with elastic collision physics
- ?? Beautiful transparent cylinder with decorative elements
- ?? Interactive controls (Start / Pause / Reset / Molecule Count Slider)
- ?? Live FPS counter and physics diagnostics
- ??? **Clean MVVM architecture** with dependency injection
- ?? **Proper resource management** (IDisposable)
- ?? **Fully tested** - 5 unit tests verify physics correctness
- ?? **Extensible** - Easy to swap physics engines or add features

## Technology Stack
- .NET 8 / C# 12
- WPF / XAML
- HelixToolkit.Wpf (3D rendering)
- xUnit (testing framework)
- MVVM pattern with dependency injection

---

## ??? Architecture

### **Clean MVVM Separation**

```
View (XAML)
    ? Data Binding
ViewModel (Coordinates)
    ? Uses
Services (Physics Engine)
    ? Works with
Models (Pure Data)
```

### **Layers**

**Models** (`Models/`)
- `MoleculeModel` - Pure physics data (position, velocity, rotation)
- No dependencies on UI or rendering

**Services** (`Services/`)
- `IPhysicsEngine` - Interface for physics implementations
- `CpuPhysicsEngine` - Current O(n²) collision detection
- `MoleculeFactory` - Creates randomized molecules

**ViewModels** (`ViewModels/`)
- `SimulationViewModel` - Main coordinator with DI support
- `BaseViewModel` - INotifyPropertyChanged implementation
- `RelayCommand<T>` - Command pattern with generic parameters

**Views** (`*.xaml`)
- `MainWindow.xaml` - Pure data binding, no code-behind logic
- `Molecule` - 3D visual representation that syncs from `MoleculeModel`

**Configuration**
- `SimulationSettings` - All constants in one place (no magic numbers!)

---

## ?? Testing

The project includes **5 unit tests** that verify physics correctness:

```bash
# Run all tests
dotnet test

# Output:
Test Run Successful.
Total tests: 5
     Passed: 5
Total time: 1.6 seconds
```

**Tests verify:**
- ? Head-on collisions reverse velocities
- ? Boundary collisions reflect molecules
- ? Non-colliding molecules maintain velocity
- ? Factory creates valid molecules
- ? Diagnostics report status

**?? See [HOW_TO_USE_TESTS.md](HOW_TO_USE_TESTS.md) for detailed testing guide**

---

## ?? Documentation

### **For Understanding the Refactoring:**
- **[WHAT_WE_DID.md](WHAT_WE_DID.md)** - Quick summary of changes
- **[MVVM_REFACTORING_EXPLAINED.md](MVVM_REFACTORING_EXPLAINED.md)** - Detailed before/after comparison with code examples
- **[REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md)** - Comprehensive summary with metrics

### **For Using the Tests:**
- **[HOW_TO_USE_TESTS.md](HOW_TO_USE_TESTS.md)** - Complete guide to running and understanding tests
- **[TEST_PROJECT_SETUP.md](TEST_PROJECT_SETUP.md)** - How the test project was configured

### **Historical:**
- **[MVVM_IMPROVEMENTS.md](MVVM_IMPROVEMENTS.md)** - Original improvement proposal

---

## ?? Quick Start

### **Build & Run**
```bash
git clone https://github.com/kajencik/3DMolecules.git
cd 3DMolecules
dotnet build
dotnet run --project 3DMolecules/3DMolecules.csproj
```

Or open `3DMolecules.sln` in Visual Studio 2022+ and press **F5**.

### **Run Tests**
```bash
dotnet test
```

### **Project Structure**
```
3DMolecules/
??? 3DMolecules/         # Main WPF application
?   ??? Models/        # Pure data models
?   ??? Services/       # Physics engines & factories
?   ??? ViewModels/           # MVVM ViewModels
?   ??? Views/  # XAML views
?   ??? Molecule.cs           # 3D visual component
?   ??? CylindricalBoundary.cs
?   ??? SimulationSettings.cs # Configuration constants
?
??? 3DMolecules.Tests/        # Unit test project
?   ??? PhysicsEngineTests.cs # 5 physics tests
?
??? Documentation/ # Markdown documentation
  ??? README.md    # This file
    ??? WHAT_WE_DID.md
    ??? MVVM_REFACTORING_EXPLAINED.md
    ??? HOW_TO_USE_TESTS.md
    ??? ...
```

---

## ?? Key Achievements

### **Before Refactoring**
- ? Physics mixed with rendering in `Molecule.cs`
- ? Magic numbers scattered everywhere
- ? No tests possible
- ? Memory leaks (timer not disposed)
- ? Hard to change or extend

### **After Refactoring**
- ? Clean separation: Models ? Services ? ViewModels ? Views
- ? All constants in `SimulationSettings`
- ? 5 passing unit tests
- ? Proper `IDisposable` implementation
- ? Easy to swap physics engines via `IPhysicsEngine`

### **Code Quality**
| Metric | Before | After |
|--------|--------|-------|
| **Testability** | ? | ? 5 tests |
| **Magic Numbers** | ~15 | 0 |
| **Resource Leaks** | Yes | No |
| **Lines of Code** | ~600 | ~800 (more organized) |
| **Architecture** | Mixed | Clean MVVM |

---

## ?? Extending the Simulation

### **Change Molecule Count**
Use the slider in the UI (0-240) or modify `SimulationSettings.DefaultMoleculeCount`.

### **Add Different Physics Engine**
```csharp
// Implement the interface
public class GpuPhysicsEngine : IPhysicsEngine
{
    public void Update(IList<MoleculeModel> molecules, double deltaTime)
    {
   // GPU acceleration code here
    }
    
    public string GetDiagnostics() => "GPU Engine | ...";
}

// Use it (no other changes needed!)
var viewModel = new SimulationViewModel(new GpuPhysicsEngine(), factory);
```

### **Add Spatial Partitioning**
```csharp
public class SpatialGridPhysicsEngine : IPhysicsEngine
{
    // O(n) collision detection instead of O(n²)
}
```

### **Save/Load Simulation**
```csharp
// Models are serializable
var json = JsonSerializer.Serialize(moleculeModels);
File.WriteAllText("simulation.json", json);
```

---

## ?? Performance

**Current Capacity:** ~240 molecules at 60 FPS

**Optimization Roadmap:**
1. **Spatial Partitioning** (Grid/Octree) ? 5,000-8,000 molecules
2. **Geometry Instancing** ? Better rendering
3. **Parallel Processing** ? Multi-core utilization
4. **GPU Compute Shaders** ? 50,000-100,000+ molecules

The clean architecture makes these optimizations easy to implement without major rewrites.

---

## ?? Contributing

Contributions welcome! Priority areas:
- ?? Spatial partitioning implementation
- ?? GPU acceleration (ComputeSharp/ILGPU)
- ?? Additional unit tests (energy conservation, momentum, etc.)
- ?? UI enhancements (molecule selection, themes)
- ?? Performance profiling and benchmarks

### **Development Workflow**
1. Read the documentation (`MVVM_REFACTORING_EXPLAINED.md`)
2. Make your changes
3. Run tests: `dotnet test` (should all pass)
4. Submit pull request

---

## ?? Learning Resources

This project demonstrates:
- ? **MVVM pattern** in WPF applications
- ? **Dependency Injection** without frameworks
- ? **SOLID principles** (Single Responsibility, Dependency Inversion, etc.)
- ? **Unit testing** physics logic in isolation
- ? **Resource management** with `IDisposable`
- ? **Configuration management** (centralized constants)
- ? **3D graphics** with HelixToolkit.Wpf
- ? **Physics simulation** (collision detection & response)

Perfect for learning professional WPF development practices! ??

---

## ?? License

MIT License – see `LICENSE`.

## ?? Acknowledgments

- [HelixToolkit.Wpf](https://github.com/helix-toolkit/helix-toolkit) - 3D rendering
- WPF / .NET team - Framework
- Clean Code & SOLID principles community
- AI assistance (GitHub Copilot Chat) - Architecture guidance

---

## ?? Project Status

**Current Version:** v2.0 (MVVM Refactored)

**Status:** ? Stable - All tests passing, ready for extensions

**Developed with:** AI-assisted coding (GitHub Copilot Chat) following industry best practices

---

**Enjoy exploring molecules in 3D with clean, maintainable, tested code!** ???

*Questions? Check the documentation files or open an issue on GitHub.*