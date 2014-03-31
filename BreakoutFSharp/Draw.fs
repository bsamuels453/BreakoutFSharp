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

    let updateBallSprite (ballSprite:CircleShape) (ballState : BallState) =
        ballSprite.Position <- new Vector2f(ballState.Position.X, ballState.Position.Y)
        ballSprite
        
    let updatePaddleSprite (paddleSprite:RectangleShape) (paddleState:PaddleState) =
        paddleSprite.Position <- new Vector2f(paddleState.Position.X, paddleState.Position.Y)
        paddleSprite

    let updateBlockSprites (blockSprites:RectangleShape list) activeBlocks =
        let activeBlockPositions = activeBlocks |> List.map (fun a -> new Vector2f(a.Position.X, a.Position.Y))
        if blockSprites.Length <> activeBlocks.Length then
            let blocksToRemove = 
                blockSprites
                |> List.filter (fun b -> not <| List.exists b.Position.Equals activeBlockPositions )
            blocksToRemove |> List.map (fun b->b.Dispose()) |> ignore
            let blockPosToRemove = blocksToRemove |> List.map (fun b -> b.Position)
            let newBlockSprites = blockSprites |> List.filter (fun b-> not <| List.exists b.Position.Equals blockPosToRemove)
            newBlockSprites
        else
            blockSprites

    let updateRenderState renderState gameState =
        let ballSprite = updateBallSprite renderState.BallSprite gameState.BallState
        let paddleSprite = updatePaddleSprite renderState.PaddleSprite gameState.PaddleState
        let blockSprites = updateBlockSprites renderState.BlockSprites gameState.ActiveBlocks
        {BallSprite=ballSprite; PaddleSprite=paddleSprite; BlockSprites=blockSprites}

    let draw (win:RenderWindow) renderState =
        win.Clear Color.Black

        renderState.BallSprite.Draw(win, RenderStates.Default)
        renderState.PaddleSprite.Draw(win, RenderStates.Default)

        List.map (fun (s:RectangleShape)-> s.Draw(win,RenderStates.Default)) renderState.BlockSprites |> ignore
        win.Display()
        ()

    let genDefaultPaddleSprite gameState textures =
        let sprite = new RectangleShape(new Vector2f(paddleWidth, paddleHeight));
        sprite.Texture <- getTexture textures "red"
        sprite.Position <- new Vector2f(gameState.PaddleState.Position.X, gameState.PaddleState.Position.Y)
        sprite

    let genDefaultBallSprite gameState textures =
        let sprite = new CircleShape(ballWidth/2.0f)
        sprite.Texture <- getTexture textures "blue"
        sprite.Position <- new Vector2f(gameState.BallState.Position.X, gameState.BallState.Position.Y)
        sprite

    let genDefaultBlockSprites gameState (textures:(string*Texture) list) =
        gameState.ActiveBlocks |> List.map (fun block ->
            let idx = List.findIndex block.Position.Y.Equals blockYCoords
            let texture = snd textures.[idx]
            let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight))
            sprite.Texture <- texture
            sprite.Position <- new Vector2f(block.Position.X, block.Position.Y)
            sprite
            )
        

    let genDefaultRenderState gameState textures =
        {
            PaddleSprite=genDefaultPaddleSprite gameState textures;
            BallSprite=genDefaultBallSprite gameState textures;
            BlockSprites=genDefaultBlockSprites gameState textures
        }