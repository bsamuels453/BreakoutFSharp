﻿namespace global

[<AutoOpen>]
module GameConsts =
    let paddleWidth = 110.0f
    let paddleHeight = 5.0f
    let ballWidth = 11.0f
    let screenWidth = 800.0f
    let screenHeight = 600.0f
    let paddleXAxis = 50.0f
    let numBlockCols = 14.0f
    let numBlockRows = 6.0f
    let blockHeight = 0.23f * screenHeight/numBlockRows
    let blockWidth = screenWidth/numBlockCols
    let paddleSpeed = 5.5f
    let initlBallSpeed = 4.0f
    let ballSpeedMultiplier = 0.3f
    let speedIncreaseIncrement = 5

    let blockXCoords = [for x in 0..(int numBlockCols-1) -> float32 x * blockWidth]
    let blockYCoords = [for y in 0..(int numBlockRows-1) -> float32 y * blockHeight]