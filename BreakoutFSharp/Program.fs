open SFML.Window;
open SFML.Graphics;
open SFML.Audio;
open System;
open GameTypes;
open Draw;
open Update;
open System.Threading;
open System.IO;
open System.Diagnostics;

let throttleTo60fps =
    let sw = new Stopwatch()
    sw.Start()
    let rec loopUntilFrameEnd() =
        sw.Stop()
        let elapsed = sw.Elapsed.TotalMilliseconds
        if elapsed >= 16.6 then
            sw.Restart()
        else
            sw.Start()
            let a = [1..100]
            loopUntilFrameEnd()
    loopUntilFrameEnd
    
let getIdleTime (sw:Stopwatch) =
    sw.Stop()
    let elapsed = sw.Elapsed.TotalMilliseconds
    elapsed / 16.6

let executeEveryHundred =
    let count = ref 0
    (fun c -> 
        if !count = 100 then
            count:=0
            c()
        else
            count := !count + 1    
    )

[<EntryPoint>]
[<STAThread>]
let main argv =
    let win = initializeWindow()
    let textures = Resource.LoadTextures()
    let mutable gameState = genDefaultGameState()
    let mutable renderState = {Sprites=[]}
    SpriteGen.GenerateDefaultScene gameState
    let stopwatch = new Stopwatch()

    while win.IsOpen() do
        throttleTo60fps()
        stopwatch.Start()

        win.DispatchEvents()
        let keyboardState = Control.PollKeyboard()

        let newPaddlePos = Update.PaddleTick gameState.PaddleState keyboardState
        let newPaddleState = {gameState.PaddleState with Position = newPaddlePos}
        gameState <- {gameState with PaddleState = newPaddleState}  

        let (newBallState, newActiveBlocks) = Update.BallTick gameState.PaddleState.Position gameState.ActiveBlocks gameState.BallState
        gameState <- {gameState with BallState = newBallState; ActiveBlocks = newActiveBlocks}

        renderState <- UpdateRenderState renderState gameState textures
        Draw win renderState

        let idleTime = getIdleTime stopwatch
        executeEveryHundred (fun () -> System.Console.WriteLine("Idle during " + string (100.0 - idleTime * 100.0) + "% of 16.6ms timeslice"))
        stopwatch.Reset()
    0