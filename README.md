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

## The idea (for future me)

I didn’t want to study maths. Then my brain, for reasons known only to itself, decided the obvious solution was to make a whole atmospheric mystery game where maths is how you investigate. This is the full design I was trying to get at, so future me can reopen the potato and remember what it was meant to grow into.

### The feeling

This is a single-player narrative mystery about needing to understand what is happening in a place. The player should move through curiosity, uncertainty, investigation, discovery, and then that little mental click where something seen earlier suddenly means something different. It should feel like a serious story that happens to involve learning maths, not a school worksheet wearing a detective hat.

The place is the Morrow Institute, a believable research institution with people, departments, records, offices, equipment, routines, and history. The building should feel ordinary enough to be real, and gradually become significant as the player learns more. The mystery is the reason to keep going: a contradiction or unexplained event raises a question; documents, objects, people, and research materials provide incomplete pieces; the player has to decide what those pieces mean.

### What the player actually does

You play as the protagonist, moving through the place yourself. The camera is a deliberate 2D side view: walk through connected rooms and corridors to discover the building, rather than looking down at a map or playing as a floating cursor. Movement and interaction stay simple. Get close to a selected object, see a clear context prompt, and inspect it. Read a paper, examine a research item, use a computer or terminal, talk to or listen to a human presence, find evidence, and return to earlier rooms when new information gives them a different meaning.

Not everything is a clue. Some props make the institute feel lived in, some tell you about its people or history, some matter to the investigation, and some are just there. No giant quest arrows, no glowing outline around every object, no collecting random junk for points. Let the player notice things.

### The investigation loop

The basic rhythm is:

1. Enter a place with a reason to be there.
2. Notice a detail that raises a question.
3. Inspect selected objects and find documents or other evidence.
4. Compare what different sources say; some information can be incomplete or misleading without anyone necessarily having the whole picture.
5. Explore another connected room when a lead points somewhere.
6. Use a mathematical idea to test, calculate, interpret, or verify something that the story actually depends on.
7. Let the result unlock information, a route, or a new interpretation.
8. Make a conclusion that has a consequence, then reach a short ending for this chapter.

The player should do more than collect three objects and type a password. They need context to understand the question, and their reasoning should change what they know or can do. The intended feeling at the end is: “I entered a place, noticed something, investigated, found information that didn’t make sense, had to think, solved something, and now I understand the situation differently. I want to know what happens next.”

### Maths is part of the mystery

The long-term idea is for maths to be a major solving system, eventually drawing on the Class 11 CBSE Mathematics knowledge map and moving from prerequisites toward harder, non-routine and JEE-level problems. That is a future direction, not a demand to cram an entire syllabus into the first build. The vertical slice needs one substantial, carefully designed mathematical encounter.

That encounter must have a clear, valid solution and use information the player can find in the game. It should require real reasoning, not a trivial fact or an arbitrary code. The answer format must be something the game can validate honestly; if the task asks for a proof, a single number box cannot pretend to judge the proof. Most importantly, the result must matter to the case: it could verify a relationship, expose a discrepancy, reveal more terminal information, unlock a document, change how existing evidence reads, or open the next route. “Correct!” by itself is not a story consequence.

The player should be able to try before being taught the formal method, when that makes sense. If they get stuck, hints help them discover the path instead of punishing them or instantly giving away the answer. For example, point first toward an observation, then toward a relationship between pieces of information, then toward a useful method, and only then give more direct assistance. No XP, coins, streaks, or reward for refusing help. It is a mystery; hints are a hand, not a scorecard.

### Story and slice size

The first playable chapter should be a small, original, self-contained story with a beginning, middle, and end. The protagonist needs a specific reason to investigate. There should be a contained setting, a central unanswered question, a credible human presence (a character, recording, or someone’s trace in the evidence), multiple clues, at least one incomplete or misleading interpretation, a meaningful maths obstacle, a discovery that changes the player’s understanding, and a consequence. Resolve this chapter while leaving room for the larger world and mystery later.

Keep the first environment to roughly three to five connected rooms. An entrance or corridor, an office, an archive, a research space, and perhaps one restricted area are possible jobs those rooms could do; they should not be five empty boxes built to hit a number. Make the layout believable, give at least a couple of rooms their own story or character details, and let the player revisit them. The eventual institution could span a few floors and departments, but building the whole place is not the first slice. No open world, city simulator, or procedural mega-building.

Let story arrive through exploration, short interactions, papers, brief dialogue, quiet transitions, and the maths encounter. Don’t dump the whole explanation at once. Don’t explain every clue right when it appears. The temporary chapter’s story should be replaceable later without rebuilding the evidence and progression systems.

### Look and sound

The art should be simple but intentional, original, and emotionally serious. A restrained palette might use muted greens, old paper, dirty cream, faded blues, dark wood, and controlled warm light; different rooms can have their own identity while still belonging to one institute. Compose rooms like people use them: furniture with a purpose, believable research material, readable doorways, useful negative space, layered backgrounds and foregrounds, and objects placed with care. Avoid the “random sprites in a hallway” look.

Use light and shadow selectively. Pools of warm light, colder rooms, silhouettes, door frames, and changes in ambience can guide attention without plastering the screen with effects. Keep important things visible enough to play. Sound is part of the building too: room-specific ventilation, electrical hum, distant machinery, footsteps, paper and terminal sounds, and occasional building noises. Silence can stay silent; music should have a reason to arrive. Use original or properly usable art and audio. If a final asset isn’t available, make a coherent temporary version and label it honestly instead of acting like the missing thing exists.

### Systems the story needs

Keep the machinery modest, but make the important state real and persistent. Evidence should have an ID, title, content, source, and discovered state; relationships and unlock conditions can be added where useful. Track the current question, discovered evidence, maths challenge status, story flags, and what is now available. Documents, hints, the evidence view, interaction prompts, maths screen, pause/menu, and save feedback should look and feel like the same game. Save the meaningful progress so closing the game doesn’t erase the investigation.

Keep code and content separable enough that a future story can replace the temporary one. Player movement, camera, interactions, evidence, investigation state, maths validation, hints, story progression, audio, and persistence each need clear responsibilities, but this is a small game slice, not an excuse to invent thirty frameworks. Use the existing Unity project and reuse suitable setup and assets. Don’t overwrite useful work without checking it.

### Things this idea is not

Not first-person, not conventional 3D, not an overhead map game. Not combat, crafting, survival, multiplayer, RPG stats, XP, coins, daily streaks, a complete maths curriculum, or the whole future institute. Not a generic educational app, not a giant fetch quest, and not a mystery that explains every answer immediately. Keep the focus on walking through a place, investigating information, and using maths because the answer changes what the player understands.

Basically: I was avoiding a textbook and somehow designed a research institution, a mystery, a story, a hint system, a save system, and a maths problem with narrative consequences. This was the plan. Future me, start with one good room and one good question.