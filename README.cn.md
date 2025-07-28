[English README](./README.md)

# Unity-DynamicAtlas

![](Image~/LOGO.png)

Unity动态图集解决方案

其中矩形打包算法基于[GitHub - villekoskelaorg/RectanglePacking: Super fast AS3 implementation of rectangle packing algorithm](https://github.com/villekoskelaorg/RectanglePacking.git) 做了些许扩展，在此鸣谢@[villekoskelaorg (Ville Koskela) · GitHub](https://github.com/villekoskelaorg)

![01](Image~/01.png)

## 实现的特性

- 图集压缩纹理

- 异步加载资源

- 图集内部引用计数，自动释放引用为0的区域

- 大图自动Fallback为散图引用，可自定义触发阈值

- 支持传入自定义资源加载方法

## 使用限制

- 不支持OpenGL ES2.0

- 散图和图集的纹理压缩格式必须一致

- 散图不能为静态图集成员
