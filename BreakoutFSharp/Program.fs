open SFML.Window;
open SFML.Graphics;
open SFML.Audio;
open System;
open GameTypes;
open Draw;
open Update;
open System.IO;

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

[<EntryPoint>]
[<STAThread>]
let main argv =
    let win = initializeWindow()
    let textures = loadTextures()
    let mutable gameState = genDefaultGameState()
    let mutable renderState = genDefaultRenderState gameState textures

    while win.IsOpen() do
        win.DispatchEvents()
        let keyboardState = pollKeyboard()

        let newPaddlePos = paddleTick gameState.PaddleState.Position keyboardState
        let newPaddleState = {gameState.PaddleState with Position = newPaddlePos}
        gameState <- {gameState with PaddleState = newPaddleState}  

        let (newBallState, newActiveBlocks) = ballTick gameState.PaddleState.Position gameState.ActiveBlocks gameState.BallState
        gameState <- {gameState with BallState = newBallState; ActiveBlocks = newActiveBlocks}

        renderState <- updateRenderState renderState gameState
        draw win renderState
        
        ()
    0