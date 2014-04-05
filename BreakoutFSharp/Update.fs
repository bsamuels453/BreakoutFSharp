namespace global

[<AutoOpen>]
module Update =
    open SFML.Window;

    let paddleTick (oldPaddleState:PaddleState) keyboardState=
        let restrictPaddlePos pos =
            if pos.X > (screenWidth - paddleWidth) then
                {X=screenWidth-paddleWidth; Y=pos.Y} 
            elif pos.X < 0.0f then
                {X=0.0f; Y=pos.Y} 
            else pos
            
        let aState = (getKeyState keyboardState Keyboard.Key.A).KeyState
        let dState = (getKeyState keyboardState Keyboard.Key.D).KeyState
        if aState=Pressed && dState=Pressed ||aState=Released && dState=Released then
            oldPaddleState.Position
        else
            queueSpriteUpdate oldPaddleState.PaddleId
            if aState=Pressed then
               restrictPaddlePos {X=oldPaddleState.Position.X - paddleSpeed; Y=oldPaddleState.Position.Y}
            else
               restrictPaddlePos {X=oldPaddleState.Position.X + paddleSpeed; Y=oldPaddleState.Position.Y}

    let calcNewBallPos (ballState:BallState) =
        let xPos = ballState.Position.X + ballState.Velocity.X
        let yPos = ballState.Position.Y + ballState.Velocity.Y
        {ballState with Position={X=xPos;Y=yPos}}

    let resolveBoundaryCollision ballState =
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
        {X=newVelX;Y=newVelY}

    let rectangleOverlap p1 d1 p2 d2 =
        not (p2.X > p1.X+d1.X || p2.X+d2.X < p1.X || p2.Y > p1.Y + d1.Y || p2.Y+d2.Y < p1.Y)

    let resolvePaddleCollision paddleState ballState =
        let desiredPosition = (calcNewBallPos ballState).Position
        let paddleDims = {X=paddleWidth; Y=paddleHeight}
        let ballDims = {X=ballWidth; Y=ballWidth}
        if rectangleOverlap paddleState paddleDims ballState.Position ballDims then
            {ballState.Velocity with Y=ballState.Velocity.Y * -1.0f}
        else
            ballState.Velocity

    let resolveBlockCollision activeBlocks ballState  =
        let desiredPosition = (calcNewBallPos ballState).Position
        let ballDims = {X=ballWidth; Y=ballWidth}
        let blockDims = {X=blockWidth; Y=blockHeight}
        let collideBlocks = activeBlocks |> List.filter (fun block -> 
            rectangleOverlap desiredPosition ballDims block.Position blockDims) 
        if collideBlocks.Length = 0 then
            (activeBlocks, ballState)
        else
            let getReflectionAxis block =
                let leftDist = abs <| (desiredPosition.X + ballWidth) - block.Position.X
                let rightDist = abs <| desiredPosition.X - (block.Position.X + blockWidth)
                let topDist = abs <| (desiredPosition.Y + ballWidth) - block.Position.Y
                let botDist = abs <| desiredPosition.Y - (block.Position.Y + blockHeight)

                if leftDist < topDist && leftDist < botDist then
                    {X=(-1.0f); Y=(1.0f)}
                elif rightDist < topDist && rightDist < botDist then
                    {X=(-1.0f); Y=(1.0f)}
                else
                    {X=(1.0f); Y=(-1.0f)}

            let reflectionAxis = 
                collideBlocks
                |> List.map getReflectionAxis 
                |> Seq.ofList 
                |> Seq.distinct
                |> Seq.reduce (fun v1 v2 -> {X=v1.X*v2.X; Y=v1.Y*v2.Y})

            let resolvedVel = {
                X=ballState.Velocity.X*reflectionAxis.X; 
                Y=ballState.Velocity.Y*reflectionAxis.Y}
            
            let newActiveBlocks =  
                activeBlocks
                |> List.filter (fun b -> not <| List.exists b.Equals collideBlocks)

            collideBlocks |> List.map (fun b -> queueSpriteDeletion b.BlockId) |> ignore
            (newActiveBlocks, {ballState with Velocity=resolvedVel; NumBounces=ballState.NumBounces+1} )

    let incrementBallSpeed ballState =
        if ballState.NumBounces % 5 = 0 then
            {ballState with Velocity = {X=ballState.Velocity.X*1.05f; Y=ballState.Velocity.Y*1.05f}; NumBounces = ballState.NumBounces+1}
        else
            ballState

    let ballTick paddleState activeBlocks prevBallState =
        let boundaryResolvedVelocity = resolveBoundaryCollision prevBallState
        let paddleResolvedVelocity = resolvePaddleCollision paddleState {prevBallState with Velocity = boundaryResolvedVelocity}
        let (newActiveBlocks ,collisionResolvedBallState) = resolveBlockCollision activeBlocks {prevBallState with Velocity = paddleResolvedVelocity}

        let speedAdjustedBallState = incrementBallSpeed collisionResolvedBallState

        queueSpriteUpdate speedAdjustedBallState.BallId
        (calcNewBallPos speedAdjustedBallState, newActiveBlocks)