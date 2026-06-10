# Modular Stage Art Generation

## Why split assets

Do not generate each stage as one giant illustration. Keep the pieces separate so stage edits stay cheap:

- `sky`: one wide opaque tileable layer. No transparency needed.
- `far`: distant silhouettes or landmarks. Usually opaque/tileable, sometimes transparent.
- `mid`: reusable stage identity parts, such as trees, mountains, crystals, towers, mushrooms.
- `near`: foreground silhouettes, grasses, fences, rocks. Generate on chroma key and remove the background.
- `Props/Far`, `Props/Mid`, `Props/Near`: small transparent parts for procedural parallax placement.
- enemies and UI cut-ins: isolated sprites under `Assets/Resources`.

Runtime loading is automatic. If a sprite exists in `Assets/Resources/StageArt/StageX`, the stage scene will use it. Missing files are ignored.

## Runtime folder layout

```text
Assets/Resources/StageArt/Stage1/sky.png
Assets/Resources/StageArt/Stage1/far.png
Assets/Resources/StageArt/Stage1/mid.png
Assets/Resources/StageArt/Stage1/near.png
Assets/Resources/StageArt/Stage1/Props/Far/*.png
Assets/Resources/StageArt/Stage1/Props/Mid/*.png
Assets/Resources/StageArt/Stage1/Props/Near/*.png

Assets/Resources/Enemies/Batkin.png
Assets/Resources/Enemies/Togemaru.png
Assets/Resources/Enemies/Pettan.png
Assets/Resources/Enemies/Kabuton.png
Assets/Resources/Enemies/KingSlime.png
Assets/Resources/UI/SpecialCutin.png
```

## Common style suffix

```text
high-resolution 2D pixel art for a side-scrolling platformer game, 16-bit inspired but modern and detailed, vibrant colors, strong readable silhouettes, clean edges, no text, no watermark, no characters in background
```

## Chroma-key rule

Use chroma key for anything that should be independently placed.

```text
Create the requested game asset on a perfectly flat solid #ff00ff chroma-key background for background removal.
The background must be one uniform color with no shadows, gradients, texture, reflections, floor plane, or lighting variation.
Keep the subject fully separated from the background with crisp edges and generous padding.
Do not use #ff00ff anywhere in the subject.
No cast shadow, no contact shadow, no watermark, and no text.
```

Use `#00ff00` only when the asset has no green/cyan content. For forests, frogs, crystals, and green hero effects, prefer `#ff00ff`.

## Stage part prompts

### Stage1: morning grassland

- `sky`: Bright early morning sky over rolling green hills, soft gradient from warm pale yellow horizon to clear cerulean blue, a few fluffy cumulus clouds catching golden morning light from upper left, gentle volumetric god rays, peaceful and fresh mood, seamless horizontally tileable, 2048x1024.
- `far`: Soft distant meadow hills and tiny round tree silhouettes, pale morning haze between layers, upper-left sunlight, very low contrast, seamless horizontally tileable, 2048x1024.
- `mid props`: Round meadow tree, birch trunk cluster, small windmill silhouette, distant grassy mound. Generate each as a separate transparent PNG.
- `near props`: Tall grass clump, wildflower patch, foreground birch trunk, curved leaf silhouette. Generate each as a separate transparent PNG.

### Stage2: sunset thorn highland

- `sky`: Dramatic sunset sky in burning orange, coral pink and magenta gradients, huge low sun half-hidden behind jagged mountain silhouettes, streaky backlit clouds, cinematic warm light, seamless horizontally tileable, 2048x1024.
- `far`: Jagged sharp mountain ridges in deep purple silhouette against sunset, multiple overlapping ridge layers, thin golden rim light tracing every mountain edge, seamless horizontally tileable, 2048x1024.
- `mid props`: Sharp purple rock spire, thorn bush, cracked plateau chunk, dry twisted tree. Generate each as a separate transparent PNG.
- `near props`: Black-violet thorn silhouette, spiky rock foreground, dead branch arch, warning cactus-like thorn cluster. Generate each as a separate transparent PNG.

### Stage3: slime forest

- `sky/far`: Deep ancient forest interior, huge mossy tree trunks fading into teal-green atmospheric mist, dramatic god rays through canopy, floating pollen and fireflies, seamless horizontally tileable, 2048x1024.
- `mid props`: Giant tree trunk, oversized red mushroom, oversized teal mushroom, hanging vine curtain, mossy log. Generate each as a separate transparent PNG.
- `near props`: Large backlit leaf cluster, fern clump, dark foreground root, glowing pollen patch. Generate each as a separate transparent PNG.

### Stage4: light cave

- `sky/far`: Vast dark cavern interior, near-black deep blue darkness, distant magma river casting warm orange glow from below, cyan crystal clusters as cold accent lights, fog catching the underglow, seamless horizontally tileable, 2048x1024.
- `mid props`: Massive rock pillar, stalactite cluster, stalagmite cluster, cyan crystal vein, orange-lit cave wall chunk. Generate each as a separate transparent PNG.
- `near props`: Pure black cave rock silhouette, hanging stalactite silhouette, magma rim rock, small glowing crystal cluster. Generate each as a separate transparent PNG.

### Stage5: king slime castle

- `sky`: Ominous storm night sky in deep violet and indigo, huge dark fantasy castle silhouette on a cliff, sickly green glowing windows, forked lightning illuminating cloud edges, full moon behind fast clouds, seamless horizontally tileable, 2048x1024.
- `far`: Castle outer walls, broken towers and sharp spires in dark purple-black silhouette, torn banners, cold lightning rim light, green torch flames, seamless horizontally tileable, 2048x1024.
- `mid props`: Broken tower, castle wall segment, green-flame torch, torn banner, cracked stone arch. Generate each as a separate transparent PNG.
- `near props`: Wrought-iron fence spikes, small gargoyle statue, dead tree silhouette, foreground stone rubble. Generate each as a separate transparent PNG.

## Enemy prompts

Generate enemies as isolated transparent sprites, or use chroma key first and remove it.

- `Batkin`: Round chubby purple bat creature with huge ears, tiny fangs, cape-like wings spread mid-flap, sleepy mischievous eyes, soft cyan rim light as if lit by glowing cave crystals, single game sprite, side view facing right, full body centered, 512x512.
- `Togemaru`: Spiky hedgehog ball creature covered in sharp dark-violet spikes, small worried cute face peeking from the front, spikes catching orange sunset rim light, clearly dangerous-to-touch silhouette, single game sprite, side view facing right, full body centered, 512x512.
- `Pettan`: Big-mouthed green frog creature sitting flat on the ground, cheeks inflated about to spit a glowing bubble, leaf-pattern back, dappled forest light on its skin, single game sprite, side view facing right, full body centered, 512x512.
- `Kabuton`: Small beetle creature with a huge polished metallic shield-like horn covering its front, soft unprotected round body behind, front armor reflecting bright specular light while the back stays soft and matte, single game sprite, side view facing right, full body centered, 512x512.
- `KingSlime`: Giant majestic slime king boss, translucent purple jelly body with darker core visible inside, golden crown tilted on top, angry glowing eyes, small slimes trapped inside its body, dramatic purple backlight and green castle light from below, imposing but slightly goofy, single game sprite, side view facing right, full body centered, 1024x1024.
