# Implementation Tasks

## Task List

### 1. Create Description Directory
**Directory**: `SuperDucky.Connector/assets/description/`

**Steps**:
1. Create the `description` folder inside `assets`
2. Verify directory was created successfully

**Validation**:
- Directory exists at `SuperDucky.Connector/assets/description/`

---

### 2. Create English Description File
**File**: `SuperDucky.Connector/assets/description/en.md`

**Steps**:
1. Create the file with UTF-8 encoding
2. Write English description focusing on game features:
   - What the mod does: Exports all game items and tracks your collection
   - Item encyclopedia: Automatically exports item names, descriptions, and icons
   - Acquisition monitoring: Tracks items you collect in real-time
   - Per-save tracking: Separate data for each save slot
   - How it works: Runs automatically when game loads, saves to temp directory
3. Keep it player-friendly, avoid technical implementation details
4. Keep it concise and easy to understand

**Validation**:
- File exists at correct path
- Content is valid Markdown
- Focuses on game features, not technical details

---

### 3. Create Chinese Description File
**File**: `SuperDucky.Connector/assets/description/zh.md`

**Steps**:
1. Create the file with UTF-8 encoding
2. Write Chinese description focusing on game features:
   - 模块功能：导出所有游戏物品并追踪收集进度
   - 物品百科：自动导出物品名称、描述和图标
   - 获取监控：实时追踪收集的物品
   - 存档分离：每个存档独立保存数据
   - 工作方式：游戏加载时自动运行，保存到临时目录
3. Keep it player-friendly, avoid technical implementation details
4. Ensure translation matches English content

**Validation**:
- File exists at correct path
- Content is valid Markdown
- Chinese translation is natural and accurate
- Focuses on game features, not technical details

---

### 4. Verify Naming Conventions
**Files**: `en.md`, `zh.md`

**Steps**:
1. Confirm both filenames use only lowercase letters
2. Confirm English file is named `en.md`
3. Confirm Chinese file is named `zh.md`
4. Verify no spaces or special characters in filenames

**Validation**:
- Filenames match convention exactly

---

### 5. Cross-Validate Content
**Files**: `en.md`, `zh.md`

**Steps**:
1. Verify both files describe the same game features
2. Check that content is player-friendly and not overly technical
3. Ensure both language versions are consistent

**Validation**:
- Content focuses on game features
- No unnecessary technical details
- Both files describe equivalent functionality

---

### 6. Final Validation
**Command**: `openspec validate module-description-files-arg-value --strict`

**Steps**:
1. Run OpenSpec validation
2. Resolve any validation errors

**Validation**:
- Validation passes with no errors

## Dependencies

```
Task 1 (Create Directory) ──────────────────────────────┐
                                                       ├──► Task 4 (Verify Naming)
Task 2 (Create English) ────────────────────────────────┤
                                                       ├──► Task 5 (Cross-Validate)
Task 3 (Create Chinese) ────────────────────────────────┤
                                                       │         │
Task 1 must complete before Tasks 2 and 3 ─────────────┘         │
                                                                 ├──► Task 6 (Final Validation)
Tasks 2, 3, 4 must complete before Task 5 ───────────────────────┘
```

**Notes**:
- Task 1 must complete first (directory must exist)
- Tasks 2 and 3 can be done in parallel after Task 1
- Task 4 requires Tasks 1, 2, 3 to complete
- Task 5 requires Tasks 2, 3, 4 to complete
- Task 6 requires all previous tasks to complete

## Acceptance Criteria

All tasks are complete when:
1. [x] `assets/description/` directory exists
2. [x] `assets/description/en.md` exists
3. [x] `assets/description/zh.md` exists
4. [x] Both files use lowercase naming (`en.md` and `zh.md`)
5. [x] Content focuses on game features (item encyclopedia, collection tracking)
6. [x] Descriptions are player-friendly and easy to understand
7. [x] Files are valid Markdown with UTF-8 encoding
8. [x] `openspec validate --strict` passes without errors
