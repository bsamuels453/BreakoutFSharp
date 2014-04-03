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

    let mutable private drawablesToUpdate:ObjectId list = []

    let queueObjectUpdate objectId =
        drawablesToUpdate <- List.Cons (objectId, drawablesToUpdate)

    let updateBallSprite (ballSprite:BallSpriteState) (ballState : BallState) =
        ballSprite.Sprite.Position <- new Vector2f(ballState.Position.X, ballState.Position.Y)
        ballSprite
        
    let updatePaddleSprite (paddleSprite:PaddleSpriteState) (paddleState:PaddleState) =
        paddleSprite.Sprite.Position <- new Vector2f(paddleState.Position.X, paddleState.Position.Y)
        paddleSprite

    let updateBlockSprites (blockSprites:BlockSpriteState list) activeBlocks =
        let activeBlockPositions = activeBlocks |> List.map (fun a -> new Vector2f(a.Position.X, a.Position.Y))
        if blockSprites.Length <> activeBlocks.Length then
            let blocksToRemove = 
                blockSprites
                |> List.filter (fun b -> not <| List.exists b.Sprite.Position.Equals activeBlockPositions )
            blocksToRemove |> List.map (fun b->b.Sprite.Dispose()) |> ignore
            let blockPosToRemove = blocksToRemove |> List.map (fun b -> b.Sprite.Position)
            let newBlockSprites = blockSprites |> List.filter (fun b-> not <| List.exists b.Sprite.Position.Equals blockPosToRemove)
            newBlockSprites
        else
            blockSprites

    let updateRenderState renderState gameState =
        let drawables = Seq.ofList drawablesToUpdate
        drawablesToUpdate <- []
        let grouped = 
            drawables 
            |> Seq.groupBy (fun d ->
                match d with
                |BallId(_) -> BallId(0)//not meant to be real id, just placeholder for the type
                |PaddleId(_) -> PaddleId(0)
                |BlockId(_) -> BlockId(0)
            )
            |> List.ofSeq

        let ballRenderState = ref renderState.BallSprite
        let paddleRenderState = ref renderState.PaddleSprite
        let blocksRenderState = ref renderState.BlockSprites

        grouped |> List.map (fun group ->
            match group with
            | (BallId(_), _) -> ballRenderState := updateBallSprite renderState.BallSprite gameState.BallState
            | (PaddleId(_), li) -> paddleRenderState := updatePaddleSprite renderState.PaddleSprite gameState.PaddleState
            | (BlockId(_), li) ->  blocksRenderState := updateBlockSprites renderState.BlockSprites gameState.ActiveBlocks
            ) |> ignore

        {BallSprite= !ballRenderState; PaddleSprite= !paddleRenderState; BlockSprites= !blocksRenderState}

    let draw (win:RenderWindow) renderState =
        win.Clear Color.Black

        renderState.BallSprite.Sprite.Draw(win, RenderStates.Default)
        renderState.PaddleSprite.Sprite.Draw(win, RenderStates.Default)

        List.map (fun (s:BlockSpriteState)-> s.Sprite.Draw(win,RenderStates.Default)) renderState.BlockSprites |> ignore
        win.Display()
        ()

    let genDefaultPaddleSprite gameState textures : PaddleSpriteState =
        let sprite = new RectangleShape(new Vector2f(paddleWidth, paddleHeight));
        sprite.Texture <- getTexture textures "red"
        sprite.Position <- new Vector2f(gameState.PaddleState.Position.X, gameState.PaddleState.Position.Y)
        {Sprite=sprite; Id=gameState.PaddleState.PaddleId}

    let genDefaultBallSprite gameState textures : BallSpriteState =
        let sprite = new CircleShape(ballWidth/2.0f)
        sprite.Texture <- getTexture textures "blue"
        sprite.Position <- new Vector2f(gameState.BallState.Position.X, gameState.BallState.Position.Y)
        {Sprite=sprite; Id=gameState.BallState.BallId}

    let genDefaultBlockSprites gameState (textures:(string*Texture) list) : BlockSpriteState list=
        gameState.ActiveBlocks |> List.map (fun block ->
            let idx = List.findIndex block.Position.Y.Equals blockYCoords
            let texture = snd textures.[idx]
            let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight))
            sprite.Texture <- texture
            sprite.Position <- new Vector2f(block.Position.X, block.Position.Y)
            {Sprite=sprite; Id=block.BlockId}
            )

    let genDefaultRenderState gameState textures =
        {
            PaddleSprite=genDefaultPaddleSprite gameState textures;
            BallSprite=genDefaultBallSprite gameState textures;
            BlockSprites=genDefaultBlockSprites gameState textures
        }