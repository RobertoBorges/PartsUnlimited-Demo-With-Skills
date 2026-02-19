# CI/CD Pipeline Setup Report

**Project:** PartsUnlimited .NET 8 Migration  
**Phase:** 5 — CI/CD Pipeline Setup  
**Date:** 2026-02-18  
**Status:** ✅ Complete

---

## 1. Pipeline Architecture

Three GitHub Actions workflows were created, covering the full DevOps lifecycle:

```
Pull Request ──► ci.yml          ──► Build · Test · Scan · Validate
Push to main ──► cd.yml          ──► Build · Push to ACR · Deploy to AKS
Manual       ──► infra-apply.yml ──► Terraform Plan / Apply / Destroy
```

### Authentication Model

All pipelines authenticate to Azure using **OIDC federation** — no stored credentials:

| Workflow   | Federated Subject |
|------------|-------------------|
| ci.yml     | `pull_request` |
| cd.yml     | `ref:refs/heads/main` |
| infra-apply.yml | `ref:refs/heads/main` |

- **App Registration:** `partsunlimited-github-actions` (App ID `73a5b6c5-70a8-4df3-ae32-2c34335b02fb`)
- **GitHub Secrets (non-sensitive IDs only):** `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`

---

## 2. CI Pipeline — `.github/workflows/ci.yml`

**Trigger:** `pull_request` → `main`, `workflow_dispatch`

| Job | Purpose | Tools |
|-----|---------|-------|
| `dotnet-build` | Restore, build, test | dotnet 8 SDK |
| `docker-build` | Validate Dockerfile (no push) | Docker Buildx |
| `security-scan` | Container vulnerability scan | Trivy → SARIF → GitHub Advanced Security |
| `terraform-validate` | fmt check, init, validate | Hashicorp setup-terraform@v3 |
| `dependency-audit` | Check for vulnerable NuGet packages | `dotnet list package --vulnerable` |

**Quality Gates:**
- Trivy scan must complete (uploads to GitHub Security tab)
- Terraform fmt and validate must pass
- dotnet build must succeed

---

## 3. CD Pipeline — `.github/workflows/cd.yml`

**Trigger:** `push` to `main` (paths: `src/**`, `helm/**`), `workflow_dispatch`

| Job | Purpose |
|-----|---------|
| `build-and-push` | Build Docker image, push `:latest` + `:<SHA>` to ACR |
| `deploy-to-aks` | `kubectl rollout restart`, wait for rollout, health check |

**Deployment Strategy:**
- `imagePullPolicy: Always` + `:latest` tag → rolling update on pod restart
- Concurrency: `group: deploy-production`, `cancel-in-progress: false` (safe queue)
- Environment: `production` (can add GitHub deployment protection rules)
- Health check: verifies ready replicas = desired replicas after rollout

**Key Variables (GitHub Repository Variables):**

| Variable | Value |
|----------|-------|
| `ACR_LOGIN_SERVER` | `acrpartsunlimiteddevy5zz.azurecr.io` |
| `AKS_CLUSTER_NAME` | `aks-partsunlimited-dev` |
| `AKS_RESOURCE_GROUP` | `rg-partsunlimited-dev` |

---

## 4. Infrastructure Pipeline — `.github/workflows/infra-apply.yml`

**Trigger:** `workflow_dispatch` only (manual)

**Inputs:**
- `action`: `plan` | `apply` | `destroy`
- `target` (optional): Terraform resource target (e.g. `module.storage`)

**Safety controls:**
- `apply` and `destroy` require `production` GitHub environment (can add mandatory approval)
- Plan artifact uploaded for audit trail (30-day retention)
- Plan summary posted to workflow step summary

**Backend config passed via `-backend-config` flags (no sensitive values in code):**

| Variable | Value |
|----------|-------|
| `TF_STATE_RESOURCE_GROUP` | `rg-partsunlimited-dev` |
| `TF_STATE_STORAGE_ACCOUNT` | `stpartsunlimitedtfstate` |
| `TF_STATE_CONTAINER` | `tfstate` |

---

## 5. Remote Terraform State

| Item | Value |
|------|-------|
| Storage Account | `stpartsunlimitedtfstate` |
| Container | `tfstate` |
| State Key (local dev) | `partsunlimited-local.tfstate` |
| State Key (CI/CD) | `partsunlimited.tfstate` |
| Auth | Azure AD (`use_azuread_auth = true`) — no SharedKey |
| Shared Key Access | Disabled (policy compliant) |

### Local Development

```powershell
# First-time setup:
cd infra
terraform init -backend-config="backend-local.hcl"

# Run bootstrap script to (re)create backend-local.hcl if missing:
..\scripts\bootstrap-cicd.ps1
```

The `infra/backend-local.hcl` file is gitignored — it must be created on each developer machine using the bootstrap script.

---

## 6. RBAC Assignments (GitHub Actions SP)

