; Unshipped diagnostics for BlazOrbit.CodeGeneration.
; Promoted into AnalyzerReleases.Shipped.md at release time.

### New Rules

Rule ID  | Category                    | Severity | Notes
---------|-----------------------------|----------|-------------------------------------------------------------------------------------------
BOBGEN001| BlazOrbit.CodeGeneration     | Warning  | Multiple .razor components share the same simple name; @inherits resolution may pick the wrong one.
BOBGEN002| BlazOrbit.CodeGeneration     | Warning  | Ambiguous @inherits base type; multiple types match the simple name. Fully-qualify @inherits to silence.
