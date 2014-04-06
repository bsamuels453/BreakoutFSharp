namespace global

module SpriteGen =
    open SFML.Graphics;
    open SFML.Window;
    open System.Diagnostics;

    let genBallGhost gameState delayFrameCount sizeMult transp=
        let ghostWidth = float32 ballWidth * sizeMult
        let prevBallCoords = ref [ for i in 1 .. delayFrameCount -> {X= -100.0f; Y= -100.0f} ]

        let calcNormalizedPos pos =
            let center = pos + {X=ballWidth/2.0f; Y=ballWidth/2.0f}
            center - {X=ghostWidth/2.0f; Y=ghostWidth/2.0f}

        let updateGhost renderState gameState (spriteState:SpriteState) =
            let newPos = (!prevBallCoords).Head
            
            let normalizedPos = calcNormalizedPos gameState.BallState.Position
            prevBallCoords :=  (!prevBallCoords).Tail @ [normalizedPos]

            spriteState.Sprite.Position <- newPos.ToVec2f()

            spriteState

        let createSprite textures =
            let sprite = new CircleShape(ghostWidth/2.0f)
            sprite.Texture <- Resource.extractResource textures "blue"
            sprite.FillColor <- new Color(255uy, 255uy, 255uy, byte (transp*100.0f))
            sprite.Position <- new Vector2f(-100.0f, -100.0f)

            {Sprite=sprite; Id=GameFunctions.generateSpriteId() ; ZLayer = 1.0; Update=updateGhost; AutoUpdate=true}

        Draw.queueSpriteAddition createSprite


    let genDefaultPaddleSprite gameState =
        let fadeTime = 0.25
        let sw = new Stopwatch()

        let updatePaddle renderState gameState (sprite:SpriteState) =
            let paddleState = gameState.PaddleState
            sprite.Sprite.Position <- paddleState.Position.ToVec2f()

            if sw.IsRunning then
                sw.Stop()
                let elapsed = sw.Elapsed.TotalSeconds
                sw.Start()
                if elapsed > fadeTime then
                    sw.Reset()
                    sprite.Sprite.FillColor <- new Color(128uy, 128uy, 128uy, 255uy)
                    {sprite with AutoUpdate=false}
                else
                    let delta = byte(elapsed/fadeTime*128.0)
                    sprite.Sprite.FillColor <- new Color(255uy - delta, 255uy - delta, 255uy - delta, 255uy)
                    sprite

            elif paddleState.CollidedLastFrame then
                sw.Restart()
                sprite.Sprite.FillColor <- new Color(255uy, 255uy, 255uy, 255uy)
                {sprite with AutoUpdate=true}
            else
                sprite

        let createSprite textures =
            let sprite = new RectangleShape(new Vector2f(paddleWidth, paddleHeight));
            sprite.Texture <- Resource.extractResource textures "cyan"
            sprite.Position <- gameState.PaddleState.Position.ToVec2f()
            sprite.FillColor <- new Color(128uy, 128uy, 128uy, 255uy)
            {Sprite=sprite; Id=gameState.PaddleState.PaddleId; ZLayer = 1.0; Update=updatePaddle; AutoUpdate=false}

        Draw.queueSpriteAddition createSprite

    let genFallingBlockAnim ballPos blockPos =
        let sw = new Stopwatch()
        let displayTime = 1.0
        let origPos = blockPos
        let blockCenter = {X=blockPos.X + blockWidth/2.0f; Y=blockPos.Y + blockHeight/2.0f}
        let initlVelocityX = float ((blockCenter.X-ballPos.X) * 5.0f)
        let initlVelocityY = float ((blockCenter.Y-ballPos.Y) * 10.0f)
        let accelerationY = 800.0
        let fadedColor = 170uy

        let updateAnim renderState gameState (s:SpriteState) =
            sw.Stop()
            let elapsed = sw.Elapsed.TotalSeconds
                
            if elapsed > displayTime then
                Draw.queueSpriteDeletion s.Id
            else sw.Start()

            let deltaY = initlVelocityY * elapsed + 0.5 * accelerationY * elapsed * elapsed
            let deltaX = initlVelocityX * elapsed
            s.Sprite.Position <- new Vector2f(origPos.X + float32 deltaX, origPos.Y + float32 deltaY)
            let mutable transparency = 1.0 - elapsed/displayTime
            if transparency > 1.0 then transparency <- 1.0
            if transparency < 0.0 then transparency <- 0.0
            let color = new Color(fadedColor, fadedColor, fadedColor, byte <| 255.0 * transparency)
            s.Sprite.FillColor <- color
            s

        let createSprite (textures:(string*Texture) list) =
            let idx = List.findIndex blockPos.Y.Equals blockYCoords
            let texture = snd textures.[idx]
            let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight));
            sprite.Texture <- texture
            sprite.Position <- blockPos.ToVec2f()
            sprite.FillColor <- new Color(fadedColor, fadedColor, fadedColor, 255uy)
            sw.Start()

            {Sprite=sprite; Id=generateSpriteId(); ZLayer= 1.0; Update=updateAnim; AutoUpdate=true}
        Draw.queueSpriteAddition createSprite
        ()

    let genDefaultBallSprite gameState =
        let updateBall renderState gameState (sprite:SpriteState) =
            let ballState = gameState.BallState
            sprite.Sprite.Position <- ballState.Position.ToVec2f()
            sprite

        let createSprite textures =
            let sprite = new CircleShape(ballWidth/2.0f)
            sprite.Texture <- Resource.extractResource textures "blue"
            sprite.Position <- gameState.BallState.Position.ToVec2f()

            {Sprite=sprite; Id=gameState.BallState.BallId; ZLayer = 1.0; Update=updateBall; AutoUpdate=false}
        Draw.queueSpriteAddition createSprite

    let genDefaultBlockSprites gameState =
        let updateBlock renderState gameState (sprite:SpriteState) = sprite

        gameState.ActiveBlocks |> List.map (fun block ->
            let createSprite (textures:(string*Texture) list) = 
                let idx = List.findIndex block.Position.Y.Equals blockYCoords
                let texture = snd textures.[idx]
                let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight))
                sprite.Texture <- texture
                sprite.Position <- block.Position.ToVec2f()
                {Sprite=sprite; Id=block.BlockId; ZLayer = 0.0; Update=updateBlock; AutoUpdate=false}

            Draw.queueSpriteAddition createSprite
            )

    let generateDefaultScene gameState =
            genDefaultPaddleSprite gameState |> ignore
            genDefaultBallSprite gameState |> ignore
            genDefaultBlockSprites gameState |> ignore
