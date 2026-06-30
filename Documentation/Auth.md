# 🔐 Sistema de Autenticación, Roles y Permisos por Sucursal (RBAC Multi-Tenant)

Este documento describe la arquitectura y las reglas de negocio que gobiernan el control de acceso, la estructura de permisos y el comportamiento del flujo de Login en el sistema.

---

## 🏗️ Estructura de la Base de Datos (Modelo Relacional)

El sistema utiliza un enfoque híbrido relacional/JSON que garantiza consistencia de datos, integridad referencial y flexibilidad en la asignación de permisos individuales.

*   **User:** Entidad global del usuario.
*   **UserBranchRoles:** Tabla intermedia que rompe la relación de muchos a muchos. **Un usuario tiene exactamente un (1) Rol por cada Sucursal (Branch)** a la que pertenece.
*   **Role:** Definición del rol dentro de la organización (ej. Administrador, Cajero, Almacenero).
*   **Feature:** El catálogo de módulos y pantallas disponibles en el sistema (ej. `products`, `sales`, `pos`). Es administrada mediante Seeders en el código.
*   **RoleFeaturePermission:** Conecta un Rol con una Feature específica bajo el esquema del Tenant. Almacena las acciones concedidas en una lista de strings primitiva (`List<string> Permissions`).

---

## 🎨 Anatomía de las Entidades Core

### 1. Feature (El Catálogo de lo Posible)
Define qué pantallas o componentes abstractos existen en el software y qué acciones exponen.
*   `Key (string)`: Identificador único de la característica (ej. `"products"`). **Fuente de la verdad**.
*   `IsMenu (bool)`: Control de UI. Indica si el Frontend debe renderizar esta Feature en el menú lateral.
*   `Module (Enum)`: Agrupador de negocio (`Inventory`, `Sales`, `Core`).
*   `AvailableActions (JSON/List)`: Lista de acciones configuradas (ej. `[{Key: "read"}, {Key: "create"}]`).

### 2. RoleFeaturePermission (La Asignación Real)
Determina qué llaves específicas del catálogo han sido otorgadas a un Rol.
*   `FeatureKey (string)`: Llave foránea hacia la Feature.
*   `Permissions (List<string>)`: Colección de acciones asignadas (ej. `["read", "create"]`).

> ⚠️ **Regla de Validación del Backend:** El backend valida estrictamente que cualquier string guardado en la lista de `Permissions` exista previamente dentro del catálogo de `AvailableActions` de la Feature correspondiente.

---

## 📋 Reglas de Negocio para Control de Acceso (Matriz de Diseño)

Para evitar ambigüedades entre la visualización en la interfaz (Frontend) y la seguridad de los endpoints (Backend), el sistema opera bajo la siguiente matriz de comportamiento:

| Escenario | `IsMenu` | Permisos en Array | Comportamiento Frontend (UI) | Comportamiento Backend (API) |
| :--- | :---: | :---: | :--- | :--- |
| **Pantalla Estándar** | `true` | `["read", "create"]` | Muestra el acceso en el menú lateral. Permite ver la lista y el botón "Crear". | Permite peticiones `GET` (Lectura) y `POST` (Escritura). |
| **Acción sin Vista (Menor Privilegio)** | `true` ó `false` | `["create"]` *(No tiene `read`)* | **Oculta** la pantalla del menú lateral. Permite interactuar con la Feature (ej. crear un producto) **únicamente** como un modal/acción emergente desde otros flujos (ej. desde Compras). | Bloquea peticiones `GET` general (`403 Forbidden`). Permite peticiones `POST` para inserción. |
| **Sub-ruta o Vista Invisible** | `false` | `["read"]` | **Oculta** del menú lateral. Si el usuario navega directamente a la URL o accede desde un enlace interno, el Router lo deja pasar. | Permite peticiones `GET`. |
| **Sin Acceso** | Irrelevante | `[]` *(O no existe registro)* | Oculta el menú por completo. Si intenta forzar la URL, el Router del frontend lo expulsa al Home/Login. | Cualquier petición HTTP devuelve un estado estricto `403 Forbidden`. |

---

## ⚡ Estrategia de Rendimiento en el Login (Lazy Two-Queries)

El endpoint de Login implementa un patrón de **Fallo Rápido (Fail Fast)** dividiendo la consulta en dos transacciones independientes para proteger la base de datos contra el desperdicio de recursos y la explosión cartesiana.