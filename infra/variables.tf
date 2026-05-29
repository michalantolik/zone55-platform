variable "project_name" {
  description = "Short project name used in Azure resource names."
  type        = string
  default     = "blogplatform"
}

variable "environment" {
  description = "Deployment environment name."
  type        = string
  default     = "dev"
}

variable "location" {
  description = "Azure region."
  type        = string
  default     = "westeurope"
}

variable "sql_admin_login" {
  description = "Azure SQL administrator login."
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "Azure SQL administrator password."
  type        = string
  sensitive   = true
}
