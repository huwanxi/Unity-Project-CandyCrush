明白了！这次我精准聚焦于 **`OnCollisionEnter` 的参数**，也就是 `Collision` 和 `Collision2D` 类，把它们的**所有常用变量**掰开揉碎了讲清楚。同时把 Physics/Physics2D 也分开详细列出。

---

## 📦 一、碰撞事件参数：`Collision` (3D) 与 `Collision2D` (2D)

当碰撞发生时，Unity 通过这个参数把**本次碰撞的所有细节**传递给你。

### 🧊 3D Collision (用于 `OnCollisionEnter(Collision collision)`)

| 变量名 | 类型 | 作用与描述 | 示例 |
|:---|:---|:---|:---|
| `gameObject` | `GameObject` | 撞到的那**整个物体** | `collision.gameObject.SetActive(false);` |
| `transform` | `Transform` | 撞到物体的 Transform 组件 | `collision.transform.position` |
| `rigidbody` | `Rigidbody` | 撞到物体的 Rigidbody 组件（如果没有则为 null） | `collision.rigidbody.AddForce(Vector3.up);` |
| `collider` | `Collider` | 撞到的**具体碰撞体**（一个物体可能有多个碰撞体） | `collision.collider.enabled = false;` |
| `contacts` | `ContactPoint[]` | **所有接触点**的数组（重要！） | `foreach (var contact in collision.contacts) { /* 处理每个接触点 */ }` |
| `contactCount` | `int` | 本次碰撞的接触点数量 | `if (collision.contactCount > 0) { ... }` |
| `impulse` | `Vector3` | 本次碰撞施加的总冲量 | 用于计算碰撞力度 |
| `relativeVelocity` | `Vector3` | 两个碰撞物体的**相对速度** | `float impactForce = collision.relativeVelocity.magnitude;` |
| `transform` | `Transform` | （同上） | 方便链式调用 |

#### 🔍 接触点详解 (`ContactPoint`)
```csharp
void OnCollisionEnter(Collision collision)
{
    // 获取第一个接触点
    ContactPoint contact = collision.contacts[0];
    
    Vector3 point = contact.point;          // 碰撞发生的位置（世界坐标）
    Vector3 normal = contact.normal;        // 碰撞点的法线方向（比如地面的朝上方向）
    Collider thisCollider = contact.thisCollider;     // 我自己的哪个碰撞体撞到的
    Collider otherCollider = contact.otherCollider;   // 对方的哪个碰撞体
    float separation = contact.separation;   // 穿透深度（一般调试用）
}
```

---

### 🧩 2D Collision2D (用于 `OnCollisionEnter2D(Collision2D collision)`)

| 变量名 | 类型 | 作用与描述 | 示例 |
|:---|:---|:---|:---|
| `gameObject` | `GameObject` | 撞到的整个物体 | `collision.gameObject.tag` |
| `transform` | `Transform` | 撞到物体的 Transform | `collision.transform.localScale` |
| `rigidbody` | `Rigidbody2D` | 撞到物体的 Rigidbody2D | `collision.rigidbody.velocity = Vector2.zero;` |
| `collider` | `Collider2D` | 撞到的具体碰撞体 | `collision.collider.isTrigger = true;` |
| `contacts` | `ContactPoint2D[]` | **所有接触点**的数组 | 见下方详细拆解 |
| `contactCount` | `int` | 接触点数量 | `for (int i = 0; i < collision.contactCount; i++)` |
| `relativeVelocity` | `Vector2` | 相对速度 | `if (collision.relativeVelocity.magnitude > 10) 播放撞击音效` |
| `enabled` | `bool` | 这个碰撞事件是否启用 | 极少用 |
| `otherRigidbody` | `Rigidbody2D` | 对方的 Rigidbody2D（等价于 `rigidbody`） | 同 `rigidbody` |
| `otherCollider` | `Collider2D` | 对方的 Collider2D（等价于 `collider`） | 同 `collider` |

#### 🔍 2D 接触点详解 (`ContactPoint2D`)
```csharp
void OnCollisionEnter2D(Collision2D collision)
{
    // 获取第一个接触点
    ContactPoint2D contact = collision.GetContact(0); // 推荐用 GetContact 方法
    
    Vector2 point = contact.point;              // 碰撞点（世界坐标）
    Vector2 normal = contact.normal;            // 法线方向
    Vector2 relativeVelocity = contact.relativeVelocity; // 该点的相对速度
    Collider2D thisCollider = contact.collider;        // 我的碰撞体
    Collider2D otherCollider = contact.otherCollider;  // 对方碰撞体
    float separation = contact.separation;       // 分离距离（负值表示穿透）
    
    // 另一种获取接触点的方式（直接访问数组）
    // ContactPoint2D contact = collision.contacts[0];
}
```

---

## ⚡ 二、Physics (3D) 与 Physics2D 静态类 - 详细参数版

### 🏗️ Physics 3D (完整热门 API 及参数)

