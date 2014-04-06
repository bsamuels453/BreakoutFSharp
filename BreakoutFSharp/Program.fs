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
    let textures = Resource.loadTextures()
    let mutable gameState = genDefaultGameState()
    let mutable renderState = {Sprites=[]}
    SpriteGen.generateDefaultScene gameState
    let stopwatch = new Stopwatch()

    while win.IsOpen() do
        throttleTo60fps()
        stopwatch.Start()

        win.DispatchEvents()
        let keyboardState = Control.pollKeyboard()

        let newPaddleState = Update.paddleTick gameState.PaddleState keyboardState
        gameState <- {gameState with PaddleState = newPaddleState}  

        let (newBallState, newActiveBlocks, finalPaddleState) = Update.ballTick gameState.PaddleState gameState.ActiveBlocks gameState.BallState
        gameState <- {gameState with BallState = newBallState; ActiveBlocks = newActiveBlocks; PaddleState = finalPaddleState}

        renderState <- updateRenderState renderState gameState textures
        draw win renderState

        let idleTime = getIdleTime stopwatch
        executeEveryHundred (fun () -> System.Console.WriteLine("Idle during " + string (100.0 - idleTime * 100.0) + "% of 16.6ms timeslice"))
        stopwatch.Reset()
    0