[中文版 README](./README.cn.md)

# Unity-DynamicAtlas

![](Image~/LOGO.png)

A dynamic atlas solution for Unity.

The rectangle packing algorithm is based on [GitHub - villekoskelaorg/RectanglePacking: Super fast AS3 implementation of rectangle packing algorithm](https://github.com/villekoskelaorg/RectanglePacking.git) with some extensions. Special thanks to @[villekoskelaorg (Ville Koskela) · GitHub](https://github.com/villekoskelaorg).

![01](Image~/01.png)

## Features

- Atlas texture compression
- Asynchronous resource loading
- Internal reference counting for atlas, automatically releases regions with zero references
- Large images automatically fallback to loose texture reference, with customizable threshold
- Supports custom resource loading methods

## Limitations

- OpenGL ES2.0 is not supported
- The texture compression format of loose textures and atlases must be consistent
- Loose textures cannot be static atlas members 

## Usage Example

You can quickly get a reference from the `Init.cs` script in the Demo scene.

1. Call the initialization method DynamicAtlasManager.Init in your external code to initialize various parameters required for the dynamic atlas. It is recommended to call it in your initialization logic, such as Awake().

2. Use the `DynamicImage` component instead of the `UnityEngine.UI.Image` component. You can directly mount images statically, or dynamically load images through the `DynamicImage.SetDynamicSprite(string spriteName)` method.
   
   ![](Image~/2025-07-29-16-22-26-image.png)

3. Enjoy! 