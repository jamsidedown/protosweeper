# Protosweeper

PvP Minesweeper over gRPC

## Goals

- Practice mode
  - Single player
  - Untimed
  - Play specific seeds (optional)
  - Restart games
  - Interactable web page to work test strategy
    - Web page uses websockets
  - Playable through gRPC to practice for PvP
- PvP (either race or timed with scoreboard)
  - Race
    - Game can start once two players join
    - Starts when the first player clicks a cell
    - First click streamed to both players
    - All subsequent clicks are kept hidden from opponent
    - Winner is the first player to clear the board
    - Clicking a mine loses the game
  - Leaderboard
    - Solve five games back to back
    - Discard the slowest and fastest times
    - The average of the remaining three games goes on the scoreboard
  - When joining a game, provide a spectator link
  - Spectate through webpage
    - Stream clicks with SSE
  - Receive seed at the end of a game for optional practice

## User flow

- Practice
  - Create a new game
    - Difficulty
    - Optional seed
    - Check game is solvable without guessing?
  - Store game in `ConcurrentDictionary`?
  - Connect websockets for the browser
  - Connect optional gRPC
  - Connections add a `ChannelReader` and `ChannelWriter` to the game
  - Listen for clicks on all `ChannelReader`s
  - Generate game board when first click received
  - Stream cells to all `ChannelWriter` instances
    - Prioritise gRPC?
  - Finish the game when all cells cleared or mine detonated
  - Cleanup the game when all channels disconnected or store state in a DB?
  - Restarting a game returns the state to just after the first click
- PvP
  - TBC
