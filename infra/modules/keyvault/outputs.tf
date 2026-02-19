output "vault_uri" { value = azurerm_key_vault.main.vault_uri }
output "vault_id"  { value = azurerm_key_vault.main.id }
output "dp_key_id" {
  description = "Key Vault key ID for Data Protection key wrapping."
  value       = azurerm_key_vault_key.dataprotection.id
}
