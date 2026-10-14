# The Quiet Interval

A short, atmospheric 2D side-scrolling mystery in the Morrow Institute records annex. The camera follows Ira through connected rooms; the player sees the space from the side and discovers new rooms by walking into them.

## Run it

1. Open this project folder in Unity Hub with Unity **6000.6.5f1**.
2. Open `Assets/Scenes/SampleScene.unity` and press **Play**.
3. Choose **Enter the annex**.

## Controls

- **A / D** or **← / →**: move
- **E**: inspect the nearby object shown in the prompt
- **Tab**: open case notes and discovered evidence
- **Esc**: close an open document or open the pause menu
- The receiver terminal accepts a decimal metres answer and offers optional, progressive hints.

Evidence, the receiver calculation, and Ira's last position are saved locally. **Restart investigation** in the pause menu clears that save.

The runtime camera, visible animated protagonist, connected room plates, collision floor, locked passage, interface, and audio playback are initialized from the existing sample scene. Story copy is in `Assets/Resources/QuietHours/Data/story.json`; original room art, character frames, and room audio are in `Assets/Resources/QuietHours/`.