| Role | Scope |
|------|-------|
| `Contributor` | `rg-partsunlimited-dev` |
| `AcrPush` | `acrpartsunlimiteddevy5zz` |
| `Key Vault Secrets Officer` | `kv-partsunlimited-dev` |
| `Key Vault Crypto Officer` | `kv-partsunlimited-dev` |
| `Azure Kubernetes Service Cluster Admin Role` | `aks-partsunlimited-dev` |
| `Storage Blob Data Owner` | `stpartsunlimitedtfstate/tfstate` container |

---

## 7. GitHub Secrets & Variables to Configure

### Required Secrets (set once — values are non-sensitive IDs)

```powershell
gh secret set AZURE_CLIENT_ID       --body "73a5b6c5-70a8-4df3-ae32-2c34335b02fb"
gh secret set AZURE_TENANT_ID       --body "7bf7ca02-20a6-4cc7-a35d-8fa9c5fd4529"
gh secret set AZURE_SUBSCRIPTION_ID --body "3464f809-852d-45e3-846c-f5411419cc83"
```

### Required Secrets (use your secrets manager — sensitive values)

```powershell
gh secret set TF_VAR_ENTRA_CLIENT_SECRET   --body "<from terraform.tfvars>"
gh secret set TF_VAR_SQL_ADMIN_LOGIN       --body "<from terraform.tfvars>"
gh secret set TF_VAR_SQL_ADMIN_PASSWORD    --body "<from terraform.tfvars>"
```

### Repository Variables (non-sensitive)

```powershell
gh variable set ACR_LOGIN_SERVER          --body "acrpartsunlimiteddevy5zz.azurecr.io"
gh variable set AKS_CLUSTER_NAME          --body "aks-partsunlimited-dev"
gh variable set AKS_RESOURCE_GROUP        --body "rg-partsunlimited-dev"
gh variable set TF_STATE_RESOURCE_GROUP   --body "rg-partsunlimited-dev"
gh variable set TF_STATE_STORAGE_ACCOUNT  --body "stpartsunlimitedtfstate"
gh variable set TF_STATE_CONTAINER        --body "tfstate"
gh variable set TF_VAR_LOCATION           --body "canadacentral"
gh variable set TF_VAR_ENVIRONMENT        --body "dev"
gh variable set TF_VAR_AKS_KUBERNETES_VERSION --body "1.32"
gh variable set TF_VAR_ENTRA_CLIENT_ID    --body "7b569f5c-78f1-4cf4-bf01-ea5c02350670"
gh variable set TF_VAR_ENTRA_TENANT_ID    --body "7bf7ca02-20a6-4cc7-a35d-8fa9c5fd4529"
```

Or run the bootstrap script which sets these automatically (except sensitive secrets):

```powershell
.\scripts\bootstrap-cicd.ps1
```

---

## 8. Security Integration

| Control | Implementation |
|---------|---------------|
| No stored credentials | OIDC federation via `azure/login@v2` |
| Container scanning | Trivy on every PR + CD build |
| Dependency audit | `dotnet list package --vulnerable` on every PR |
| Secret management | Azure Key Vault (managed identity access) |
| Least privilege | SP roles scoped to specific resources, not subscription |
| SharedKey disabled | TF state storage uses Azure AD auth only |
| Image tagging | SHA-pinned tags for traceability |

---

## 9. Monitoring & Observability

Application Insights is already deployed and connected to the PartsUnlimited app (from Phase 3). The CD pipeline posts a deployment annotation:
- Step summary shows: image tag, digest, pod health at deploy time
- GitHub environment `production` tracks deployment history

---

## 10. Bootstrap Script

`scripts/bootstrap-cicd.ps1` automates the full setup:

```powershell
# Full setup (requires az CLI + gh CLI logged in):
.\scripts\bootstrap-cicd.ps1

# Preview changes without making them:
.\scripts\bootstrap-cicd.ps1 -WhatIf
```

---

## 11. Troubleshooting Guide

| Issue | Cause | Fix |
|-------|-------|-----|
| `AADSTS70011: invalid_scope` | Federated credential subject mismatch | Verify `issuer` and `subject` on the App Registration |
| `Error: Backend initialization required` | No `backend-local.hcl` | Run `bootstrap-cicd.ps1` or create file manually |
| `403 on blob storage` | Missing `Storage Blob Data Owner` | Check RBAC on `tfstate` container |
| `Unauthorized on ACR` | Missing `AcrPush` role | Verify role assignment for GitHub Actions SP |
| `kubectl: cluster not reachable` | AKS credentials not fetched | Ensure `az aks get-credentials` step runs before `kubectl` |
| Rolling update stuck | Health check pods not ready | Check `kubectl describe pod` for image pull or OOM errors |

---

*Report generated: 2026-02-18 | Migration Phase 5 of 5 — Complete*
