namespace global

[<AutoOpen>]
module Draw =
    open SFML.Window;
    open SFML.Graphics;
    open SFML.Audio;

    let loadTextures() =
        let listbuilder = new ListBuilder()
        listbuilder{
            yield "blue", new Texture("bluesplotch.jpg")
            yield "green", new Texture("greensplotch.jpg")
            yield "purple", new Texture("purplesplotch.jpg")
            yield "red", new Texture("redsplotch.jpg")
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

    let generateBallSprite textures (ballState:BallState) =
        let c = new CircleShape(ballWidth/2.0f)
        c.Texture <- getTexture textures "blue"
        c.Position <- new Vector2f(ballState.Position.X, ballState.Position.Y)
        c

    let generatePaddleSprite textures paddleState =
        let p = new RectangleShape(new Vector2f(paddleWidth, paddleHeight))
        p.Texture <- getTexture textures "red"
        p.Position <- new Vector2f(paddleState.X, paddleState.Y)
        p


    let draw (win:RenderWindow) textures gameState =
        win.Clear Color.Black
        let listbuilder = new ListBuilder()

        let ballSprite = generateBallSprite textures gameState.BallState
        let paddleSprite = generatePaddleSprite textures gameState.PaddleState

        ballSprite.Draw(win, RenderStates.Default)
        paddleSprite.Draw(win, RenderStates.Default)
        List.map (fun (s:BlockState)-> s.Sprite.Draw(win,RenderStates.Default)) gameState.ActiveBlocks |> ignore
        win.Display()
        ()