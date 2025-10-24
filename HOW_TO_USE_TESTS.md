# How to Use Unit Tests - A Practical Guide

## ?? **What Are These Tests For?**

The tests verify that your physics engine works correctly **without running the full application**. This means you can:
- ? Quickly check if physics logic is correct
- ? Catch bugs before they appear in the UI
- ? Verify changes don't break existing behavior
- ? Understand how the physics engine should behave

---

## ? **Current Status - All Tests Pass!**

```
Test Run Successful.
Total tests: 5
     Passed: 5
Total time: 1.6 seconds
```

What this means:
1. ? **Collision detection works** - Molecules bounce off each other correctly
2. ? **Boundary enforcement works** - Molecules stay inside the cylinder
3. ? **Non-colliding molecules work** - Molecules don't interfere when far apart
4. ? **Factory works** - Random molecules are created with valid properties
5. ? **Diagnostics work** - Physics engine reports its status

---

## ?? **Understanding Each Test**

### **Test 1: `HeadOnCollision_ReversesVelocities`** ?

**What it tests:** When two molecules collide head-on, they should bounce back.

```csharp
// Place molecules 4 units apart, moving toward each other
Molecule 1: at (-2, 0, 0), moving RIGHT at +0.5
Molecule 2: at (+2, 0, 0), moving LEFT at -0.5

// Run simulation for up to 10 steps
engine.Update(molecules, 1.0);

// After collision:
// Molecule 1 should bounce LEFT (velocity.X < 0)
// Molecule 2 should bounce RIGHT (velocity.X > 0)
```

**Why it's important:** Verifies the core collision detection and response logic.

---

### **Test 2: `BoundaryCollision_ReflectsVelocity`** ?

**What it tests:** Molecules should bounce off the cylinder walls.

```csharp
// Place molecule OUTSIDE boundary
Position: (11, 0, 0)  // Cylinder radius is 10
Velocity: (+1, 0, 0)   // Moving away from center

engine.Update(molecules, 1.0);

// After update:
// - Velocity should reverse: X becomes negative
// - Position should be inside: distance ? 10
```

**Why it's important:** Ensures molecules stay within the simulation boundaries.

---

### **Test 3: `NoCollision_MaintainsVelocity`** ?

**What it tests:** Molecules far apart should not affect each other.

```csharp
Molecule 1: at (0, 0, 0), velocity (0.5, 0.3, 0.2)
Molecule 2: at (5, 5, 5), velocity (0, 0, 0)

Distance = ?(5² + 5² + 5²) = 8.66 units
Collision radius = 1.2 units
// No collision should occur

engine.Update(molecules, 1.0);

// Molecule 1's velocity should be unchanged
```

**Why it's important:** Prevents false positive collisions and validates collision detection accuracy.

---

### **Test 4: `MoleculeFactory_CreatesValidMolecules`** ?

**What it tests:** Factory creates molecules with valid properties.

```csharp
var factory = new MoleculeFactory(new Random(42));
var molecule = factory.CreateRandom();

// Checks:
// ? Molecule is not null
// ? Velocity > 0 (molecules should move)
// ? RotationSpeed > 0 (molecules should spin)
// ? RotationAxis is normalized (length ? 1.0)
```

**Why it's important:** Ensures initialization logic creates valid simulation objects.

---

### **Test 5: `GetDiagnostics_ReturnsInfo`** ?

**What it tests:** Physics engine provides diagnostic information.

```csharp
engine.Update(molecules, 1.0);
var diagnostics = engine.GetDiagnostics();

// Should return something like:
// "CPU Engine | Checks: 1 | Collisions: 0"
```

**Why it's important:** Enables performance monitoring and debugging.

---

## ?? **How to Run the Tests**

### **Option 1: Command Line (Easiest)**

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~BoundaryCollision"
```

**Example output:**
```
Test Run Successful.
Total tests: 5
     Passed: 5
 Total time: 1.6 seconds
```

### **Option 2: Visual Studio**

1. **Open Test Explorer**
   - Menu: `Test` ? `Test Explorer`
   - Keyboard: `Ctrl+E, T`

2. **You'll see all tests:**
   ```
   ?? PhysicsEngineTests
? HeadOnCollision_ReversesVelocities
    ? BoundaryCollision_ReflectsVelocity
      ? NoCollision_MaintainsVelocity
   ? MoleculeFactory_CreatesValidMolecules
      ? GetDiagnostics_ReturnsInfo
   ```

3. **Run tests:**
   - Click "Run All" (?? button)
   - Or right-click individual test ? "Run"

4. **View results:**
   - Green ? = Passed
   - Red ? = Failed (with error details)

### **Option 3: Visual Studio Code**

1. Install extension: ".NET Core Test Explorer"
2. Tests appear in Testing sidebar (?? icon)
3. Click play button to run

---

## ?? **Reading Test Results**

### **When All Tests Pass ?**

```
Passed!  - Failed:     0, Passed:     5, Skipped:     0, Total:     5
```

**Meaning:** All physics logic is working as expected!

### **When a Test Fails ?**

```
? HeadOnCollision_ReversesVelocities (3 ms)
   Error Message: First molecule should bounce back
   Stack Trace:
      at PhysicsEngineTests.HeadOnCollision_ReversesVelocities()
    in PhysicsEngineTests.cs:line 34
