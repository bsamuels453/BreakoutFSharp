namespace global

[<AutoOpen>]
module GameFunctions =
    let generatePaddleId = 
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            PaddleId(!count)
            )

    let generateBallId =
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            BallId(!count)
            )

    let generateBlockId =
        let count = ref 0;
        (fun () -> 
            count := !count + 1
            BlockId(!count)
            )