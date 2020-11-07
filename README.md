**The University of Melbourne**
# COMP30019 – Graphics and Interaction

# Project-2 README

## Table of contents
- [COMP30019 – Graphics and Interaction](#comp30019--graphics-and-interaction)
- [Project-2 README](#project-2-readme)
  - [Table of contents](#table-of-contents)
  - [Team Members & Contributions](#team-members--contributions)
  - [Explanation of the game](#explanation-of-the-game)
  - [Technologies](#technologies)
  - [User Interface](#user-interface)
  - [Player Control](#player-control)
  - [Graphics Pipeline & Camera Motion](#graphics-pipeline--camera-motion)
    - [Graphics Pipeline](#graphics-pipeline)
    - [Particle Effects](#particle-effects)
    - [Shaders](#shaders)
    - [Camera](#camera)
  - [Models & Entities](#models--entities)
    - [Player Model & Sound](#player-model--sound)
    - [Enemy Models](#enemy-models)
    - [Level Design & Objects](#level-design--objects)
  - [Querying & Observational Methods](#querying--observational-methods)
    - [Participants](#participants)
    - [Methodology](#methodology)
    - [Feedback](#feedback)
    - [Changes made from feedback](#changes-made-from-feedback)
  - [External Code/APIs used](#external-codeapis-used)

## Team Members & Contributions

| Name | Task | State |
| :---         |     :---:      |          ---: |
| Jiawei(Derrick) Li  | Player Interactions, Animations, Level Design, Lighting, Shader     |  Done |
| Rushith Karunaratne    | Particle effects, level design, animations, weapon system     |  Done |
| Joshua Bugeja    | Enemy AI/behaviour, level design, testing evaluation     |  Done |

## Explanation of the game
Our game is a first person shooter (FPS) inspired by the beautiful Titanfall 2 movement mechanics.  
In the game, you are a reserve pilot of The 6-4 ready to fight for freedom in the great Frontier. However, the three things, the holy trinity of the identity of a pilot was stole from you. The objects are scattered around heavily guarded enemy territory, it is your mission, and the final trial for becoming a true Titan pilot...

## Technologies
Project is created with:
* Unity 2019.4.3f1
* Blender 2.90.0

## User Interface
Controls are suitable for the genre of the game. With conventional movement keys such as "WASD", "Space" for jump and "C" for crouch, as well as intuitive weapon control like "R" for reload, left mouse for fire and right mouse for aim down sight. User interface is simple and easy to grasp, with custom animation added for better transistion. Even more, there is dedicated options menu at the beginning and during the game for players to adjust settings according to their preferences.

## Player Control 
Player movement is a key feature of our game, since a variety of ways for the player to move around the map are introduced, such as sprinting, wallrun and crouch sliding. The most exciting part is wallrunning without doubt, it is also the hardest part to implement since checking when to wallrun is a rather hideous process.
```c#
public class PlayerControl : MonoBehaviour
{
    ...
    // check a bunch of requirement for initaiting a wall run: surface normal angle, if grounded, if input, if the same wall is ran again
        if (Mathf.Abs(Vector3.Dot(collisionSurfaceNorm, Vector3.up)) < 0.5f && !grounded && (horizontal != 0 || forward != 0) 
            && ((collision.gameObject.GetInstanceID() != lastCollided) || Time.time - exitTime > wallRunCD))
        {
            startTime = Time.time;
            isWallRunning = true;
        }
        else
        {
            isWallRunning = false;
        }
    ...
}
```
The code above gives us a slight glipse on the conditions to initiate a wallrun. The approach we took is to check whether the impact surface is indeed a vertical surface by calculating the dot product of the surface normal vector and the up direction vector. Then, a ground check(done in other parts of the code), also we need to make sure that player has input, that they are willing to start a wallrun. As well as checking whether the player is trying to re-jump back on to the same surface they just left, to make sure they don't just wallrun infinitely.  

## Graphics Pipeline & Camera Motion
### Graphics Pipeline
A number of adjustments were made to our graphics pipeline in order to increase our game's performance.

While Unity automatically culls objects outside of the camera's vision, in order to reduce the number of tris/verts in view, we also included occlusion culling. Occlusion culling meant that objects that blocked from the camera's view by other objects were not rendered. All non-changing objects were marked as 'static' and given occlusion culling, signifcantly increasing performance in the game.

### Particle Effects
The implemented particle effects are visible in many sections of the gameplay. The player muzzle flash and enemy muzzle flash are both implemented using the unity particle effect generation system and instantiated in a script call. The other particle effects (bllod splatter, fire, water etc.) were all imported using the unity particle effects library.

### Shaders
Two custom shaders are used for better visual and help player understand their surroundings.

**Hologram Shader**  
The hologram shader is implemented by multiplying the original texture by a hologram colour,overlaid with some strips for some hologram vibe, and set the shader mode to transparent shading for a see-through effect. Rim lighting is also added for a better visual representation. Rim formula is from the Unity maual for custom shader.
``` c
//Alpha mask coordinates
o.grabPos = UnityObjectToViewPos(i.vertex);

//Scroll Alpha mask uv
o.grabPos.y += _Time * _ScrollSpeedV;
```
One thing worth mentioning is that, the alpha mask(strips) coordinates are converted to view perspective, which means only model and view projection matrices are being multiplied to the vertex. That is because we want the hologram to have a consistant scrolling direction(top to bottom) with respect to the player's viewing position.  
**Chromatic Aberration**  
This a simple shader that introduces chromatic aberration. The idea is really straight-forward, take the red and blue colour chanels from the texture, shift their X and Y UV a bit so that they are mapped slightly off. This effect is applied to the camera so it is enabled globally. It is activate when player steps into the reactor/power core room of each level to represent a radiation effect.
``` c
float colR = tex2D(_MainTex, float2(i.uv.x - _Amount, i.uv.y - _Amount)).r;
float colG = tex2D(_MainTex, i.uv).g;
float colB = tex2D(_MainTex, float2(i.uv.x + _Amount, i.uv.y + _Amount)).b;
```

### Camera
Two cameras are used for the best visual effect. A main camera for rendering the environment, enemies ect, and another camera for rendering first person player model only. This is achieved by assgining different layers for the player and rest of the world, and setting different culling masks for the cameras. The benefits is to prevent player model clipping into the surfaces when being close to other objects. Also, adjusting field of view for the main camera will not cause the model to stretch since it is rendered on another camera.

## Models & Entities
### Player Model & Sound
The player model consists of two part, an arm model and a weapon model. The arm model is from the game [**"Insurgency"**](https://insurgency-sandstorm.com/en), extracted by [Shadow2620](http://steamcommunity.com/id/shadow2620/) and textured by [Nanman](http://steamcommunity.com/profiles/76561198053155440/). Weapon model is extracted from the game [**"Apex Legends"**](https://www.ea.com/games/apex-legends), more specifically weapon model of R-99 SMG. Player animations are custom made with Blender by Jiawei(Derrick) Li. All player related sound(footstep, wallrun, slide, weapon sound) are extracted from the game [**"Titanfall 2"**](https://www.ea.com/games/titanfall/titanfall-2).
### Enemy Models
The enemy models used in our project were primarily third-party models from Mixamo.com. Similarly, the animations for these models were also taken from Mixamo.com.

In order to save performance, custom hitboxes for our enemies were made and attached to the enemy models. Rather than using the model's entire mesh as a collider, six colliders were attached to each enemy to register impact from bullets. A cube collider was used for the head and capsule colliders for the torso and legs (one for the torso and two on each leg).

Animator controllers were used to manage enemy AI states and trigger transition between these states (such as idle, running, attacking etc.).
### Level Design & Objects
Most objects used in the level design were from the Snaps Prototype | Sci-fi / Industrial package from the [Unity asset store](https://assetstore.unity.com/packages/3d/environments/sci-fi/snaps-prototype-sci-fi-industrial-136759).

In addition to this package, some custom objects such as floors, ramps, walkways, roofs etc. were modelled by us utilising the features of Unity ProBuilder to shape the objects as we needed.

## Querying & Observational Methods

### Participants
Five participants were chosen to test our game demo. In order to get a good sense of different perspectives, we evaluated participants of varying game experience levels.These partipants were in the age range of 18 to 22 and included:
- Two experienced gamers
- Two casual gamers
- One person who plays games rarely
### Methodology
One observational method and one querying technique were chosen in this evaluation. Cooperative evaluation was chosen as it provided a great way to encourage criticism and provide clarification on the game during the partipant's playthrough. In addition, a System Usability Scale (SUS) questionnaire was chosen as our querying technique and given to the particpant after their playthrough.

Participants played though the first two levels of the game, including the tutorial level. The game was played while screen-sharing in order for the observer to conduct their observational evaluation. After the game as previously mentioned, the participants were asked the ten items in the SUS questionnaire and their responses were recorded.

The participant's general feedback and criticisms were noted down throughout the playthrough on a document. Furthermore, the questionaire responses were similarly noted down in a specialised PDF document of the SUS questionnaire.
### Feedback
A range of feedback on our game was given.

Some criticisms included:
- The game was too difficult (gun enemies hit too often, did too much damage etc.)
- Some enemies' attack range was strange (melee enemy hit from too far away)
- The game ran poorly on some low-end systems
- Reloading was unneccesarily hard to notice when to do it
- Lighting in some areas were poor/hard to see
- Options such as FOV were only available in the main menu screen (not ingame)

Some examples of positive feedback include:
- Movement was enjoyable (features such as sliding, wallrunning & double jump)
- Game was simple to learn
- Controls were straightforward and easy to use
- Aesthetics of the game were fitting
- Gameplay was fun
- Levels were designed well (reminiscent of Halo maps for the level played)

Averaging out the SUS results gave the scores: 3.4, 1, 4.4, 1, 3.2, 3.8, 5, 3, 4.2, 1.4

These results indicate that the system was generally not complex and were easy to use by most users. The points of contention that could be improved on was that functions weren't always too well integrated, there was too much inconsistency and that some parts were awkward to use.


### Changes made from feedback
From the feedback, a number of changes were made to our game:
- The damage the enemies dealt was reduced from 15 to 8
- The soldier enemy's attack chance was reduced from a flat 33% rate, to having a higher/lower chance to hit based on their distance from the player (50% when really close, 33% medium range, 25% long range)
- The melee enemy's attack range was adjusted to be more realistic
- Occlusion culling was introduced to improve performance
- Auto-reloading when gun magazine is empty
- Lighting was overall improved
- Give the user ability to change settings when the game is paused

## External Code/APIs used
The following Code/APIs/Models are from external sources and is not of our own making, all credit goes to them.
- Enemy models and animation were used from Mixamo.com
- Enemy game sounds were used from Zapsplat.com
- Player arm model is the property of [New World Interactive](https://newworldinteractive.com/) in the game [Insurgency: Sand Storm](https://insurgency-sandstorm.com/en)
- Weapon model is the property of [Respawn Entertainment, LLC](https://www.respawn.com/) in the game [Apex Legends](https://www.ea.com/games/apex-legends)
- Player sound effects are the property of [Respawn Entertainment, LLC](https://www.respawn.com/) in the game [Titanfall 2](https://www.ea.com/games/titanfall/titanfall-2)
- Most level objects from 'Snaps Prototype | Sci-Fi / Industrial' on the Unity asset store (https://assetstore.unity.com/packages/3d/environments/sci-fi/snaps-prototype-sci-fi-industrial-136759)
- Sound scripts are made by [Brackyes](https://www.youtube.com/watch?v=6OT43pvUyfY)