```

**How to diagnose:**
1. **Read error message:** "First molecule should bounce back"
2. **Look at line 34:** The specific assertion that failed
3. **Understand the issue:** Velocity didn't reverse as expected
4. **Check the code:** Look at `CpuPhysicsEngine.HandleCollision()`

---

## ?? **Test-Driven Development Workflow**

### **Scenario: You want to change collision physics**

```bash
# 1. Run tests BEFORE making changes
dotnet test
# ? All 5 tests pass - baseline established

# 2. Make your changes to CpuPhysicsEngine.cs
# (e.g., change CollisionRestitution from 0.8 to 0.5)

# 3. Run tests AFTER changes
dotnet test
# ?? Maybe some tests fail - behavior changed

# 4. Review results:
# - If tests fail but new behavior is correct: Update tests
# - If tests fail and behavior is wrong: Fix your code

# 5. Verify
dotnet test
# ? All tests pass again
```

### **Scenario: You add a new feature**

```csharp
// 1. Write test FIRST (Test-Driven Development)
[Fact]
public void RotationSpeed_DecreasesOverTime()
{
  var engine = new CpuPhysicsEngine();
    var molecule = new MoleculeModel(...);
    molecule.RotationSpeed = 10.0;
    
    engine.Update(new[] { molecule }, 1.0);
    
    // Expect friction to slow rotation
    Assert.True(molecule.RotationSpeed < 10.0, "Friction should slow rotation");
}

// 2. Run test - it FAILS ? (feature doesn't exist yet)

// 3. Implement feature in CpuPhysicsEngine
private void UpdateMolecule(MoleculeModel molecule, double deltaTime)
{
    // Add rotational friction
    molecule.RotationSpeed *= 0.99;  // 1% friction
    // ...
}

// 4. Run test again - it PASSES ?
```

---

## ?? **Practical Examples**

### **Example 1: Verify Changes Don't Break Things**

```bash
# You want to optimize collision detection

# Step 1: Baseline
dotnet test
? All 5 tests pass

# Step 2: Make changes to CpuPhysicsEngine
# ... optimize collision loop ...

# Step 3: Verify
dotnet test
? All 5 tests still pass
# Your optimization didn't break anything!
```

### **Example 2: Find Bugs Quickly**

```bash
dotnet test

? BoundaryCollision_ReflectsVelocity fails
   Error: "Should be within boundary"

# Now you know exactly what's broken:
# - Boundary enforcement logic
# - Look at EnforceBoundary() method

# Fix the bug, then:
dotnet test
? All pass - bug fixed!
```

### **Example 3: Test Without Running Full App**

**Before (manual testing):**
1. Run application (5 seconds to start)
2. Watch molecules visually
3. Try to observe collision behavior
4. Not sure if it's working correctly
5. Close and restart to try different scenario

**After (automated testing):**
```bash
dotnet test
# ? All collision logic verified in 1.6 seconds!
# No need to run the UI
```

---

## ?? **Adding Your Own Tests**

Want to test something specific? Add a new test method:

```csharp
[Fact]
public void EnergyConservation_MaintainedAfterCollision()
{
    // Arrange
    var engine = new CpuPhysicsEngine();
    var molecules = new List<MoleculeModel>
    {
        new(new Point3D(-1, 0, 0), new Vector3D(2, 0, 0), ...),
        new(new Point3D(1, 0, 0), new Vector3D(-2, 0, 0), ...)
    };
    
    // Calculate initial kinetic energy
    double initialEnergy = CalculateKineticEnergy(molecules);
    
    // Act - Let collision happen
    for (int i = 0; i < 10; i++)
     engine.Update(molecules, 1.0);
    
    // Assert - Energy should be conserved (approximately)
    double finalEnergy = CalculateKineticEnergy(molecules);
    Assert.True(Math.Abs(initialEnergy - finalEnergy) < 0.1, "Energy should be conserved");
}

private double CalculateKineticEnergy(List<MoleculeModel> molecules)
{
    return molecules.Sum(m => 0.5 * m.Velocity.LengthSquared);
}
```

Then run: `dotnet test` - your new test will run automatically!

---

## ?? **What Tests Give You**

### **Confidence**
```
? "I changed collision code - did I break anything?"
   ? Run tests - all pass = safe to proceed
```

### **Speed**
```
? "Does boundary collision work?"
   ? 1.6 seconds to verify vs. manual testing
```

### **Documentation**
```
? "How should collisions work?"
   ? Read HeadOnCollision test - shows expected behavior
```

### **Regression Prevention**
```
? "I fixed a bug - how do I prevent it from coming back?"
   ? Write a test for that bug - it won't return unnoticed
```

---

## ?? **Summary**

### **What Tests Do**
- ? Verify physics logic works correctly
- ? Catch bugs before they reach the UI
- ? Document expected behavior
- ? Give confidence when making changes
- ? Prevent regressions

### **How to Use Them**
1. **Before changes:** Run tests (establish baseline)
2. **After changes:** Run tests (verify nothing broke)
3. **When adding features:** Write test first, then implement
4. **When fixing bugs:** Write test that fails, then fix it

### **Quick Commands**
```bash
# Run all tests
dotnet test

# Run with details
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "HeadOnCollision"

# Watch mode (auto-run on file changes)
dotnet watch test
```

### **Current Project Status**
```
? All 5 tests passing
? Physics engine verified
? Collision detection validated
? Boundary enforcement confirmed
? Object creation validated
```

---

**Bottom line:** Tests let you verify your code works **quickly, automatically, and confidently**, without manually running the application and visually inspecting results every time! ??

**Your physics engine is now professionally verified!** Any changes you make can be tested in seconds to ensure nothing breaks.
