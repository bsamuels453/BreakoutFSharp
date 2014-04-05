﻿namespace global

[<AutoOpen>]
module SpriteGen =
    open SFML.Graphics;
    open SFML.Window;
    open System.Diagnostics;

    let genDefaultPaddleSprite gameState =
        let createSprite textures =
            let sprite = new RectangleShape(new Vector2f(paddleWidth, paddleHeight));
            sprite.Texture <- Resource.GetTexture textures "red"
            sprite.Position <- new Vector2f(gameState.PaddleState.Position.X, gameState.PaddleState.Position.Y)
            let updatePaddle renderState gameState (sprite:SpriteState) =
                let paddleState = gameState.PaddleState
                sprite.Sprite.Position <- new Vector2f(paddleState.Position.X, paddleState.Position.Y)
                sprite

            {Sprite=sprite; Id=gameState.PaddleState.PaddleId; ZLayer = 1.0; Update=updatePaddle; AutoUpdate=false}
        Draw.QueueSpriteAddition createSprite

    let genFallingBlockAnim ballPos blockPos =
        let createSprite (textures:(string*Texture) list) =
            let idx = List.findIndex blockPos.Y.Equals blockYCoords
            let texture = snd textures.[idx]
            let sprite = new RectangleShape(new Vector2f(blockWidth, blockHeight));
            sprite.Texture <- texture
            sprite.Position <- blockPos.ToVec2f()

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
                    Draw.QueueSpriteDeletion s.Id
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
        Draw.QueueSpriteAddition createSprite
        ()

    let genDefaultBallSprite gameState =
        let createSprite textures =
            let sprite = new CircleShape(ballWidth/2.0f)
            sprite.Texture <- Resource.GetTexture textures "blue"
            sprite.Position <- gameState.BallState.Position.ToVec2f()
            let updateBall renderState gameState (sprite:SpriteState) =
                let ballState = gameState.BallState
                sprite.Sprite.Position <- ballState.Position.ToVec2f()
                sprite

            {Sprite=sprite; Id=gameState.BallState.BallId; ZLayer = 1.0; Update=updateBall; AutoUpdate=false}
        Draw.QueueSpriteAddition createSprite

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

            Draw.QueueSpriteAddition createSprite
            )

    let generateDefaultScene gameState =
            genDefaultPaddleSprite gameState |> ignore
            genDefaultBallSprite gameState |> ignore
            genDefaultBlockSprites gameState |> ignore