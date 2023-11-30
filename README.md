# MSharp

## 介绍

MSharp是一个基于C#的编译框架，用于将C#代码(子集)编译成《像素工厂》逻辑系统所支持的格式。

项目短期目标是实现一个这样的编译器，长期目标是实现游戏相关的调试功能(支持基础语句执行、游戏模拟、绘图模拟、多游戏处理器并发模拟)。

### 与其他项目的差异

MSharp与大部分小型自定义编译器项目的最大区别是，MSharp依赖于C#基础设施，代码编辑、智能提示、代码阅读、代码分析等功能都由现代化的ide提供，而后者大多使用较为简单的编辑器，只支持最基础的提示功能。

MSharp的优点也是其缺点，想要使用完善的功能就要安装ide，如vs等。

### 你不会在本项目中看到

1.编译原理，本项目只有语义分析，词法和语法均由Roslyn（C#编译API）完成。

2.复杂的代码优化，本人实力有限。

3.复杂的设计模式，本项目不会刻意引入任何设计模式，除非它们真的有价值。

4.无用的接口，本项目会尽可能避免使用接口，除非某些功能实现较复杂并且其功能明确而没有歧义。

### 你会在本项目中看到

1.语法树分析为主，运行时直接取值为辅。项目部分代码既在运行时被运行，又作为文本资源与用户代码一同参与编译。如注解、游戏常量取值。

2.C#高级用法，如反射、LINQ、dynamic以及各种方便的语法。

## 预期主流C#语法和功能支持程度

> 不支持的只是没有测试和特别设计的，可能某些标记为不支持的操作实际上是可用的。
>
> double* 表示游戏中的基本变量，使用double能存储的数据类型，如int bool(假定long也是)
>
> GO为游戏对象

