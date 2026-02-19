terraform {
  required_version = ">= 1.7.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.110"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.53"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.13"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.30"
    }
  }

  # Backend is configured via -backend-config flags (local dev) or GitHub Actions secrets (CI/CD).
  # Local dev:  terraform init -backend-config=backend-local.hcl
  # CI/CD:      see .github/workflows/infra-apply.yml
  backend "azurerm" {}
}

provider "azurerm" {
  subscription_id = var.subscription_id
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }
  }
}

provider "azuread" {}

# The AKS context name matches the cluster name. Run:
#   az aks get-credentials --resource-group <rg> --name aks-partsunlimited-dev
# to populate ~/.kube/config before running Terraform locally.
provider "helm" {
  kubernetes {
    config_path    = "~/.kube/config"
    config_context = "aks-partsunlimited-dev"
  }
}

provider "kubernetes" {
  config_path    = "~/.kube/config"
  config_context = "aks-partsunlimited-dev"
}
