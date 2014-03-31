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

let genDefaultBallState textures =
    let sprite = new CircleShape(ballWidth/2.0f)
    sprite.Texture <- getTexture textures "blue"
    let position = {X=300.0f; Y=300.0f}
    let velocity = {X=initlBallSpeed; Y=initlBallSpeed}
    {Position=position; Velocity=velocity; Sprite=sprite}

let genDefaultPaddleState textures : PaddleState =
    let sprite = new RectangleShape(new Vector2f(paddleWidth, paddleHeight));
    sprite.Texture <- getTexture textures "red"
    let position = {X=400.0f; Y=screenHeight - paddleXAxis};
    {Position=position; Sprite=sprite}

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
    let mutable gameState = genDefaultGameState textures

    while win.IsOpen() do
        win.DispatchEvents()
        let keyboardState = pollKeyboard()

        let newPaddlePos = paddleTick gameState.PaddleState.Position keyboardState
        let newPaddleState = {gameState.PaddleState with Position = newPaddlePos}
        gameState <- {gameState with PaddleState = newPaddleState}  

        let (newBallState, newActiveBlocks) = ballTick gameState.PaddleState.Position gameState.ActiveBlocks gameState.BallState
        gameState <- {gameState with BallState = newBallState; ActiveBlocks = newActiveBlocks}

        gameState <- updateGraphics gameState
        draw win textures gameState
        
        ()
    0