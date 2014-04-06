namespace global

module Sound =
    open System.IO;
    open SFML.Audio;

    let mutable private soundsToProc:string list = []

    let queueSoundStart string =
        soundsToProc <- List.Cons (string, soundsToProc)

    let createSound buffers name =
        let buffer:SoundBuffer = Resource.extractResource buffers name
        new Sound(buffer)

    let updateSoundState gameState prevSoundState =
        let newSounds = 
            soundsToProc
            |> List.map (fun name -> createSound prevSoundState.SoundBuffers name)
        soundsToProc <- []

        newSounds |> List.map (fun s -> s.Play()) |> ignore

        let totalSounds = List.append prevSoundState.ActiveSounds newSounds

        let (playingSounds, endedSounds) = 
            totalSounds |> List.partition (fun s -> s.Status = SoundStatus.Playing)

        endedSounds |> List.map (fun s -> s.Dispose()) |> ignore

        {prevSoundState with ActiveSounds = playingSounds}

    let genDefaultSoundState() =
        {ActiveSounds=[]; SoundBuffers=Resource.loadSounds()}