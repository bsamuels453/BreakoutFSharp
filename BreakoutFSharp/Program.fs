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

let initializeWindow() =
    let win = new RenderWindow(new VideoMode(uint32 screenWidth,uint32 screenHeight), "Breakout F#")
    win.Closed.Add (fun _ -> win.Close())
    win.SetKeyRepeatEnabled false
    win

let genStartBlocks () =
    [for x in blockXCoords do
        for y in blockYCoords ->
            {Position={X=x; Y=y}}
    ]

let genDefaultBallState textures =
    let position = {X=300.0f; Y=300.0f}
    let velocity = {X=initlBallSpeed; Y=initlBallSpeed}
    {Position=position; Velocity=velocity}

let genDefaultPaddleState textures : PaddleState =
    let position = {X=400.0f; Y=screenHeight - paddleXAxis};
    {Position=position}

let genDefaultGameState textures =
    {
    BallState = genDefaultBallState textures
    PaddleState = genDefaultPaddleState textures
    ActiveBlocks = genStartBlocks textures
    }

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
    let textures = loadTextures()
    let mutable gameState = genDefaultGameState()
    let mutable renderState = genDefaultRenderState gameState textures
    let stopwatch = new Stopwatch()

    let mutable samples = 0
    let mutable total = 0.0

    while win.IsOpen() do
        throttleTo60fps()
        stopwatch.Start()

        win.DispatchEvents()
        let keyboardState = pollKeyboard()

        let newPaddlePos = paddleTick gameState.PaddleState.Position keyboardState
        let newPaddleState = {gameState.PaddleState with Position = newPaddlePos}
        gameState <- {gameState with PaddleState = newPaddleState}  

        let (newBallState, newActiveBlocks) = ballTick gameState.PaddleState.Position gameState.ActiveBlocks gameState.BallState
        gameState <- {gameState with BallState = newBallState; ActiveBlocks = newActiveBlocks}

        renderState <- updateRenderState renderState gameState
        draw win renderState

        let idleTime = getIdleTime stopwatch
        executeEveryHundred (fun () -> System.Console.WriteLine("Idle: " + string (100.0 - idleTime * 100.0) + "%"))
        stopwatch.Reset()
    0