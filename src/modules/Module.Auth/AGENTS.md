# Auth, Roles, and Branch-Based Permissions (Multi-Tenant RBAC)

This document describes the architecture and business rules governing access control, permission structure, and Login flow behavior.

---

## Database Structure (Relational Model)

The system uses a hybrid relational/JSON approach that guarantees data consistency, referential integrity, and flexibility in individual permission assignment.

*   **User:** Global user entity.
*   **UserBranchRoles:** Junction table breaking the many-to-many relationship. **A user has exactly one (1) Role per Branch** they belong to.
*   **Role:** Role definition within the organization (e.g. Administrator, Cashier, Warehouse).
*   **Feature:** Catalog of available modules and screens (e.g. `products`, `sales`, `pos`). Managed via Seeders in code.
*   **RoleFeaturePermission:** Connects a Role to a specific Feature under the Tenant's schema. Stores granted actions as a primitive string list (`List<string> Permissions`).

---

## Core Entity Anatomy

### 1. Feature (The Catalog of the Possible)
Defines which screens or abstract components exist in the software and what actions they expose.
*   `Key (string)`: Unique feature identifier (e.g. `"products"`). **Source of truth**.
*   `IsMenu (bool)`: UI control. Indicates whether the Frontend should render this Feature in the sidebar menu.
*   `Module (Enum)`: Business grouping (`Inventory`, `Sales`, `Core`).
*   `AvailableActions (JSON/List)`: List of configured actions (e.g. `[{Key: "read"}, {Key: "create"}]`).

### 2. RoleFeaturePermission (The Actual Assignment)
Determines which specific keys from the catalog have been granted to a Role.
*   `FeatureKey (string)`: Foreign key to the Feature.
*   `Permissions (List<string>)`: Collection of assigned actions (e.g. `["read", "create"]`).

> **Backend Validation Rule:** The backend strictly validates that any string saved in the `Permissions` list must exist in the Feature's `AvailableActions` catalog.

---

## Access Control Business Rules (Design Matrix)

To avoid ambiguity between UI rendering (Frontend) and endpoint security (Backend), the system operates under the following behavior matrix:

| Scenario | `IsMenu` | Permissions in Array | Frontend (UI) Behavior | Backend (API) Behavior |
| :--- | :---: | :---: | :--- | :--- |
| **Standard Screen** | `true` | `["read", "create"]` | Shows access in sidebar. Allows viewing the list and the "Create" button. | Allows `GET` (Read) and `POST` (Write) requests. |
| **Action without View (Least Privilege)** | `true` or `false` | `["create"]` *(No `read`)* | **Hides** screen from sidebar. Allows interacting with the Feature (e.g., creating a product) **only** as a modal/emergent action from other flows. | Blocks general `GET` requests (`403 Forbidden`). Allows `POST` for insertion. |
| **Sub-route or Invisible View** | `false` | `["read"]` | **Hidden** from sidebar. If the user navigates directly to the URL or accesses via an internal link, the Router allows it. | Allows `GET` requests. |
| **No Access** | Irrelevant | `[]` *(Or no record exists)* | Hides the menu entirely. If the user forces the URL, the Frontend Router redirects to Home/Login. | Any HTTP request returns a strict `403 Forbidden`. |

---

## Login Performance Strategy (Lazy Two-Queries)

The Login endpoint implements a **Fail Fast** pattern by splitting the query into two independent transactions to protect the database from resource waste and cartesian explosion.
