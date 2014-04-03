﻿namespace global

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
    let mutable private drawablesToAdd:SpriteState list = []
    let mutable private drawablesToRemove:ObjectId list = []

    let queueSpriteUpdate objectId =
        drawablesToUpdate <- List.Cons (objectId, drawablesToUpdate)

    let queueSpriteAddition sprite=
        drawablesToAdd <- List.Cons (sprite, drawablesToAdd)

    let queueSpriteDeletion objectId =
        drawablesToRemove <- List.Cons (objectId, drawablesToRemove)


    let updateBallSprite (ballSprite:SpriteState) (ballState : BallState) =
        ballSprite.Sprite.Position <- new Vector2f(ballState.Position.X, ballState.Position.Y)
        ballSprite
        
    let updatePaddleSprite (paddleSprite:SpriteState) (paddleState:PaddleState) =
        paddleSprite.Sprite.Position <- new Vector2f(paddleState.Position.X, paddleState.Position.Y)
        paddleSprite

    let updateBlockSprites (blockSprites:SpriteState list) activeBlocks =
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
        let mutable drawableSprites = renderState.Sprites

        if drawablesToAdd.Length <> 0 then
            drawableSprites <- List.append drawableSprites drawablesToAdd
            drawableSprites <- List.sortBy (fun elem -> elem.ZLayer) drawableSprites
            drawablesToAdd <- []

        drawableSprites <- drawableSprites |> List.filter (fun elem -> not( drawablesToRemove |> List.exists elem.Id.Equals))

        let newRenderState = {Sprites=drawableSprites}


        drawablesToRemove <- []

        drawableSprites <- drawableSprites |> List.map (fun spriteState ->
            if drawablesToUpdate |> List.exists spriteState.Id.Equals then
                spriteState.Update newRenderState gameState spriteState
            else
                spriteState
        )

        {renderState with Sprites = drawableSprites}

    let draw (win:RenderWindow) renderState =
        win.Clear Color.Black

        renderState.Sprites |> List.map (fun s -> s.Sprite.Draw(win,RenderStates.Default)) |> ignore

        win.Display()
        ()

    let genDefaultPaddleSprite gameState textures =
        let sprite = new RectangleShape(new Vector2f(paddleWidth, paddleHeight));
        sprite.Texture <- getTexture textures "red"
        sprite.Position <- new Vector2f(gameState.PaddleState.Position.X, gameState.PaddleState.Position.Y)
        let updatePaddle renderState gameState (sprite:SpriteState) =
            let paddleState = gameState.PaddleState
            sprite.Sprite.Position <- new Vector2f(paddleState.Position.X, paddleState.Position.Y)
            sprite

        let spriteState = {Sprite=sprite; Id=gameState.PaddleState.PaddleId; ZLayer = 1.0; Update=updatePaddle}
        queueSpriteAddition spriteState


    let genDefaultBallSprite gameState textures =
        let sprite = new CircleShape(ballWidth/2.0f)
        sprite.Texture <- getTexture textures "blue"
        sprite.Position <- new Vector2f(gameState.BallState.Position.X, gameState.BallState.Position.Y)
        let updateBall renderState gameState (sprite:SpriteState) =
            let ballState = gameState.BallState
            sprite.Sprite.Position <- new Vector2f(ballState.Position.X, ballState.Position.Y)
            sprite

        let spriteState = {Sprite=sprite; Id=gameState.BallState.BallId; ZLayer = 1.0; Update=updateBall}
        queueSpriteAddition spriteState

    let genDefaultBlockSprites gameState (textures:(string*Texture) list)=
        let updateBlock renderState gameState (sprite:SpriteState) = sprite

        gameState.ActiveBlocks |> List.map (fun block ->
            let idx = List.findIndex block.Position.Y.Equals blockYCoords
            let texture = snd textures.[idx]
            let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight))
            sprite.Texture <- texture
            sprite.Position <- new Vector2f(block.Position.X, block.Position.Y)
            let spriteState = {Sprite=sprite; Id=block.BlockId; ZLayer = 1.0; Update=updateBlock}
            queueSpriteAddition spriteState
            )

    let generateDefaultScene gameState textures =
            genDefaultPaddleSprite gameState textures |> ignore
            genDefaultBallSprite gameState textures |> ignore
            genDefaultBlockSprites gameState textures |> ignore