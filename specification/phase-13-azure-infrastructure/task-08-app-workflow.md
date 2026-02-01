# Task 08: Application Workflow

## Metadata
| Key | Value |
|-----|-------|
| ID | task-08 |
| Status | pending |
| Dependencies | task-06, task-07 |

## Summary
Create GitHub Actions workflow for building Docker images and deploying to Azure Container Apps with parallel service builds.

## Scope
- [ ] Create `.github/workflows/app.yml` workflow file
- [ ] Configure matrix strategy for parallel service builds
- [ ] Build Docker images using Azure Container Registry (ACR) Tasks
- [ ] Deploy updated images to Container Apps
- [ ] Configure workflow triggers (push to src/, manual dispatch)
- [ ] Run database migrations before app deployment
- [ ] Output deployment URLs and health check results

## Related Specs
- -> [azure-infrastructure.md](../high-level-specs/azure-infrastructure.md) (Section: 9.2 GitHub Actions Workflows, 9.3 Deployment Order)

---
## Notes
- ACR Tasks build images in Azure (no local Docker needed)
- Matrix strategy parallelizes builds for faster deployment
- Deployment order: build images -> run migrations -> deploy services
