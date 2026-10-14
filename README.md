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

I didn’t want to study maths, so naturally I thought: what if I made myself a whole atmospheric mystery game and hid the maths inside it. Efficient? Absolutely not. But at least I might actually care about the numbers if they were attached to something.

You’re the person walking through a research institute after hours. It’s a 2D side-view, so you find the place by going through it room by room, checking the things that seem worth checking, reading papers people left behind, and slowly piecing together what happened. Not every chair is secretly evidence. Some chairs are just chairs. Probably.

There’s a story in there, an unresolved contradiction, and a maths problem that matters to solving it. You gather enough context to understand what the problem is asking, take a shot at working it out, and can get hints if your brain temporarily leaves the building. The answer lets you verify something and changes what you know or can do next. So the intended loop is: walk around, notice something off, investigate, stare at numbers for a bit, work it out, then realize an earlier detail means something else. Basically I tried to trick myself into studying by making the worksheet part of a mystery. The institute and the whole case may be a slightly excessive way to avoid a textbook, but the idea was there.