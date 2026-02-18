# ─── Core ───────────────────────────────────────────────────────────────────
variable "subscription_id" {
  description = "Azure subscription ID. Set in terraform.tfvars (gitignored) or ARM_SUBSCRIPTION_ID env var."
  type        = string
  sensitive   = true
}

variable "location" {
  description = "Azure region for all resources."
  type        = string
  default     = "canadacentral"
}

variable "environment" {
  description = "Deployment environment (dev | staging | prod)."
  type        = string
  default     = "dev"
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be dev, staging, or prod."
  }
}

variable "project" {
  description = "Short project name used in resource naming."
  type        = string
  default     = "partsunlimited"
}

# ─── Entra ID / OIDC ────────────────────────────────────────────────────────
variable "entra_tenant_id" {
  description = "Entra ID Tenant ID for OIDC authentication."
  type        = string
  sensitive   = true
}

variable "entra_client_id" {
  description = "Entra ID App Registration Client ID."
  type        = string
  sensitive   = true
}

variable "entra_client_secret" {
  description = "Entra ID App Registration Client Secret (stored in Key Vault at deploy time)."
  type        = string
  sensitive   = true
}

variable "deployer_ips" {
  description = "Public IPs of the machine running Terraform — allowed through Key Vault network ACL."
  type        = list(string)
  default     = ["142.113.37.92", "172.172.34.115"]
}

# ─── AKS ────────────────────────────────────────────────────────────────────
variable "aks_kubernetes_version" {
  description = "Kubernetes version for the AKS cluster."
  type        = string
  default     = "1.32"
}

variable "aks_system_node_count" {
  description = "Number of nodes in the system node pool."
  type        = number
  default     = 2
}

variable "aks_system_vm_size" {
  description = "VM size for the system node pool."
  type        = string
  default     = "Standard_D2s_v3"
}

variable "aks_user_node_count" {
  description = "Number of nodes in the user (app) node pool."
  type        = number
  default     = 2
}

variable "aks_user_vm_size" {
  description = "VM size for the user node pool."
  type        = string
  default     = "Standard_D2s_v3"
}

# ─── Azure SQL ───────────────────────────────────────────────────────────────
variable "sql_admin_login" {
  description = "SQL Server administrator login (used only for initial setup; Managed Identity auth is preferred)."
  type        = string
  default     = "sqladmin"
  sensitive   = true
}

variable "sql_admin_password" {
  description = "SQL Server administrator password (rotated after bootstrap; store in Key Vault)."
  type        = string
  sensitive   = true
}

variable "sql_sku_name" {
  description = "Azure SQL Database SKU (e.g., GP_Gen5_2)."
  type        = string
  default     = "GP_Gen5_2"
}

# ─── Container image ─────────────────────────────────────────────────────────
variable "app_image_tag" {
  description = "Docker image tag for the PartsUnlimited web application."
  type        = string
  default     = "latest"
}

# ─── Azure ML ────────────────────────────────────────────────────────────────
variable "azure_ml_account_key" {
  description = "Azure ML account key for recommendation engine (stored in Key Vault)."
  type        = string
  sensitive   = true
  default     = ""
}

variable "azure_ml_base_url" {
  description = "Azure ML recommendation engine base URL."
  type        = string
  default     = ""
}
