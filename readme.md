- Toggle cursorlock by pressing O
- Move camera with W, A, S, D
- Ascend with Q, descend with E
- Faster with shift, slower with ctrl
- Exit with esc

- Press G to toggle time of day
- press H to toggle weather between sunny, rain and snow
- press T to toggle cars and pedestrians on and off (disable/enable)
- press Y to respawn all cars
- press P to toggle between flycam and car POV (if dev_camera and democarPOV are in use)
- press R to (un)pause all NPCs and traffic lights

- Add "ReplayPrefab" onto the scene hierarchy
- Objects tagged as "Replayable" will be recorded for replay
- Press N to start recording a replay
- Press M to stop recording
- Press B to replay

- Left mouse button on cameras while cursor is visible selects them
- Hold down left mouse button on one of the arrows and drag to move the camera
- Holding down Left Control while clicking allows selection of multiple cameras
- Holding down Left Shift while clicking selects the whole rig of a camera if
  there is one
- Right mouse button cancels the selection.

- Left mouse button on cars selects them
- Pressing L when one rig and one car are selected links them
- The cameras' y-coordinates will not change. This means the cameras might not link to the car. Drag the camera rig to the car's level and try again.
- Right mouse button cancels the car selection.


- To change the amount of cars and pedestrians, edit the file Assets\NPCSystem\PersistentData\NPC.config. 

- To make/edit roads, add RouteGenerator to the scene. Disable all other cameras and preferably disable all pedestrians and cars too. Click play. When in game, hold right mouse button and use WASD to move, release right mouse button to use mouse. Use G to toggle between adding and deleting waypoints. 
- When creating new paths, click and drag from point to point. New path will snap to the closest already existing waypoints, if it is withing tolerance.

- Windridge City Demo Scene doesn't have any lighting data baked in and therefore the scene has to be baked before it can be used comfortably.
- The car model supplied with the project is not optimal and probably should not be used unless a lot of tweaking is made.  