| 模块         | 功能点         | 具体功能 | 支持程度                     | 示例           | 进度           |
| ------------ | -------------- | -------- | ---------------------------- | -------------- | ------------------ |
| 局部变量       | 名称   | *        | C#语法完全支持<br/>(@开头不支持,与游戏常量冲突) | double a=1     |                    |
|              | 类型 | 基础类型 | double*                 | double a;int c; |                    |
| | 类型 | 字符串 | 待定 |  | |
| | 类型 | 游戏常量 | 完全支持<br/>(非C#语法) | UnitConst | |
|              | 取值、赋值 | *        | 完全支持                     | a=1;b=a;a=b=1; |                    |
| 数值         | 字面量         | 基础类型 | double*                      | -1,1,0.12      |      |
|  | 字面量 | 字符串 | 待定 |  |  |
|  | 字面量 | 游戏常量 | 完全支持<br/>(非C#语法) | UnitConst.xxx |  |
|  | GO数据 | * | 面向对象式的读取 | unit.hp |  |
| 运算 | 基础运算 | +-*/等 | 完全支持 | a = a + b |  |
|  | 优先级 | () | 完全支持 | a*(b+c) |  |
|  | 条件语句 | 与或非等 | 基本支持<br/>不支持运算符短路 | a=b+c |  |
|  | 高级运算 | 自增自减 | 仅对局部变量 | a++,++a |  |
|  | 高级运算 | 三目运算符 | 完全不支持<br/>难以完成且收益不大 |  |  |
| 高级语句块 | if | if、else、else if | 完全支持 |  |  |
|              | for        | for               | 完全支持                                        |                 |                |
| | while | while | 完全支持 |  |  |
| 自定义数据类型 | 类 | | 完全不支持 |  |  |
|  | 结构体 | | 完全不支持 |  |  |
| 命名空间 |            |                   | 待定                                            |  |  |
| 注释 | * | * | 完全支持 |  |  |
| 游戏控制 | 单位控制 |  | 完全支持 | | |
|  | 建筑控制 |  | 完全支持 | | |
|  | 存储控制 |  | 完全支持 | | |
|  | 显示控制 |  | 完全支持 |  |  |
|  | | | | | |
|  |            |                   |                                                 |  |  |
|  |            |                   |                                                 |  |  |
| 函数         | 返回值         |          | double*                      |                |                    |
|              | 方法体         |          | 基本支持                     |                |        |
|              | 普通参数       |          | 完全支持                     |                |                    |
|              | out等参数      |          | 基本不支持（仅game api支持） |                |                    |
|              | 默认参数       |          | 不支持                       |                |                    |
| 函数调用     |                |          | 内联、堆栈                   |                |                    |

## 预期调试支持程度

| 模块     | 功能点           | 具体功能 | 支持程度 | 示例              | 进度 |
| -------- | ---------------- | -------- | -------- | ----------------- | ---- |
| 基础语句 | 游戏无关语句执行 | *        | 完全支持 | int a=b;<br/>b=c; |      |
| 游戏控制 | 待定             | 待定     | 待定     | 待定              | 待定 |
| 绘图控制 | 游戏无关绘图执行 | *        | 待定     | 待定              | 待定 |
|          | 游戏相关绘图执行 | *        | 待定     | 待定              | 待定 |











# 进度

## 游戏代码创建
### 中间代码生成

#### 游戏操作

| 模块     | 功能点               | 状态             | 备注 |
| -------- | -------------------- | ---------------- | ---- |
| 状态获取 | 面向对象风格的值获取 | 基本完成         |      |
| 控制     | 单位控制             | 基本完成、未测试 |      |
|          | 建筑控制             | 基本完成、未测试 |      |
|          | 存储控制             | 基本完成、未测试 |      |
|          | 显示控制             | 基本完成、未测试 |      |



TODO



#### 基础语句
| 模块         | 功能点                         | 状态             | 备注                       |
| ------------ | ------------------------------ | ---------------- | -------------------------- |
| 变量定义     | 局部变量定义                   | 基本完成         |                            |
|              | 类成员对象定义（关联游戏物体） | 基本完成，待测试 | 待优化，给类添加变量表     |
| 基础运算     | + - * / 等                     | 基本完成，待测试 |                            |
| 自增自减运算 | 前++ --                        | 基本完成，待测试 |                            |
|              | 后++ --                        | 基本完成，待测试 |                            |
| 内联函数调用 | 调用                           | 功能早期         | 已有设计存在问题，需要重写 |

#### 复杂结构
| 模块      | 功能点       | 状态               | 备注 |
| --------- | ------------ | ------------------ | ---- |
| if语句    | 条件判定     | 基本完成，待测试   |      |
|           | 跳转         | 基本完成，待测试   |      |
|           | 语句体       | 基本完成，待测试   |      |
| goto      | goto和标签   | 暂不解决           |      |
| for循环   | 变量初始化   | 基本完成，待测试   |      |
|           | 条件判定     | 基本完成，待测试   |      |
|           | 循环后操作   | 基本完成，待测试   |      |
|           | 语句体       | 基本完成，待测试   |      |
|           | 跳转（困难） | 可能差不多，待测试 |      |
| while循环 | 条件判定     | 基本完成，待测试   |      |
|           | 跳转         | 可能差不多，待测试 |      |
|           | 语句体       | 基本完成，待测试   |      |

#### 函数调用

#### 类分析

| 模块   | 功能点     | 状态       | 备注       |
| ------ | ---------- | ---------- | ---------- |
| 类分析 | 类变量分析 | 未开始     |            |
|        | 方法分析   | 见方法分析 | 类参数限制 |
|        | 忽略无关类 | 完成       |            |

#### 函数分析

| 模块     | 功能点       | 状态       | 备注       |
| -------- | ------------ | ---------- | ---------- |
| 方法分析 | 方法变量分析   | 未开始     |            |
|          | 方法体分析   | 见语句分析 | 类参数限制 |
|          | 忽略无关方法 | 完成       |            |

TODO

## 代码优化

## 代码执行预览
## 代码质量
### ExpressionHandle优化

参数结构化
文件结构优化，只暴露ExpressionHandle

### links处理优化

