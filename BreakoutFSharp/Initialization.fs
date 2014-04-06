namespace global

[<AutoOpen>]
module Initialization =
    open SFML.Window;
    open SFML.Graphics;

    let initializeWindow() =
        let win = new RenderWindow(new VideoMode(uint32 screenWidth,uint32 screenHeight), "Breakout F#")
        win.Closed.Add (fun _ -> win.Close())
        win.SetKeyRepeatEnabled false
        win

    let private genStartBlocks () =
        [for x in blockXCoords do
            for y in blockYCoords ->
                {Position={X=x; Y=y}; BlockId=generateSpriteId()}
        ]

    let private genDefaultBallState() =
        let position = {X=300.0f; Y=300.0f}
        let velocity = {X=initlBallSpeed; Y=initlBallSpeed}
        {Position=position; Velocity=velocity; BallId=generateSpriteId(); NumBounces=1}

    let private genDefaultPaddleState() : PaddleState =
        let position = {X=400.0f; Y=screenHeight - paddleXAxis};
        {Position=position; PaddleId=generateSpriteId(); CollidedLastFrame=false}

    let genDefaultGameState() =
        {
        BallState = genDefaultBallState()
        PaddleState = genDefaultPaddleState()
        ActiveBlocks = genStartBlocks()
        }