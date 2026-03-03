---
description: "Convert a brief feature description into a structured Feature Spec document"
name: "Write Feature Spec"
argument-hint: "Briefly describe the feature you want to spec out..."
agent: "spec-writer"
tools: [read, search, edit, web]
---

# Write Feature Spec

Convert the following feature brief into a complete Feature Spec document.

**Feature Brief**: {{feature brief}}

## Instructions for the Spec Writer Agent

1. **Explore the codebase** to identify which existing Service this feature belongs to (or if a new service is needed)
2. **Research similar patterns** already in the project (look in `Services/` for existing Commands, Events, DTOs)
3. **Draft the spec** using the Feature Spec template (see `spec-writer.agent.md`)
4. **Save** to `docs/specs/{FeatureName}.spec.md`

## Spec Quality Checklist

Before finalizing, verify the spec includes:
- [ ] Plain-language Overview (non-technical summary)
- [ ] User Stories with IDs
- [ ] BDD Acceptance Criteria (Given/When/Then) linked to User Stories
- [ ] API Contract as markdown tables (NOT raw YAML)
- [ ] Data model table with column types
- [ ] Mermaid CQRS sequence diagram
- [ ] Test scenarios table (with AC references)
- [ ] Dependencies section

## Why This Format?

> This project uses a **Feature Spec** format instead of raw OpenAPI/Swagger YAML because:
> - OpenAPI YAML is designed for machines, not humans
> - This format combines narrative + structure for both human review and AI parsing
> - BDD scenarios directly generate test cases (used by `/generate-unit-tests`)
> - Mermaid diagrams render in GitHub, VS Code, and most doc platforms
