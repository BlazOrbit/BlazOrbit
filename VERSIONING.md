# Versioning Strategy

BlazOrbit follows **Semantic Versioning (SemVer)** with a simplified Git Flow built around two long-lived branches:
`master` and `develop`.

---

## Semantic Versioning

Versions follow the format `MAJOR.MINOR.PATCH`:

| Segment       | Change Type                                       | Example           |
|---------------|---------------------------------------------------|-------------------|
| **MAJOR (X)** | Breaking change that requires consumer action     | `1.0.0` → `2.0.0` |
| **MINOR (Y)** | New functionality, backward-compatible            | `1.0.0` → `1.1.0` |
| **PATCH (Z)** | Bug fix or small improvement, backward-compatible | `1.0.0` → `1.0.1` |

Additional labels:

| Pattern           | Meaning                                 | Example            |
|-------------------|-----------------------------------------|--------------------|
| `X.Y.Z-preview.N` | Active development build from `develop` | `1.0.0-preview.42` |
| `X.Y.Z-rc.N`      | Release candidate, feature-complete     | `1.0.0-rc.2`       |

---

## Support Policy

- **Latest stable** (`master`) receives bug fixes and security patches.

---
