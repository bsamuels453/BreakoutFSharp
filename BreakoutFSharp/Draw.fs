namespace global

[<AutoOpen>]
module Draw =
    open SFML.Window;
    open SFML.Graphics;
    open SFML.Audio;
    open System.Diagnostics;

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
    let mutable private drawablesToAdd:((string*Texture) list -> SpriteState) list = []
    let mutable private drawablesToRemove:ObjectId list = []

    let queueSpriteUpdate objectId =
        drawablesToUpdate <- List.Cons (objectId, drawablesToUpdate)

    let queueSpriteAddition spriteInitializer =
        drawablesToAdd <- List.Cons (spriteInitializer, drawablesToAdd)

    let queueSpriteDeletion objectId =
        drawablesToRemove <- List.Cons (objectId, drawablesToRemove)

    let updateRenderState renderState gameState textures =
        let mutable drawableSprites = renderState.Sprites |> Array.ofList

        if drawablesToAdd.Length <> 0 then
            let generatedSprites = drawablesToAdd |> List.map (fun f -> f textures)
            drawableSprites <- Array.append drawableSprites (Array.ofList generatedSprites)
            drawableSprites <- Array.sortBy (fun elem -> elem.ZLayer) drawableSprites
            drawablesToAdd <- []

        drawableSprites <- drawableSprites |> Array.filter (fun elem -> not( drawablesToRemove |> List.exists elem.Id.Equals))
        drawablesToRemove <- []

        let newRenderState = {Sprites=List.ofArray drawableSprites}

        let rec applyUpdates sprites updates =
            match updates with
            | [] -> ()
            | h::t ->
                let idx = sprites |> Array.findIndex (fun s -> s.Id.Equals h)
                let sprite = sprites.[idx]
                sprites.[idx] <- sprite.Update newRenderState gameState sprite
                applyUpdates sprites t

        let autoUpdateSprites = 
            drawableSprites 
            |> Array.filter (fun sp -> sp.AutoUpdate)
            |> Array.map (fun sp -> sp.Id)
            |> List.ofArray

        drawablesToUpdate <- List.append drawablesToUpdate autoUpdateSprites

        applyUpdates drawableSprites drawablesToUpdate
        drawablesToUpdate <- []

        {renderState with Sprites = List.ofArray drawableSprites}

    let draw (win:RenderWindow) renderState =
        win.Clear Color.Black

        renderState.Sprites |> List.map (fun s -> s.Sprite.Draw(win,RenderStates.Default)) |> ignore

        win.Display()
        ()

    let genDefaultPaddleSprite gameState =
        let createSprite textures =
            let sprite = new RectangleShape(new Vector2f(paddleWidth, paddleHeight));
            sprite.Texture <- getTexture textures "red"
            sprite.Position <- new Vector2f(gameState.PaddleState.Position.X, gameState.PaddleState.Position.Y)
            let updatePaddle renderState gameState (sprite:SpriteState) =
                let paddleState = gameState.PaddleState
                sprite.Sprite.Position <- new Vector2f(paddleState.Position.X, paddleState.Position.Y)
                sprite

            {Sprite=sprite; Id=gameState.PaddleState.PaddleId; ZLayer = 1.0; Update=updatePaddle; AutoUpdate=false}
        queueSpriteAddition createSprite

    let genFallingBlockAnim ballPos blockPos =
        let createSprite (textures:(string*Texture) list) =
            let idx = List.findIndex blockPos.Y.Equals blockYCoords
            let texture = snd textures.[idx]
            let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight));
            sprite.Texture <- texture
            sprite.Position <- new Vector2f(blockPos.X, blockPos.Y)

            let displayTime = 1.0
            let origPos = sprite.Position
            let blockCenter = {X=blockPos.X + blockWidth/2.0f; Y=blockPos.Y + blockHeight/2.0f}
            let initlVelocityX = float ((blockCenter.X-ballPos.X) * 5.0f)
            let initlVelocityY = float ((blockCenter.Y-ballPos.Y) * 10.0f)
            let accelerationY = 800.0

            let sw = new Stopwatch()
            sw.Start()

            let updateAnim renderState gameState (s:SpriteState) =
                sw.Stop()
                let elapsed = sw.Elapsed.TotalSeconds
                
                if elapsed > displayTime then
                    queueSpriteDeletion s.Id
                else sw.Start()

                let deltaY = initlVelocityY * elapsed + 0.5 * accelerationY * elapsed * elapsed
                let deltaX = initlVelocityX * elapsed
                s.Sprite.Position <- new Vector2f(origPos.X + float32 deltaX, origPos.Y + float32 deltaY)
                let mutable transparency = 1.0 - elapsed/displayTime
                if transparency > 1.0 then transparency <- 1.0
                if transparency < 0.0 then transparency <- 0.0
                let color = new Color(255uy, 255uy, 255uy, byte <| 255.0 * transparency)
                s.Sprite.FillColor <- color
                s

            {Sprite=sprite; Id=generateSpriteId(); ZLayer= 1.0; Update=updateAnim; AutoUpdate=true}
        queueSpriteAddition createSprite
        ()

    let genDefaultBallSprite gameState =
        let createSprite textures =
            let sprite = new CircleShape(ballWidth/2.0f)
            sprite.Texture <- getTexture textures "blue"
            sprite.Position <- new Vector2f(gameState.BallState.Position.X, gameState.BallState.Position.Y)
            let updateBall renderState gameState (sprite:SpriteState) =
                let ballState = gameState.BallState
                sprite.Sprite.Position <- new Vector2f(ballState.Position.X, ballState.Position.Y)
                sprite

            {Sprite=sprite; Id=gameState.BallState.BallId; ZLayer = 1.0; Update=updateBall; AutoUpdate=false}
        queueSpriteAddition createSprite

    let genDefaultBlockSprites gameState =
        let updateBlock renderState gameState (sprite:SpriteState) = sprite

        gameState.ActiveBlocks |> List.map (fun block ->
            let createSprite (textures:(string*Texture) list) = 
                let idx = List.findIndex block.Position.Y.Equals blockYCoords
                let texture = snd textures.[idx]
                let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight))
                sprite.Texture <- texture
                sprite.Position <- new Vector2f(block.Position.X, block.Position.Y)
                {Sprite=sprite; Id=block.BlockId; ZLayer = 0.0; Update=updateBlock; AutoUpdate=false}

            queueSpriteAddition createSprite
            )

    let generateDefaultScene gameState =
            genDefaultPaddleSprite gameState |> ignore
            genDefaultBallSprite gameState |> ignore
            genDefaultBlockSprites gameState |> ignore