| 方法名 | 完整签名 | 参数详解 | 返回值 | 作用 |
|:---|:---|:---|:---|:---|
| **Raycast** | `bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)` | `origin`: 起点<br>`direction`: 方向<br>`hitInfo`: 碰撞结果（out）<br>`maxDistance`: 最大距离<br>`layerMask`: 层级过滤 | `bool` | 发射射线，返回第一个击中的物体 |
| **RaycastAll** | `RaycastHit[] RaycastAll(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)` | 同上（无 `out`） | `RaycastHit[]` | 返回所有击中的物体 |
| **RaycastNonAlloc** | `int RaycastNonAlloc(Vector3 origin, Vector3 direction, RaycastHit[] results, float maxDistance, int layerMask)` | `results`: 缓存数组 | `int` (实际击中数量) | 无GC版本，推荐使用 |
| **OverlapSphere** | `Collider[] OverlapSphere(Vector3 position, float radius, int layerMask)` | `position`: 球心<br>`radius`: 半径 | `Collider[]` | 检测球形区域内的所有碰撞体 |
| **OverlapSphereNonAlloc** | `int OverlapSphereNonAlloc(Vector3 position, float radius, Collider[] results, int layerMask)` | `results`: 缓存数组 | `int` | 无GC版球形检测 |
| **OverlapBox** | `Collider[] OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask)` | `halfExtents`: 半长宽高（size的一半）<br>`orientation`: 旋转 | `Collider[]` | 检测盒形区域 |
| **CheckSphere** | `bool CheckSphere(Vector3 position, float radius, int layerMask)` | 同 `OverlapSphere` 但只返回是否检测到 | `bool` | 快速检查球形范围内是否有物体 |
| **Linecast** | `bool Linecast(Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask)` | `start`: 起点<br>`end`: 终点 | `bool` | 两点之间连线检测 |

---

### 🎮 Physics2D (完整热门 API 及参数)

| 方法名 | 完整签名 | 参数详解 | 返回值 | 作用 |
|:---|:---|:---|:---|:---|
| **Raycast** | `RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float distance, int layerMask)` | `origin`: 起点<br>`direction`: 方向<br>`distance`: 距离<br>`layerMask`: 层级过滤 | `RaycastHit2D` | 发射2D射线 |
| **RaycastAll** | `RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int layerMask)` | 同上 | `RaycastHit2D[]` | 返回所有击中物体 |
| **RaycastNonAlloc** | `int RaycastNonAlloc(Vector2 origin, Vector2 direction, RaycastHit2D[] results, float distance, int layerMask)` | `results`: 缓存数组 | `int` | 无GC版 |
| **OverlapCircle** | `Collider2D OverlapCircle(Vector2 point, float radius, int layerMask)` | `point`: 圆心<br>`radius`: 半径 | `Collider2D` | 检测圆形区域，返回第一个 |
| **OverlapCircleAll** | `Collider2D[] OverlapCircleAll(Vector2 point, float radius, int layerMask)` | 同上 | `Collider2D[]` | 返回圆形区域内所有碰撞体 |
| **OverlapCircleNonAlloc** | `int OverlapCircleNonAlloc(Vector2 point, float radius, Collider2D[] results, int layerMask)` | `results`: 缓存数组 | `int` | 无GC版 |
| **OverlapBox** | `Collider2D[] OverlapBox(Vector2 point, Vector2 size, float angle, int layerMask)` | `size`: 矩形大小<br>`angle`: 旋转角度 | `Collider2D[]` | 检测矩形区域 |
| **OverlapArea** | `Collider2D[] OverlapArea(Vector2 pointA, Vector2 pointB, int layerMask)` | `pointA`: 左下角<br>`pointB`: 右上角 | `Collider2D[]` | 用两个点定义矩形检测 |
| **OverlapPoint** | `Collider2D OverlapPoint(Vector2 point, int layerMask)` | `point`: 检测点 | `Collider2D` | 检测某个点是否有碰撞体 |
| **Linecast** | `RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask)` | `start`: 起点<br>`end`: 终点 | `RaycastHit2D` | 两点之间连线检测 |
| **CircleCast** | `RaycastHit2D CircleCast(Vector2 origin, float radius, Vector2 direction, float distance, int layerMask)` | `radius`: 圆的半径 | `RaycastHit2D` | 将圆沿着方向投射 |
| **BoxCast** | `RaycastHit2D[] BoxCastAll(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance, int layerMask)` | `size`: 盒子大小<br>`angle`: 旋转 | `RaycastHit2D[]` | 将矩形沿着方向投射 |

---

### 📊 2D 特有的返回值类型：`RaycastHit2D`

| 变量 | 类型 | 作用 |
|:---|:---|:---|
| `collider` | `Collider2D` | 击中的碰撞体 |
| `rigidbody` | `Rigidbody2D` | 击中的刚体 |
| `transform` | `Transform` | 击中的 Transform |
| `point` | `Vector2` | 击中点的坐标 |
| `normal` | `Vector2` | 击中点的法线 |
| `distance` | `float` | 从起点到击中点的距离 |
| `fraction` | `float` | 从起点到击中点的比例（0-1） |
| `centroid` | `Vector2` | 所有击中点的中心（用于多点命中） |

---

### 💡 总结

| 场景 | 用哪个 | 关键变量 |
|:---|:---|:---|
| 碰撞时获取对方物体 | `collision.gameObject` | 整个物体 |
| 碰撞时获取对方的具体哪个碰撞体 | `collision.collider` | 单个碰撞体组件 |
| 获取碰撞点位置 | `collision.contacts[0].point` | 第一个接触点的坐标 |
| 获取碰撞力度 | `collision.relativeVelocity.magnitude` | 相对速度大小 |
| 检测圆形范围内敌人 | `Physics2D.OverlapCircleAll` | 返回 `Collider2D[]` |
| 性能优化版检测 | `...NonAlloc` | 传入缓存数组，返回 `int` |

你现在是要写一个根据碰撞力度播放音效的功能，还是要检测范围内所有敌人？告诉我具体场景，我帮你写完整代码。