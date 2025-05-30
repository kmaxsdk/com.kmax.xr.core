﻿# 更新日志

所有的改动都记录在这里  
[Chanagelog English](CHANGELOG.en.md)

## [2.3.11] - 2025-04-14

### Added

- 普通PC兼容

## [2.3.1] - 2025-02-25

### Added

- Linux 支持

### Fixed

- 使用线性空间导致的色彩问题

## [2.2.4] - 2024-10-25

### Added

- 射线输入支持按层级排序
- 射线笔独立的拖拽阈值

## [2.2.3] - 2024-10-15

### Added

- 只作用于射线笔的拖动阈值

### Fixed

- 射线笔在临界状态抓取物体导致反转的问题

## [2.2.2] - 2024-10-11

### Added

- 全局笔按键状态和模拟摇杆
- 射线笔控制相机视角示例
- 立体显示预览开关

### Changed

- 完善平台兼容性
- 射线端点自动适应不同的 ViewScale
- 眼部位置回归动画修改
- 修改示例 Demo

### Fixed

- 修复 WebGL 多场景切换的问题

### Removed

- 旧的 kcore 模块

## [2.1.10] - 2024-08-30

### Added

- 射线输入平滑效果

### Changed

- 移除c++运行时依赖
- 完善射线输入及显示
- Tracker 调试信息显示

## [2.1.0] - 2024-05-24

### Added

- 支持帧序列

## [2.0.1] - 2024-05-14

### Changed

- WebGL 默认为常规显示模式

### Fixed

- 解决K系列设备触摸不准的问题

## [2.0.0] - 2024-03-15

### Added

- 支持Kmax K系列设备
- 支持WebGL

## [1.3.1] - 2023-11-09

### Added

- 支持串流调试

## [1.2.5] - 2023-10-27

### Added

- 默认开启高刷新率
- 开放开启/禁用追踪接口

## [1.2.3] - 2023-09-18

### Added

- MacOS开发支持

## [1.2.1] - 2023-09-01

### Added

- 新增右键快捷菜单

## [1.2.0] - 2023-08-23

### Changed

- 改为unity包管理的形式

### Fixed

- 编辑器下默认显示相机聚焦后的画面

## [1.1.2] - 2023-08-17

### Changed

- 编辑器下默认为常规显示模式，在xrrig检视面板可切换为左右格式

## [1.1.0] - 2023-08-08

### Added

- 定位笔震动接口
- 添加两个示例，动态加载xrrig示例，zcore输入模块示例
- 添加kcore输入模块兼容zcore输入
