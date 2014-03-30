namespace global

[<AutoOpen>]
module GameConsts =
    let paddleWidth = 100.0f
    let paddleHeight = 3.0f
    let ballWidth = 10.0f
    let screenWidth = 800.0f
    let screenHeight = 600.0f
    let paddleXAxis = 50.0f
    let numBlockCols = 20.0f
    let numBlockRows = 6.0f
    let blockHeight = 0.23f * screenHeight/numBlockRows
    let blockWidth = screenWidth/numBlockCols
    let paddleSpeed = 0.20f
    let initlBallSpeed = 0.12f