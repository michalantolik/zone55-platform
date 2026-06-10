# Documentation

This folder contains project documentation for BlogPlatform.

---

## Documentation Index

| File | Purpose |
|---|---|
| `../README.md` | Main project overview |
| `../AZURE.md` | Azure deployment roadmap and current status |
| `adr/README.md` | Architecture Decision Records |
| `secrets-and-configuration.md` | Secrets and configuration flow |
| `../infra/README.md` | Terraform infrastructure documentation |
| `../src/README.md` | Source code structure |
| `../src/BlogPlatform/BlogPlatform.Cms/README.md` | CMS-specific documentation |
| `../tests/README.md` | Test documentation |

---

## Recommended Reading Order

1. `../README.md`
2. `../AZURE.md`
3. `adr/README.md`
4. `secrets-and-configuration.md`
5. `../infra/README.md`
6. `../src/README.md`

---

## Notes

Some configuration values are intentionally not stored in the repository.

Do not commit:

* `infra/terraform.tfvars`
* Any `*.tfvars` file except `*.tfvars.example`
* Azure publish profiles
* Local secrets
* Local environment files
* Terraform state files
