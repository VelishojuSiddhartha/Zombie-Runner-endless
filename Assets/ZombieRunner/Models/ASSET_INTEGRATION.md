# Zombie Runner asset integration

The gameplay scripts stay on the prefab roots. Replace only the visual children when the final FBX files arrive.

## Player model

1. Import the character FBX under `Models/Player/`.
2. Drag the FBX model below the root of `Prefabs/Player/PlayerPlaceholder.prefab`, aligned so the feet meet the road and the model faces local +Z.
3. Remove or disable `Visual - Placeholder Capsule`; keep the root `CharacterController`, `PlayerController`, `PlayerHealth`, and `ShootingController` components.
4. Add or keep an `Animator` on the model hierarchy. Assign `Animations/Player_Animator.controller`, keep Apply Root Motion off, then assign the model root to `PlayerController.visualRoot` and its Animator to `PlayerController.animator`.
5. Add the provided animation FBXs to `Animations/` and assign their clips to the Run, Jump, Slide, Shoot, and Dead states in the player controller.

## Zombie models

Use `Prefabs/Zombies/NormalZombie.prefab`, `FastZombie.prefab`, and `BigZombie.prefab` as the gameplay roots. Replace each capsule visual with the zombie model while keeping `ZombieController` on the root. Set the model Animator to `Animations/Zombie_Animator.controller`, turn root motion off, and assign it to the controller's Animator field.

The zombie controller parameters are `Speed` (float), `Attacking` (bool), `Dead` (bool), and `Attack` (trigger). Assign animation clips to the Locomotion, Attack, and Dead states.

## Weapon and prop models

Keep the player root and `ShootingController`. Replace `Weapon Placeholder` with the weapon model as its child and assign the muzzle transform to the Shooting Controller's Muzzle field. Replace obstacle and collectible visuals inside their existing prefabs so their colliders and pickup/damage scripts remain on the roots.

## Environment models

Import models under `Models/Environment/`. Replace the visual children inside the existing environment segment prefabs. Keep each segment root and its configured length so `EndlessRoadManager` and `EnvironmentThemeManager` can continue recycling them.

The starter Animator Controllers contain the shared parameter names and placeholder states. No external or paid assets are included in this project.
