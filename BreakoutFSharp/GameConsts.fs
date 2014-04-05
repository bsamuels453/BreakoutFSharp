namespace global

[<AutoOpen>]
module GameConsts =
    let paddleWidth = 100.0f
    let paddleHeight = 3.0f
    let ballWidth = 10.0f
    let screenWidth = 800.0f
    let screenHeight = 600.0f
    let paddleXAxis = 50.0f
    let numBlockCols = 14.0f
    let numBlockRows = 6.0f
    let blockHeight = 0.23f * screenHeight/numBlockRows
    let blockWidth = screenWidth/numBlockCols
    let paddleSpeed = 5.5f
    let initlBallSpeed = 2.80f

    let blockXCoords = [for x in 0..(int numBlockCols-1) -> float32 x * blockWidth]
    let blockYCoords = [for y in 0..(int numBlockRows-1) -> float32 y * blockHeight]