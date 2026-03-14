### 📜 核心 API 速查表

| 核心类 | 主要 API / 常用变量 | 类型 | 作用与描述 |
| :--- | :--- | :--- | :--- |
| **EventSystem** | `EventSystem.current` | 静态属性 | 获取当前场景的 EventSystem 单例 。 |
| | `RaycastAll(PointerEventData data, List<RaycastResult> result)` | 方法 | **核心方法**。用所有配置的射线检测器（如 GraphicRaycaster）进行射线检测，将命中结果按从近到远顺序存入 `result` 列表 。 |
| | `SetSelectedGameObject(GameObject selected)` | 方法 | 手动设置当前选中的 GameObject（例如让某个按钮获得焦点）。 |
| | `IsPointerOverGameObject([int pointerId])` | 方法 | 判断指定指针（默认鼠标左键）是否在 UI 元素上 。 |
| | `currentSelectedGameObject` | 变量 | 当前被选中的 GameObject 。 |
| | `firstSelectedGameObject` | 变量 | 一开始选中的 GameObject 。 |
| | `pixelDragThreshold` | 变量 | 区分点击和拖拽的像素阈值 。 |
| **PointerEventData** | `new PointerEventData(EventSystem.current)` | 构造函数 | 创建一个新的事件数据包，并关联到当前的 EventSystem 。 |
| | **`position`** | 变量 | **最常用**。当前指针位置（屏幕坐标），射线检测的起点 。 |
| | **`pressPosition`** | 变量 | 按下时的指针位置 。 |
| | **`button`** | 变量 | 哪个鼠标按键触发了事件（左键/右键/中键）。 |
| | **`clickCount`** | 变量 | 连续点击次数（用于区分单击/双击）。 |
| | **`pointerCurrentRaycast`** | 变量 | 当前事件关联的射线检测结果（RaycastResult）。 |
| | **`pointerPressRaycast`** | 变量 | 按下时关联的射线检测结果 。 |
| | **`pointerEnter`** / `pointerDrag` | 变量 | 鼠标进入的对象 / 正在拖拽的对象 。 |
| | `delta` | 变量 | 上次更新以来的指针移动增量 。 |
| | `scrollDelta` | 变量 | 鼠标滚轮的滚动量 。 |
| | `pressEventCamera` | 变量 | 与按下事件关联的摄像机（EventSystem 会自动帮你填）。 |
| | `IsPointerMoving()` | 方法 | 判断指针是否正在移动 。 |
| **RaycastResult** | `gameObject` | 变量 | **最常用**。被射线击中的 GameObject 。 |
| | `distance` | 变量 | 射线起点到命中点的距离 。 |
| | `depth` | 变量 | 被命中元素的深度（用于确定渲染顺序）。 |
| | `sortingLayer` / `sortingOrder` | 变量 | 被命中 UI 元素的排序层和顺序 。 |
| | `worldPosition` / `worldNormal` | 变量 | 命中点的世界坐标和法线 。 |
| | `module` | 变量 | 执行此次射线检测的模块（如 GraphicRaycaster）。 |
| | `isValid` | 变量 | 检测结果是否有效（是否存在关联模块和命中 GameObject）。 |
| | `Clear()` | 方法 | 重置结果 。 |

---

### 🧩 三者如何协同工作（结合你的代码）

你之前问的那段代码，正好展示了这三者的完整协作流程：

```csharp
// 1. 创建事件数据包（PointerEventData），并告诉它归哪个 EventSystem 管
//    相当于：创建一张空白的“事件表”，发证机关是 EventSystem.current
PointerEventData eventData = new PointerEventData(EventSystem.current);

// 2. 手动填表：把当前鼠标位置填进去
//    相当于：告诉 EventSystem "请用这个坐标去检测"
eventData.position = pointerPos; // pointerPos 通常是 Input.mousePosition

// 3. 准备一个容器，用来装检测结果
List<RaycastResult> results = new List<RaycastResult>();

// 4. 调用 EventSystem 的 RaycastAll 方法，执行检测
//    EventSystem 拿到 eventData 后：
//    - 从 eventData 里读取 position
//    - 找到合适的摄像机（eventData.pressEventCamera 或 Canvas 指定的摄像机）
//    - 发射射线，把所有命中的 UI 元素按顺序放进 results 列表
EventSystem.current.RaycastAll(eventData, results);

// 5. 遍历 results，处理命中的 UI 元素
foreach (RaycastResult result in results)
{
    Debug.Log("点到了：" + result.gameObject.name);
    // 可以进一步处理，比如获取 Button 组件并模拟点击
    Button btn = result.gameObject.GetComponent<Button>();
    if (btn != null)
    {
        btn.OnPointerClick(eventData); // 手动触发点击事件
    }
}
```

### 💡 总结

- **`EventSystem`** 是 **总指挥部**，负责组织射线检测、管理选中状态、分发事件 。
- **`PointerEventData`** 是 **数据包**，记录了一次交互的所有细节（位置、按键、点击次数等），需要手动创建和填充 。
- **`RaycastResult`** 是 **检测报告**，告诉你射线击中了哪个对象、距离多远、在哪个层级 。
