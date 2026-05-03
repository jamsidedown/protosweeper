# Todo

- Write solver (in private repo)
- Ensure game is solvable before returning the id to the user
  - Use solver submodule/service with a potential seed to check it can be solved programmatically
- Add background job to automatically clean up games with no clients every 10 minutes or so
- ~~Allow practice mode to take a specific seed~~
  - Unhappy with the current implementation, but it's working
- Allow connecting via gRPC
- Allow connecting via SSE to spectate your competitive game (?)
  - Not sure if this is worth adding
- Restrict websocket connections to practice mode
- Restrict gRPC connections to competitive mode
- Add some backing data store for fastest solves
- Allow users to supply a username when connecting via gRPC for fastest solves
- Add a leaderboard page