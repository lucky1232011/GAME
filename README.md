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

## A note from me

Huff abandoned this one too. When there are more tools I can pay for and are better than codex I would start continuing to this project anyway I know its a potato rn. Technology is getting advanced day by day and am waiting for the technology to become more advanced so I could work on this half baked potato. But I can say my idea was good enough. Anyways, I appreciate those people who can execute their ideas and I appreciate them even more and get impressed if they execute it well and BRO if you are one of those I can say you got the brightest future. So I feel I have realised something Ideas without Execution is nothing at all. Tho I think so I will still create more in the future for sure and refine my other half baked potatoes and also work on my other ideas.

## The idea, basically

I didn’t want to study maths, so obviously my brain went: what if I make a whole game for myself and sneak the maths into it. Very normal detour. You’re Ira, stuck doing an after-hours check in a records annex before the files get transferred. One document says one thing, its copy says another, and now you’re walking through rooms, poking at objects, reading scraps, collecting evidence, and slowly realizing the numbers are part of the mystery.

The route is basically: explore, find a lead, inspect the evidence, compare what doesn’t add up, and use the clues to solve a receiver-calibration problem. The calculation gives you something concrete to check against the records; then you decide what correction to file before the transfer. So the math isn’t a random worksheet that wandered into the game. It’s the tool Ira needs to make sense of the case. The hope was that I’d care more about getting the answer if it actually changed what happened. Anyway, that was the idea. It got a little out of hand, in a potato way.
