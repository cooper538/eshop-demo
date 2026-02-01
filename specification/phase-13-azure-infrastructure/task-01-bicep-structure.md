# Task 01: Bicep Project Structure

## Metadata
| Key | Value |
|-----|-------|
| ID | task-01 |
| Status | pending |
| Dependencies | - |

## Summary
Set up the `infra/` folder structure with Bicep modules, main orchestration file, and parameter files for different environments.

## Scope
- [ ] Create `infra/` folder structure in project root
- [ ] Create `main.bicep` entry point with module orchestration (subscription scope)
- [ ] Create `modules/` folder for resource modules
- [ ] Create `parameters/` folder with `dev.bicepparam` and `prod.bicepparam`
- [ ] Create `bicepconfig.json` for linting rules and module aliases
- [ ] Add `.gitignore` entries for Bicep build outputs

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 9.1 Infrastructure as Code)

---
## Notes
- Use Bicep instead of ARM templates for better readability
- All modules should have clearly defined inputs/outputs
