namespace global

[<AutoOpen>]
module Draw =
    open SFML.Window;
    open SFML.Graphics;
    open SFML.Audio;

    let loadTextures() =
        let listbuilder = new ListBuilder()
        listbuilder{
            yield "red", new Texture("redsplotch.jpg")
            yield "orange", new Texture("orangesplotch.jpg")
            yield "yellow", new Texture("yellowsplotch.jpg")
            yield "green", new Texture("greensplotch.jpg")
            yield "cyan", new Texture("cyansplotch.jpg")
            yield "blue", new Texture("bluesplotch.jpg")
            yield "purple", new Texture("purplesplotch.jpg")
        }

    let getTexture textures desiredTexture =
        let matchText text comp =
            match comp with
            | (t,_) when t = text -> true
            | _ -> false
        let (_,texture) =
            textures
            |> List.find (matchText desiredTexture)
        texture

    let updateBallSprite (ballState : BallState) =
        ballState.Sprite.Position <- new Vector2f(ballState.Position.X, ballState.Position.Y)
        ballState
        
    let updatePaddleSprite (paddleState:PaddleState) =
        paddleState.Sprite.Position <- new Vector2f(paddleState.Position.X, paddleState.Position.Y)
        paddleState

    let updateGraphics gameState =
        let ballState = updateBallSprite gameState.BallState
        let paddleState = updatePaddleSprite gameState.PaddleState
        {gameState with BallState = ballState; PaddleState = paddleState}


    let draw (win:RenderWindow) textures gameState =
        win.Clear Color.Black

        gameState.BallState.Sprite.Draw(win, RenderStates.Default)
        gameState.PaddleState.Sprite.Draw(win, RenderStates.Default)

        List.map (fun (s:BlockState)-> s.Sprite.Draw(win,RenderStates.Default)) gameState.ActiveBlocks |> ignore
        win.Display()
        ()