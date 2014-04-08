namespace global

module Update =
    open SFML.Window;

    let paddleTick (oldPaddleState:PaddleState) keyboardState=
        let restrictPaddlePos pos =
            if pos.X > (screenWidth - paddleWidth) then
                {X=screenWidth-paddleWidth; Y=pos.Y} 
            elif pos.X < 0.0f then
                {X=0.0f; Y=pos.Y} 
            else pos
            
        let aState = (Control.getKeyState keyboardState Keyboard.Key.A).KeyState
        let dState = (Control.getKeyState keyboardState Keyboard.Key.D).KeyState
        if aState=Pressed && dState=Pressed ||aState=Released && dState=Released then
            {oldPaddleState with CollidedLastFrame=false}
        else
            Draw.queueSpriteUpdate oldPaddleState.PaddleId
            match aState with
            | Pressed -> 
                let pos = restrictPaddlePos {X=oldPaddleState.Position.X - paddleSpeed; Y=oldPaddleState.Position.Y}
                {oldPaddleState with Position=pos; CollidedLastFrame=false}
            | Released -> 
                let pos = restrictPaddlePos {X=oldPaddleState.Position.X + paddleSpeed; Y=oldPaddleState.Position.Y}
                {oldPaddleState with Position=pos; CollidedLastFrame=false}            


    let private calcNewBallPos (ballState:BallState) =
        let xPos = ballState.Position.X + ballState.Velocity.X
        let yPos = ballState.Position.Y + ballState.Velocity.Y
        {ballState with Position={X=xPos;Y=yPos}}


    let private resolveBoundaryCollision ballState =
        let desiredPosition = (calcNewBallPos ballState).Position
        let newVelX = 
            match desiredPosition.X with
            | n when n >= (screenWidth-ballWidth) -> -ballState.Velocity.X
            | n when n <=  0.0f -> -ballState.Velocity.X
            | _ -> ballState.Velocity.X
        let newVelY = 
            match desiredPosition.Y with
            | n when n >= (screenHeight-ballWidth) -> -ballState.Velocity.Y
            | n when n <=  0.0f -> -ballState.Velocity.Y
            | _ -> ballState.Velocity.Y
        let newVel = {X=newVelX;Y=newVelY}
        if newVel<>ballState.Velocity then
            Sound.queueSoundStart "squareblip"
        newVel


    let private rectangleOverlap p1 d1 p2 d2 =
        not (p2.X > p1.X+d1.X || p2.X+d2.X < p1.X || p2.Y > p1.Y + d1.Y || p2.Y+d2.Y < p1.Y)


    let private calculateReflectionAxis desiredPosition collidedObjectOrigins blockWidth blockHeight=
        let getReflectionAxis block =
            let leftDist = abs <| (desiredPosition.X + ballWidth) - block.X
            let rightDist = abs <| desiredPosition.X - (block.X + blockWidth)
            let topDist = abs <| (desiredPosition.Y + ballWidth) - block.Y
            let botDist = abs <| desiredPosition.Y - (block.Y + blockHeight)

            if leftDist < topDist && leftDist < botDist then
                {X=(-1.0f); Y=(1.0f)}
            elif rightDist < topDist && rightDist < botDist then
                {X=(-1.0f); Y=(1.0f)}
            else
                {X=(1.0f); Y=(-1.0f)}
        collidedObjectOrigins
        |> List.map getReflectionAxis 
        |> Seq.ofList 
        |> Seq.distinct
        |> Seq.reduce (fun v1 v2 -> {X=v1.X*v2.X; Y=v1.Y*v2.Y})


    let private resolvePaddleCollision (paddleState:PaddleState) ballState =
        let desiredPosition = (calcNewBallPos ballState).Position
        let paddleDims = {X=paddleWidth; Y=paddleHeight}
        let ballDims = {X=ballWidth; Y=ballWidth}
        if rectangleOverlap paddleState.Position paddleDims ballState.Position ballDims then
            Draw.queueSpriteUpdate paddleState.PaddleId
            Sound.queueSoundStart "blip"
            let reflect = 
                match ballState.Position.Y + ballWidth/2.0f > paddleState.Position.Y + paddleHeight/2.0f with
                | true -> 1.0f
                | false -> -1.0f

            let resolvedVel = {
                X=ballState.Velocity.X; 
                Y=abs(ballState.Velocity.Y) * reflect
                }

            ({paddleState with CollidedLastFrame=true}, resolvedVel)
        else
            (paddleState, ballState.Velocity)


    let private resolveBlockCollision activeBlocks ballState  =
        let desiredPosition = (calcNewBallPos ballState).Position
        let ballDims = {X=ballWidth; Y=ballWidth}
        let blockDims = {X=blockWidth; Y=blockHeight}
        let collideBlocks = activeBlocks |> List.filter (fun block -> rectangleOverlap desiredPosition ballDims block.Position blockDims) 
        if collideBlocks.Length = 0 then
            (activeBlocks, ballState)
        else
            let collideBlockOrigins = collideBlocks |> List.map (fun b -> b.Position)
            let reflectionAxis = calculateReflectionAxis desiredPosition collideBlockOrigins blockWidth blockHeight

            let resolvedVel = {
                X=ballState.Velocity.X*reflectionAxis.X; 
                Y=ballState.Velocity.Y*reflectionAxis.Y}
            
            let newActiveBlocks =  activeBlocks |> List.filter (fun b -> not <| List.exists b.Equals collideBlocks)

            collideBlocks |> List.map (fun b -> Draw.queueSpriteDeletion b.BlockId) |> ignore
            collideBlocks |> List.map (fun b -> SpriteGen.genFallingBlockAnim ballState.Position b.Position) |> ignore
            Sound.queueSoundStart "hit"
            (newActiveBlocks, {ballState with Velocity=resolvedVel; NumBounces=ballState.NumBounces+1} )


    let private incrementBallSpeed ballState =
        if ballState.NumBounces % speedIncreaseIncrement = 0 then
            {ballState with Velocity = {X=ballState.Velocity.X+ballSpeedMultiplier; Y=ballState.Velocity.Y+ballSpeedMultiplier}; NumBounces = ballState.NumBounces+1}
        else
            ballState


    let ballTick paddleState activeBlocks prevBallState =
        let boundaryResolvedVelocity = resolveBoundaryCollision prevBallState
        let (newPaddleState, paddleResolvedVelocity) = resolvePaddleCollision paddleState {prevBallState with Velocity = boundaryResolvedVelocity}
        let (newActiveBlocks ,collisionResolvedBallState) = resolveBlockCollision activeBlocks {prevBallState with Velocity = paddleResolvedVelocity}

        let speedAdjustedBallState = incrementBallSpeed collisionResolvedBallState

        Draw.queueSpriteUpdate speedAdjustedBallState.BallId
        (calcNewBallPos speedAdjustedBallState, newActiveBlocks, newPaddleState)