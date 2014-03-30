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

let genStartBlocks (textures:(string*Texture) list) =
    let xRange = [for x in 0..(int numBlockCols-1) -> x]
    let yRange = [for y in 0..(int numBlockRows-1) -> y]
    let blockCoords = 
        [for x in xRange do
            for y in yRange ->
                (snd textures.[y], {X=float32 x * blockWidth; Y=float32 y * blockHeight})
        ]
    let genBlock (texture, pos) =
        let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight))
        sprite.Texture <- texture
        sprite.Position <- new Vector2f(pos.X, pos.Y)
        {Position=pos; Sprite=sprite}
    List.map genBlock blockCoords

let genDefaultGameState textures =
    {
    BallState = {
                Position={X=300.0f; Y=300.0f};
                Velocity={X=initlBallSpeed; Y=initlBallSpeed}
                };
    PaddleState = {X=400.0f; Y=screenHeight-paddleXAxis};
    ActiveBlocks = genStartBlocks textures
    }

[<EntryPoint>]
[<STAThread>]
let main argv =
    let win = initializeWindow()
    let textures = loadTextures()
    let mutable gameState = genDefaultGameState textures

    while win.IsOpen() do
        win.DispatchEvents()
        let keyboardState = pollKeyboard()
        let paddleState = paddleTick gameState.PaddleState keyboardState
        let (newBallState, newActiveBlocks) = ballTick paddleState gameState.ActiveBlocks gameState.BallState

        gameState <- {BallState = newBallState; PaddleState=paddleState; ActiveBlocks = newActiveBlocks}        
        draw win textures gameState
        
        ()
    0