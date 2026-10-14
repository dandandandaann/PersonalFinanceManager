# Known Issues — Pre-Existing Test Failures

> **Produced during the .NET 8 → .NET 10 upgrade investigation (feature/net10-upgrade).**
> Upgrade commits: 0698d35, a1b9b0d, 4519c5f. Zero `.cs` files changed. Build: 0 errors, 34 warnings (all pre-existing nullable/style). Identical failure sets reproduce on unmodified `main`. **The user chose to document these and deploy as-is rather than fix them now.**

---

## 1. Summary Table

| Test Project | Passed | Failed | Status |
|---|---|---|---|
| TelegramListener.Tests | 1 | 0 | All green |
| SpreadsheetManagerApi.Tests | 11 | 14 | Failures are pre-existing |
| UserManagerApi.Tests | 0 | 8 | Failures are pre-existing |
| BudgetAutomation.Engine.Tests | — | — | **0 tests discovered** (missing runner package) |

---

## 2. Real Production Bug (Worth Fixing Separately)

**`SpreadsheetManagerApi/Service/CategoryService.cs:35`**

```csharp
description = description.Trim().Normalize();
```

Dereferences `description` before any null/empty guard. `DecideCategoryAsync` throws `NullReferenceException` when description is null.

**Minimal fix:**
```csharp
description = (description ?? string.Empty).Trim().Normalize();
```

**Evidence:** The test author documented this bug in a comment at `SpreadsheetManagerApi.Tests/UnitTest/CategoryServiceTests.cs:160` (Portuguese comment noting the code throws) — commit `70d6d86`, 2025-06-21, long before the upgrade.

---

## 3. UserManagerApi.Tests — All 8 Fail, Root Cause in Test Code

**`FunctionsTests.cs:57`** does `_functions = new Functions(null);` — passes a null `IUserService` into the primary constructor. Production `Functions.cs:54`/`:199` dereference it; the NRE is swallowed by the blanket catch at `Functions.cs:88`/`:228`, so every test gets a 500.

A `TODO` comment at line 55 admits the tests need rework after the service refactor.

**Secondary issues:**
- Stale expected message strings (e.g. line 209 expects `"Invalid or missing TelegramId in path."` but production now returns `"Invalid TelegramId format or value provided."`)
- Error-message mismatches throughout
- `UserService.cs:41` calls `GetNextSetAsync()` without a `CancellationToken` while tests mock it WITH one

---

## 4. SpreadsheetManagerApi.Tests — 14 Fail, 4 Distinct Test-Code Defects

### a. `ExpenseLoggerServiceTests.cs:25-26`
Mock stub returns constant `"Test Category"` from `DecideCategoryAsync`, then tests assert `result.Category` should be `"Groceries"`/`"Utilities"` — unsatisfiable by construction.

### b. `ExpenseLoggerServiceTests.cs:28`
`_expectedSheetName = DateTime.Now.ToString("MM-yyyy")` (e.g. `"10-2026"`) but production uses `SpreadsheetConstants.Transactions.SheetName` = `"Transações"`. The `ThrowsAsync` setup can never match, and the range assertions (`"!B{row}"` etc.) reference stale columns; production now writes description to C, category to D, amount to F, total to G.

### c. `ExpenseLoggerServiceTests.cs:286`
Reflection targets a private `DecideCategory` method that no longer exists (replaced by injected `ICategoryService.DecideCategoryAsync`). `GetMethod` returns null, so the assertion compares null.

### d. The CategoryService null-deref bug above (1 of the 14).

---

## 5. BudgetAutomation.Engine.Tests — 0 Tests Discovered

The csproj lacks `<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />`, which all sibling test projects have. The project contains 10 real test cases (`UtilityTests.cs` + `IntegrationTest/DependencyInjectionTests.cs`).

Adding the runner package makes them discoverable (verified experimentally in a throwaway worktree): **9 pass, 1 fails**.

**The 1 failing test:** `DependencyInjectionTests.ShouldEnsure_AllClassesWithEndingAreRegistered(classEnding: "Command")`

**Root cause:** Alias commands (`AjudaAliasCommand`, `RegistrarDespesaAliasCommand`, `UltimaDespesaAliasCommand`) are registered as their base class `AliasCommandBase` at `BudgetAutomation.Engine/Extension/ServiceCollectionExtensions.cs:99-106`. The test's convention check doesn't accept the base-class registration.

---

## 6. Optional Hardening (Not a Blocker)

`SharedLibrary.Lambda/ApiGatewayResult.cs` uses non-source-generated `JsonSerializer.Serialize<TValue>(...)` at lines 35, 74, 172, 194, 228, producing IL3050/IL2026 AOT warnings. Could be silenced by adding a `JsonSerializerContext`.

---

## 7. Transitive Dependency Advisory

**NU1901** — AWSSDK.Core 4.0.0.5 (transitive via AWSSDK.DynamoDBv2 4.0.1 / SQS 4.0.0.2) has a known low-severity vulnerability (GHSA-9cvc-h2w8-phrp). Unchanged by the upgrade.
